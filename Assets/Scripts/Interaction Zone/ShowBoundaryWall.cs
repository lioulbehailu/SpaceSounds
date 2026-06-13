using UnityEngine;

public class BoundaryWall : MonoBehaviour
{
    public Transform hmdCamera;         // drag Main Camera here
    public Renderer wallRenderer;       // drag the Quad MeshRenderer here
    public float warningDistance = 0.5f;

    private Material wallMaterial;
    private BoxCollider triggerCollider;

    private void Start()
    {
        // Get the trigger collider on THIS object (Wall_X_Trigger)
        triggerCollider = GetComponent<BoxCollider>();

        if (wallRenderer == null)
        {
            Debug.LogError("wallRenderer not assigned on: " + gameObject.name);
            return;
        }

        if (triggerCollider == null)
        {
            Debug.LogError("No BoxCollider found on: " + gameObject.name);
            return;
        }

        wallMaterial = wallRenderer.material;
        SetWallVisible(false);
    }

    private void Update()
    {
        if (hmdCamera == null || triggerCollider == null || wallMaterial == null)
            return;

        // Get the closest point on THIS wall's collider to the HMD
        Vector3 closestPoint = triggerCollider.ClosestPoint(hmdCamera.position);
        float distanceToWall = Vector3.Distance(hmdCamera.position, closestPoint);

        if (distanceToWall <= warningDistance)
            SetWallVisible(true);
        else
            SetWallVisible(false);
    }

    private void SetWallVisible(bool visible)
    {
        Color c = wallMaterial.color;
        c.a = visible ? 0.9f : 0f;

        wallMaterial.color = c;
        wallMaterial.SetColor("_BaseColor", c);
    }
}
