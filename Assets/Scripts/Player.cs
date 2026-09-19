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
    private CapsuleCollider capsuleCollider;
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
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            // CharacterController already provides a capsule. A second collider on the
            // player makes planet-down ground probes jitter, especially on sloped edges.
            capsuleCollider.enabled = false;
        }

        inputManager = InputManager.Instance;
        planet = PlanetGravity.Instance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        smoothNormal = PlanetMotor.SurfaceUp(transform.position, planet.transform.position);
    }

    void Update()
    {
        Vector3 planetCenter = planet.transform.position;
        Vector3 upDirection = PlanetMotor.SurfaceUp(transform.position, planetCenter);
        smoothNormal = Vector3.Slerp(smoothNormal, upDirection, Time.deltaTime * 10f).normalized;
        Vector3 up = smoothNormal;

        groundedPlayer = ProbeGround(up, out RaycastHit groundHit);

        Vector2 input = inputManager.GetPlayerMovement();
        Vector3 move = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

        if (move.magnitude > 0)
        {
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", true);
        }
        else
        {
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", false);
        }

        Vector3 inputRelative = transform.TransformDirection(new Vector3(move.x, 0f, move.z));
        Vector3 moveDirection = PlanetMotor.TangentMove(inputRelative, up, playerSpeed);

        float radialVelocity = Vector3.Dot(playerVelocity, up);
        if (groundedPlayer && radialVelocity < 0f)
        {
            radialVelocity = -1f;
        }

        if (inputManager.GetJump() && groundedPlayer)
        {
            radialVelocity = Mathf.Sqrt(jumpHeight * 2f * -gravityValue);
        }

        radialVelocity += gravityValue * Time.deltaTime;
        playerVelocity = up * radialVelocity;

        Vector3 finalMove = moveDirection + playerVelocity;
        if (groundedPlayer && radialVelocity <= 0f)
        {
            Vector3 standPoint = groundHit.point + up * controller.skinWidth;
            Vector3 snap = Vector3.Project(standPoint - transform.position, up);
            if (snap.sqrMagnitude < 0.25f)
            {
                finalMove += snap / Mathf.Max(Time.deltaTime, 0.0001f);
            }
        }

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

    private bool ProbeGround(Vector3 up, out RaycastHit groundHit)
    {
        groundHit = default;
        float radius = Mathf.Max(0.05f, controller.radius * 0.9f);
        float height = Mathf.Max(controller.height, radius * 2f);
        Vector3 origin = transform.position + up * (height * 0.5f);
        float castDist = height * 0.5f + controller.skinWidth + 0.25f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, -up, castDist, ~0, QueryTriggerInteraction.Ignore);

        float best = float.MaxValue;
        bool found = false;
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (PlanetMotor.IsSelfHit(transform, hit.transform))
            {
                continue;
            }

            if (hit.distance < best)
            {
                best = hit.distance;
                groundHit = hit;
                found = true;
            }
        }

        if (!found && Physics.Raycast(origin, -up, out RaycastHit rayHit, castDist, ~0, QueryTriggerInteraction.Ignore)
            && !PlanetMotor.IsSelfHit(transform, rayHit.transform))
        {
            groundHit = rayHit;
            found = true;
        }

        return found;
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
            Collectible collectible = Collectible.FindFromHit(hit.transform);
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
        if(groundedPlayer == false) return;
        footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        footstepAudioSource.Play();
    }
    private void LateUpdate()
    {
        Vector2 delta = InputManager.Instance.GetMouseDelta();
        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        if (invertY) mouseY = -mouseY;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Vector3 planetCenter = planet.transform.position;
        Vector3 upDirection = PlanetMotor.SurfaceUp(transform.position, planetCenter);

        transform.rotation = PlanetMotor.AlignToUp(transform.rotation, upDirection);
        if (Mathf.Abs(mouseX) > 0.0001f)
        {
            transform.Rotate(upDirection, mouseX, Space.World);
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        if (head != null)
        {
            Vector3 headEuler = head.localEulerAngles;
            head.SetLocalPositionAndRotation(new Vector3(0, 0.78f, 0.09f), Quaternion.Euler(pitch, headEuler.y, 0f));
        }

        if (AstronautBody != null)
        {
            AstronautBody.transform.localPosition = Vector3.zero;
        }

        if (head == null || LeftUpperArm == null || RightUpperArm == null)
        {
            return;
        }

        float headPitch = head.localEulerAngles.x;
        if (headPitch > 180f) headPitch -= 360f;
        this.cameraPitch = headPitch / 180f;

        leftHandYaw = LeftUpperArm.transform.localRotation.y - this.cameraPitch;
        rightHandYaw = RightUpperArm.transform.localRotation.y + this.cameraPitch;

        leftHandPitch = LeftUpperArm.transform.localRotation.x - this.cameraPitch * 0.5f + 0.1f;
        rightHandPitch = RightUpperArm.transform.localRotation.x - this.cameraPitch * 0.5f + 0.1f;

        LeftUpperArm.transform.localRotation = new Quaternion(leftHandPitch, leftHandYaw, LeftUpperArm.transform.localRotation.z, LeftUpperArm.transform.localRotation.w);
        RightUpperArm.transform.localRotation = new Quaternion(rightHandPitch, rightHandYaw, RightUpperArm.transform.localRotation.z, RightUpperArm.transform.localRotation.w);

        float debugLength = 2f;
        Debug.DrawRay(transform.position, upDirection * debugLength, Color.green);
        Debug.DrawRay(transform.position, transform.forward * debugLength, Color.blue);
        Debug.DrawRay(transform.position, transform.right * debugLength, Color.red);
    }
}
