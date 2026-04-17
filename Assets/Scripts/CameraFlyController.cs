using UnityEngine;
using UnityEngine.InputSystem; // You must include this namespace

public class CameraFlyController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    void Update()
    {
        // 1. Movement using the new Input System
        Vector3 move = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) move.z += 1;
        if (Keyboard.current.sKey.isPressed) move.z -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        transform.position += transform.rotation * move * moveSpeed * Time.deltaTime;

        // 2. Rotation
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            transform.Rotate(-delta.y * lookSpeed * 0.1f, delta.x * lookSpeed * 0.1f, 0, Space.Self);
        }
    }
}