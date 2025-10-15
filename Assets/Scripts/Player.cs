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

    private CharacterController controller;
    private InputManager inputManager;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]
    private GameObject UIObject;
    [SerializeField]
    private GameObject InGameObject;

    [SerializeField]
    private GameObject LeftHand;
    [SerializeField]
    private GameObject RightHand;

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
            UIObject.GetComponent<Animator>().SetBool("IsWalking", true);
            InGameObject.GetComponent<Animator>().SetBool("IsWalking", true);
        }
        else
        {
            UIObject.GetComponent<Animator>().SetBool("IsWalking", false);
            InGameObject.GetComponent<Animator>().SetBool("IsWalking", false);
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
    }
    private void LateUpdate()
    {
        // Convert quaternion to euler angles for consistent pitch reading
        float pitch = mainCamera.transform.localEulerAngles.x;
        // Normalize pitch to -180 to 180 range
        if (pitch > 180f) pitch -= 360f;
        // Convert -180 to 180 range to -1 to 1 range
        cameraPitch = pitch / 180f;
        Debug.Log("Camera Pitch (-1 to 1): " + cameraPitch);

        leftHandYaw = LeftHand.transform.localRotation.y - cameraPitch;
        rightHandYaw = RightHand.transform.localRotation.y + cameraPitch;
    
        leftHandPitch = LeftHand.transform.localRotation.x - cameraPitch * 0.5f + 0.1f;
        rightHandPitch = RightHand.transform.localRotation.x - cameraPitch * 0.5f + 0.1f;

        Debug.Log("Camera Pitch: " + mainCamera.transform.eulerAngles.x);
        Debug.Log("Left Hand Yaw: " + leftHandYaw);
        Debug.Log("Right Hand Yaw: " + rightHandYaw);
        LeftHand.transform.localRotation = new Quaternion(leftHandPitch, leftHandYaw, LeftHand.transform.localRotation.z, LeftHand.transform.localRotation.w);
        RightHand.transform.localRotation = new Quaternion(rightHandPitch, rightHandYaw, RightHand.transform.localRotation.z, RightHand.transform.localRotation.w);
    }
}
