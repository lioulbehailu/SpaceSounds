using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookCamera : MonoBehaviour
{
    [Header("Look Settings")]
    // How fast the camera rotates relative to mouse movement (lower = slower)
    [SerializeField] private float sensitivity = 0.12f;

    // Vertical rotation limits to prevent flipping upside down
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Fly Settings")]
    // How fast the camera moves through the scene
    public float moveSpeed;

    // Current horizontal rotation (left/right) in degrees
    private float yaw;
    // Current vertical rotation (up/down) in degrees
    private float pitch;

    void Start()
    {
        // Initialize yaw and pitch from the camera's starting rotation in the scene
        Vector3 startRotation = transform.eulerAngles;
        yaw = startRotation.y;
        pitch = startRotation.x;

        // Lock the cursor to the center of the screen for FPS-style look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Safety check: do nothing if no mouse or keyboard is connected
        if (Mouse.current == null || Keyboard.current == null) return;

        HandleMovement();
        HandleLook();
        HandleCursorToggle();
    }

    void HandleMovement()
    {
        // Build a movement direction from WASD keys
        Vector3 move = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) move.z += 1;
        if (Keyboard.current.sKey.isPressed) move.z -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        // Move relative to the camera's current facing direction
        transform.position += transform.rotation * move * moveSpeed * Time.deltaTime;
    }

    void HandleLook()
    {
        // Read how far the mouse moved this frame (x = horizontal, y = vertical)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Moving the mouse right increases yaw (rotates camera right)
        yaw += mouseDelta.x * sensitivity;

        // Moving the mouse up decreases pitch (rotates camera up); subtracted to avoid inverted look
        pitch -= mouseDelta.y * sensitivity;

        // Clamp pitch so the camera can't rotate past straight up or straight down
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply the combined rotation to the camera (no roll/z-tilt)
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleCursorToggle()
    {
        // Press Escape to unlock and show the cursor again (e.g. to access menus)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}