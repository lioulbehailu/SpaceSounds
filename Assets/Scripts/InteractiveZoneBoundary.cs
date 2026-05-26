using UnityEngine;

public class InteractiveZoneBoundary : MonoBehaviour
{
    public Transform xrOrigin;    // drag XR Origin root here
    public Transform hmdCamera;   // drag Main Camera here

    // Define your room bounds manually
    // Adjust these to match exactly where your walls are in the scene
    public Vector3 roomMin = new Vector3(-5f, 0f, -2.5f);
    public Vector3 roomMax = new Vector3(5f, 4f, 2.5f);

    private void Update()
    {
        Vector3 hmdPos = hmdCamera.position;

        if (!IsInsideBounds(hmdPos))
        {
            Vector3 clampedHMD = new Vector3(
                Mathf.Clamp(hmdPos.x, roomMin.x, roomMax.x),
                Mathf.Clamp(hmdPos.y, roomMin.y, roomMax.y),
                Mathf.Clamp(hmdPos.z, roomMin.z, roomMax.z)
            );

            Vector3 correction = clampedHMD - hmdPos;
            xrOrigin.position += correction;
        }
    }

    private bool IsInsideBounds(Vector3 pos)
    {
        return pos.x >= roomMin.x && pos.x <= roomMax.x &&
               pos.y >= roomMin.y && pos.y <= roomMax.y &&
               pos.z >= roomMin.z && pos.z <= roomMax.z;
    }
}