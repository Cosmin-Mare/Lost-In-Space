using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [SerializeField]
    private AudioSource pickupAudioSource;

    private CharacterController controller;
    private InputManager inputManager;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]
    private GameObject AstronautHead;
    [SerializeField]
    private GameObject AstronautBody;

    [SerializeField]
    private GameObject LeftHand;
    [SerializeField]
    private GameObject RightHand;
    [SerializeField]
    private AudioSource footstepAudioSource;

    private float leftHandYaw = 0f;
    private float rightHandYaw = 0f;

    private float leftHandPitch = 0f;
    private float rightHandPitch = 0f;
    private float cameraPitch = 0f;
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
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

        // --- Convert to camera space movement ---
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Remove vertical tilt from camera direction
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        if (move.magnitude > 0)
        {
            AstronautHead.GetComponent<Animator>().SetBool("IsWalking", true);
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", true);
        }
        else
        {
            AstronautHead.GetComponent<Animator>().SetBool("IsWalking", false);
            AstronautBody.GetComponent<Animator>().SetBool("IsWalking", false);
        }

        // Final movement direction relative to camera
        Vector3 moveDirection = (cameraForward * move.z + cameraRight * move.x).normalized;

        transform.forward = cameraForward;

        // --- Jump ---
        if (inputManager.GetJump() && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        // --- Apply gravity ---
        playerVelocity.y += gravityValue * Time.deltaTime;

        // --- Apply movement ---
        Vector3 finalMove = (moveDirection * playerSpeed + Vector3.up * playerVelocity.y);
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
        if(inputManager.GetInteract())
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
        // Convert quaternion to euler angles for consistent pitch reading
        float pitch = mainCamera.transform.localEulerAngles.x;
        // Normalize pitch to -180 to 180 range
        if (pitch > 180f) pitch -= 360f;
        // Convert -180 to 180 range to -1 to 1 range
        cameraPitch = pitch / 180f;

        leftHandYaw = LeftHand.transform.localRotation.y - cameraPitch;
        rightHandYaw = RightHand.transform.localRotation.y + cameraPitch;
    
        leftHandPitch = LeftHand.transform.localRotation.x - cameraPitch * 0.5f + 0.1f;
        rightHandPitch = RightHand.transform.localRotation.x - cameraPitch * 0.5f + 0.1f;

        LeftHand.transform.localRotation = new Quaternion(leftHandPitch, leftHandYaw, LeftHand.transform.localRotation.z, LeftHand.transform.localRotation.w);
        RightHand.transform.localRotation = new Quaternion(rightHandPitch, rightHandYaw, RightHand.transform.localRotation.z, RightHand.transform.localRotation.w);
    }
}
