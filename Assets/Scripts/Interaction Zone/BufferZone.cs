using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Match XRI 3.0

public class BufferZone : MonoBehaviour
{
    private BoxCollider zoneCollider;

    void Awake()
    {
        // Get the cube's own collider data to check physical boundaries
        zoneCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerExit(Collider other)
    {
        // 1. Strict Tag Check
        if (!other.CompareTag("Satellite")) return;

        // 2. Safety Check: Is the player currently holding it?
        if (other.TryGetComponent<XRGrabInteractable>(out var grabComponent))
        {
            if (grabComponent.isSelected)
            {
                Debug.Log($"[BufferZone] Ignored: Player is actively holding {other.name}.");
                return;
            }
        }

        // 3. Safety Check: Has it been attached/nested under a socket or another system?
        if (other.transform.parent != null)
        {
            Debug.Log($"[BufferZone] Ignored: {other.name} is attached to a structural parent.");
            return;
        }

        // 4. Mathematical Boundary Double-Check
        // This confirms if the center of the satellite is genuinely outside the cube's box space
        if (zoneCollider != null)
        {
            Vector3 satellitePos = other.transform.position;
            // Bounds.Contains returns true if the position is inside the box volume
            if (zoneCollider.bounds.Contains(satellitePos))
            {
                Debug.Log($"[BufferZone] Protected: Unity fired an Exit event, but {other.name} is physically still inside the cube walls. Preventing destruction.");
                return;
            }
        }

        Debug.Log($"[BufferZone] Success: {other.name} left the airspace unattached. Destroying.");
        Destroy(other.gameObject);
    }

    void OnDrawGizmos()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.center, col.size);
    }
}
