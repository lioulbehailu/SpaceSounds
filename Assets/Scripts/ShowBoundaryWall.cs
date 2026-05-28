using UnityEngine;

public class BoundaryWall : MonoBehaviour
{
    public Renderer wallRenderer; // drag Wall_Front_Mesh here

    private Material wallMaterial;

    private void Start()
    {
        if (wallRenderer == null)
        {
            Debug.LogError("wallRenderer not assigned on: " + gameObject.name);
            return;
        }

        // IMPORTANT: creates a unique material instance so walls
        // don't all change together
        wallMaterial = wallRenderer.material;
        SetWallVisible(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.CompareTag("MainCamera"))
            SetWallVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited by: " + other.gameObject.name);
        if (other.CompareTag("MainCamera"))
            SetWallVisible(false);
    }

    private void SetWallVisible(bool visible)
    {
        if (wallMaterial == null) return;

        Color c = wallMaterial.color;
        c.a = visible ? 0.3f : 0f;
        wallMaterial.color = c;

        Debug.Log(gameObject.name + " wall visible: " + visible);
    }
}