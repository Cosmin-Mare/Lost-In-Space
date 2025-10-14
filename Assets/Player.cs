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

        if(move.magnitude > 0)
        {
            UIObject.GetComponent<Animator>().SetBool("IsWalking", true);
            InGameObject.GetComponent<Animator>().SetBool("IsWalking", true);
        } else
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
}
