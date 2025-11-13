using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    private float gravityValue = -9.81f;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [SerializeField]
    private AudioSource pickupAudioSource;

    private CharacterController controller;
    private InputManager inputManager;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]
    private GameObject AstronautBody;

    [SerializeField]
    private GameObject LeftUpperArm;
    [SerializeField]
    private GameObject RightUpperArm;
    [SerializeField]
    private AudioSource footstepAudioSource;
    private PlanetGravity planet;

    private float leftHandYaw = 0f;
    private float rightHandYaw = 0f;

    private float leftHandPitch = 0f;
    private float rightHandPitch = 0f;
    private float cameraPitch = 0f;

    //Camera
    [SerializeField]
    private Transform head; // assign the player's head (empty) in the Inspector

    [Header("Mouse Look")]
    [SerializeField]
    private float mouseSensitivity = 1f;
    [SerializeField]
    private bool invertY = false;
    [SerializeField]
    private float maxPitch = 80f;
    private float pitch = 0f;
    private float yaw = 0f;

    private Vector3 smoothNormal;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        planet = PlanetGravity.Instance;

        //Camera
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        smoothNormal = (transform.position - planet.transform.position).normalized;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // --- Get Input ---
        Vector2 input = inputManager.GetPlayerMovement();
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        if (move.magnitude > 0)
        {
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", true);
        }
        else
        {
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", false);
        }

        // --- 1. Radial direction from planet center ---
        Vector3 upDirection = (transform.position - planet.transform.position).normalized;
        float radialVelocity;
        Vector3 moveDirection;

        // Smooth transition between old and new normals
        float smoothFactor = 10f; // adjust between 5–15 depending on how much smoothing you want
        smoothNormal = Vector3.Slerp(smoothNormal, upDirection, Time.deltaTime * smoothFactor);

        // --- 3. Get input relative to player rotation ---
        Vector3 moveInput = new Vector3(move.x, 0, move.z);
        Vector3 inputRelative = transform.TransformDirection(moveInput);

        // --- 4. Project that input onto the smoothed normal ---
        moveDirection = Vector3.ProjectOnPlane(inputRelative, smoothNormal).normalized * playerSpeed;

        // --- 5. Apply radial velocity (gravity + jump) ---
        radialVelocity = Vector3.Dot(playerVelocity, smoothNormal);
        if (inputManager.GetJump() && groundedPlayer)
        {
            radialVelocity = Mathf.Sqrt(jumpHeight * 2f * -gravityValue);
        }

        radialVelocity += gravityValue * Time.deltaTime; // gravity toward planet center
        
        playerVelocity = upDirection * radialVelocity;

        // --- 6. Combine and move ---
        Vector3 finalMove = moveDirection + playerVelocity;
        controller.Move(finalMove * Time.deltaTime);



        if (inputManager.GetFire())
        {
            Debug.Log("Mine");
            AstronautBody.GetComponent<Animator>().SetTrigger("Mine");
        }
        if (inputManager.GetPickup())
        {
            AstronautBody.GetComponent<Animator>().SetTrigger("PickUp");
        }
        if (inputManager.GetInteract())
        {
            AstronautBody.GetComponent<Animator>().SetTrigger("Interact");
        }
    }

    public void OnMiningAnimationHit()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found for mining raycast.");
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float maxDistance = 15f;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Check both the hit object and its parents for the Collectible component
            Collectible collectible = hit.transform.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = hit.transform.GetComponentInParent<Collectible>();
            }

            if (collectible != null)
            {
                collectible.MineResource();
            }
            else
            {
                Debug.Log($"Raycast hit: {hit.transform.name}, but it's not part of a collectible resource.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
        }
    }

    public void OnPickupAnimationHit()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found for pickup raycast.");
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float maxDistance = 5f;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Check both the hit object and its parents for the Collectible component
            PickableItem item = hit.transform.GetComponent<PickableItem>();
            if (item == null)
            {
                item = hit.transform.GetComponentInParent<PickableItem>();
            }

            if (item != null)
            {
                pickupAudioSource.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                pickupAudioSource.Play();
                item.Pickup();
            }
            else
            {
                Debug.Log($"Raycast hit: {hit.transform.name}, but it's not part of a collectible resource.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
        }
    }

    public void OnInteractAnimationHit()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found for interact raycast.");
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        float maxDistance = 5f;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Interactable interactable = hit.transform.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = hit.transform.GetComponentInParent<Interactable>();
            }

            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log($"Raycast hit: {hit.transform.name}, but it's not interactable.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
        }
    }
    
    public void OnFootstepAnimationHit()
    {
        if(controller.isGrounded == false) return;
        footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        footstepAudioSource.Play();
    }
    private void LateUpdate()
    {
        //Camera controls and orientation

        Vector2 delta = InputManager.Instance.GetMouseDelta();
        // Assumes InputManager returns raw delta (mouse delta or stick delta). Scale by sensitivity.
        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        if (invertY) mouseY = -mouseY;

        yaw += mouseX;
        pitch -= mouseY; // subtract so that moving mouse up looks up

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Vector3 headEuler = head.localEulerAngles;
        
        // Orientation
        mainCamera.transform.localPosition = new Vector3(0, 0, 0);
        mainCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);

        Vector3 planetCenter = planet.transform.position;
        Vector3 objectPosition = transform.position;

        // --- 1. Surface normal at the player's position ---
        Vector3 upDirection = (objectPosition - planetCenter).normalized;

        // --- 2. Calculate tangent forward direction ---
        Vector3 forward = Vector3.Cross(transform.right, upDirection).normalized;

        // --- 4. Rotate forward vector around surface normal (yaw only) ---
        forward = Quaternion.AngleAxis(mouseX, upDirection) * forward;

        // --- 5. Recompute right to stay perpendicular ---
        Vector3 right = Vector3.Cross(forward, upDirection).normalized;

        // --- 6. Build rotation ---
        Quaternion targetRotation = Quaternion.LookRotation(forward, upDirection);
        transform.rotation = targetRotation;

        // --- 7. Debug draw axes ---
        float debugLength = 2f;
        Debug.DrawRay(transform.position, upDirection * debugLength, Color.green);   // up axis
        Debug.DrawRay(transform.position, forward * debugLength, Color.blue);        // forward axis
        Debug.DrawRay(transform.position, right * debugLength, Color.red);           // right axis

        // transform.rotation = Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z);
        head.SetLocalPositionAndRotation(new Vector3(0, 0.78f, 0.09f), Quaternion.Euler(pitch, headEuler.y, headEuler.z));
        AstronautBody.transform.localPosition = new Vector3(0, 0, 0);

        // Convert quaternion to euler angles for consistent pitch reading
        float headPitch = head.localEulerAngles.x;
        // Normalize pitch to -180 to 180 range
        if (headPitch > 180f) headPitch -= 360f;
        // Convert -180 to 180 range to -1 to 1 range
        this.cameraPitch = headPitch / 180f;

        leftHandYaw = LeftUpperArm.transform.localRotation.y - this.cameraPitch;
        rightHandYaw = RightUpperArm.transform.localRotation.y + this.cameraPitch;

        leftHandPitch = LeftUpperArm.transform.localRotation.x - this.cameraPitch * 0.5f + 0.1f;
        rightHandPitch = RightUpperArm.transform.localRotation.x - this.cameraPitch * 0.5f + 0.1f;

        LeftUpperArm.transform.localRotation = new Quaternion(leftHandPitch, leftHandYaw, LeftUpperArm.transform.localRotation.z, LeftUpperArm.transform.localRotation.w);
        RightUpperArm.transform.localRotation = new Quaternion(rightHandPitch, rightHandYaw, RightUpperArm.transform.localRotation.z, RightUpperArm.transform.localRotation.w);
    }
}
