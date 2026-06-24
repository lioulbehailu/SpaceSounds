using UnityEngine;

public class SatelliteOnOrbit : MonoBehaviour
{
    public OrbitManager orbitPath;

    private float timeOffset = 0f;
    private bool isThrown = false;
    private Rigidbody rb;
    [SerializeField] private float pathSnapDistance = 0.5f;
    [SerializeField] private float reentryCheckInterval = 0.1f; // don't check every single frame, throttle it
    public float GetSnapDistance() => pathSnapDistance;

    private float reentryCheckTimer = 0f;

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
        reentryCheckTimer = 0f;
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
        // Orbiting takes priority and short-circuits everything else
        if (orbitPath != null)
        {
            if (!isThrown)
                transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
            return;
        }

        // While thrown and not yet docked anywhere, continuously check for path reentry
        if (isThrown)
        {
            reentryCheckTimer += Time.deltaTime;
            if (reentryCheckTimer >= reentryCheckInterval)
            {
                reentryCheckTimer = 0f;
                CheckForPathReentry();
            }
        }
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

    private void CheckForPathReentry()
    {
        SatelliteFlightPath[] allPaths = FindObjectsByType<SatelliteFlightPath>(FindObjectsSortMode.None);

        SatelliteFlightPath bestPath = null;
        int bestWaypoint = 0;
        float bestT = 0f;
        float bestDist = float.MaxValue;

        foreach (var path in allPaths)
        {
            for (int i = 0; i < path.GetWaypointCount() - 1; i++)
            {
                Vector3 a = path.GetPosition(i, 0f);
                Vector3 b = path.GetPosition(i, 1f);

                Vector3 closestPoint = ClosestPointOnSegment(transform.position, a, b);
                float dist = Vector3.Distance(transform.position, closestPoint);

                if (dist < bestDist && dist <= pathSnapDistance)
                {
                    bestDist = dist;
                    bestPath = path;
                    bestWaypoint = i;
                    bestT = InverseLerp3D(a, b, closestPoint);
                }
            }
        }

        if (bestPath != null)
        {
            Debug.Log(gameObject.name + " reentered flight path: " + bestPath.gameObject.name);

            SatelliteFlightMover mover = GetComponent<SatelliteFlightMover>();
            if (mover == null) mover = gameObject.AddComponent<SatelliteFlightMover>();

            isThrown = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            mover.ResumeFlight(bestPath, bestWaypoint, bestT);
        }
    }

    private Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    private float InverseLerp3D(Vector3 a, Vector3 b, Vector3 point)
    {
        return Vector3.Distance(a, point) / Vector3.Distance(a, b);
    }
}
