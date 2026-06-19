using UnityEngine;

public class SatelliteFlightPath : MonoBehaviour
{
    [Header("Path Waypoints")]
    [Tooltip("Drag empty GameObjects marking the path, in order, from spawn to despawn")]
    public Transform[] waypoints;

    [Header("Spawn Settings")]
    public GameObject satellitePrefab;
    public float spawnInterval = 4f;
    public float travelSpeed = 1.5f;       // units per second

    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnSatellite();
        }
    }

    private void SpawnSatellite()
    {
        if (satellitePrefab == null || waypoints.Length < 2) return;

        GameObject sat = Instantiate(satellitePrefab, waypoints[0].position, Quaternion.identity);

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
}
