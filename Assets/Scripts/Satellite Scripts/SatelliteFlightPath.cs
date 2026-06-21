using UnityEngine;

public class SatelliteFlightPath : MonoBehaviour
{
    [Header("Path Waypoints")]
    [Tooltip("Drag empty GameObjects marking the path, in order, from spawn to despawn")]
    public Transform[] waypoints;

    [Header("Spawn Settings")]
    public GameObject satellitePrefab;
    public float spawnIntervalMin = 3f;
    public float spawnIntervalMax = 6f;
    private float spawnInterval;
    public float travelSpeed = 1.5f;       // units per second

    private float spawnTimer = 0f;

    void Start()
    {
        PrewarmSatellites();
    }

    private void PrewarmSatellites()
    {
        if (satellitePrefab == null || waypoints.Length < 2) return;

        float totalLength = GetTotalLength();
        float distanceTravelled = 0f;

        while (true)
        {
            distanceTravelled += Random.Range(spawnIntervalMin, spawnIntervalMax) * travelSpeed;
            if (distanceTravelled > totalLength) break;

            float accumulated = 0f;
            int waypointIndex = 0;
            float progress = 0f;

            for (int w = 0; w < waypoints.Length - 1; w++)
            {
                float segLen = GetSegmentLength(w);
                if (accumulated + segLen >= distanceTravelled)
                {
                    waypointIndex = w;
                    progress = (distanceTravelled - accumulated) / segLen;
                    break;
                }
                accumulated += segLen;
            }

            GameObject sat = Instantiate(satellitePrefab, GetPosition(waypointIndex, progress), Random.rotation);
            SatelliteFlightMover mover = sat.GetComponent<SatelliteFlightMover>();
            if (mover == null)
                mover = sat.AddComponent<SatelliteFlightMover>();
            mover.Initialize(this, waypointIndex, progress);
        }

        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            spawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            SpawnSatellite();
        }
    }

    private void SpawnSatellite()
    {
        if (satellitePrefab == null || waypoints.Length < 2) return;

        GameObject sat = Instantiate(satellitePrefab, waypoints[0].position, Random.rotation);

        SatelliteFlightMover mover = sat.GetComponent<SatelliteFlightMover>();
        if (mover == null)
            mover = sat.AddComponent<SatelliteFlightMover>();

        mover.Initialize(this, 0, 0f);
    }

    public Vector3 GetPosition(int waypointIndex, float t)
    {
        waypointIndex = Mathf.Clamp(waypointIndex, 0, waypoints.Length - 2);
        return Vector3.Lerp(
            waypoints[waypointIndex].position,
            waypoints[waypointIndex + 1].position,
            t);
    }

    public float GetSegmentLength(int waypointIndex)
    {
        waypointIndex = Mathf.Clamp(waypointIndex, 0, waypoints.Length - 2);
        return Vector3.Distance(waypoints[waypointIndex].position, waypoints[waypointIndex + 1].position);
    }

    public int GetWaypointCount() => waypoints.Length;
    public float GetTravelSpeed() => travelSpeed;

    public void DespawnSatellite(GameObject sat)
    {
        Destroy(sat);
    }

    public float GetTotalLength()
    {
        float total = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
            total += GetSegmentLength(i);
        return total;
    }

    public Vector3 GetPathDirection()
    {
        if (waypoints.Length < 2) return Vector3.forward;
        return (waypoints[waypoints.Length - 1].position - waypoints[0].position).normalized;
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawWireSphere(waypoints[i].position, 0.1f);
        }
        // Draw last waypoint
        if (waypoints[waypoints.Length - 1] != null)
            Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.1f);
    }
}
