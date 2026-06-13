using UnityEngine;

public class SatelliteOnOrbit : MonoBehaviour
{
    public OrbitManager orbitPath;

    private float timeOffset = 0f;
    private bool isThrown = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called the exact moment the player grabs the satellite
    public void OnGrabbed()
    {
        isThrown = false; // Stop tracking as a thrown projectile
        orbitPath = null; // Clear old orbits while holding it
    }

    // Called by SpaceInteractionXR when satellite is released/tossed
    public void OnThrown()
    {
        isThrown = true;
        orbitPath = null;
    }

    #region snap current satellite to closest point in orbit
    public void SnapToNearestPoint()
    {
        if (orbitPath == null) return;

        isThrown = false; // Successfully docked! Stop tracking as a thrown projectile

        float bestTime = 0f;
        float bestDist = float.MaxValue;

        int samples = 360;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples * orbitPath.orbitalPeriod;
            Vector3 pos = orbitPath.GetPositionAtTime(t);
            float dist = Vector3.Distance(transform.position, pos);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestTime = t;
            }
        }

        timeOffset = bestTime - Time.time;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
    #endregion

    void Update()
    {
        if (orbitPath == null) return;
        if (isThrown) return; // Let physics engine control it through the air

        transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
    }

    // Triggered when the flying satellite hits a SnapZone trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // CRITICAL: If we are still holding it, isThrown is false, so it will ignore this completely!
        if (!isThrown) return;
        if (orbitPath != null) return;

        if (other.CompareTag("SnapZone"))
        {
            Debug.Log(gameObject.name + " caught mid-air by SnapZone — snapping to orbit.");

            OrbitManager manager = other.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                orbitPath = manager;
                SnapToNearestPoint();
            }
        }
    }
}
