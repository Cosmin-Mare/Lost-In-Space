using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField]
    private Transform head; // assign the player's head (empty) in the Inspector

    [Header("Mouse Look")]
    [SerializeField]
    private float mouseSensitivity = 1f;
    [SerializeField]
    private bool usePlayerInputManager = true; // read mouse from InputManager (new Input System) when available
    [SerializeField]
    private bool invertY = false;
    [SerializeField]
    private float maxPitch = 85f;

    private float pitch = 0f;
    private float yaw = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (head == null)
        {
            Debug.LogWarning("FirstPersonCamera: Head transform is not assigned. Camera will follow this GameObject's transform instead.");
            head = this.transform;
        }

        // initialize yaw/pitch from current rotations
        yaw = head.localEulerAngles.y;
        pitch = transform.localEulerAngles.x;

        // lock cursor for first-person control by default
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Use LateUpdate so camera follows after player movement
    void LateUpdate()
    {
        // read mouse input
        float mouseX;
        float mouseY;

        if (usePlayerInputManager && InputManager.Instance != null)
        {
            Vector2 delta = InputManager.Instance.GetMouseDelta();
            // Assumes InputManager returns raw delta (mouse delta or stick delta). Scale by sensitivity.
            mouseX = delta.x * mouseSensitivity;
            mouseY = delta.y * mouseSensitivity;
        }
        else
        {
            // legacy input: "Mouse X"/"Mouse Y" are per-frame axis values, scale with Time.deltaTime to match sensitivity
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }

        if (invertY) mouseY = -mouseY;

        yaw += mouseX;
        pitch -= mouseY; // subtract so that moving mouse up looks up

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        // apply yaw to head (rotate around Y)
        Vector3 headEuler = head.localEulerAngles;
        headEuler.y = yaw;
        head.localEulerAngles = headEuler;

        // match camera position to the head
        transform.position = head.position;

        // apply combined rotation: pitch (x) and yaw (y)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Toggle cursor lock from other scripts (e.g. UI)
    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnDisable()
    {
        // ensure cursor unlocked when script is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
