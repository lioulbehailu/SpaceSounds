using UnityEngine;

public class SatelliteOnOrbit : MonoBehaviour
{
    // script to place staellite in correct place and make it spin

    public OrbitManager orbitPath;

    private float timeOffset = 0f; // Shifts the orbit start so we begin at the nearest point
    private bool isThrown = false;
    private Rigidbody rb;

    #region snap current satellite to closest point in orbit
    public void SnapToNearestPoint()
    {
        if (orbitPath == null) return;

        isThrown = false; // stop physics, start orbiting

        // Binary search or sample the orbit to find the time value whose
        // position is closest to where the satellite currently is
        float bestTime = 0f;
        float bestDist = float.MaxValue;

        // Sample the orbit in small steps to find the closest point
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

        // Offset so that Time.time + timeOffset == bestTime at this moment
        timeOffset = bestTime - Time.time;

        // Stop physics movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
    #endregion

    #region move stellite on orbit
    void Update()
    {
        if (orbitPath == null) return;
        if (isThrown) return; // let physics handle movement while thrown

        transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
    }
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called by SpaceInteractionXR when satellite is released
    public void OnThrown()
    {
        isThrown = true;
        orbitPath = null;
    }

    // Triggered when satellite physically enters a SnapZone trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Only react if currently thrown (not grabbed, not already orbiting)
        if (!isThrown) return;
        if (orbitPath != null) return;

        if (other.CompareTag("SnapZone"))
        {
            Debug.Log(gameObject.name + " entered SnapZone after throw — snapping to orbit.");

            OrbitManager manager = other.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                orbitPath = manager;
                SnapToNearestPoint();
            }
        }
    }
}
