using UnityEngine;

public class SatelliteOnOrbit : MonoBehaviour
{
    // script to place staellite in correct place and make it spin 

    public OrbitManager orbitPath;

    private float timeOffset = 0f; // Shifts the orbit start so we begin at the nearest point

    #region snap current satellite to closest point in orbit
    public void SnapToNearestPoint()
    {
        if (orbitPath == null) return;

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
    }
    #endregion

    #region move stellite on orbit
    void Update()
    {
        if (orbitPath == null) return;
        transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
    }
    #endregion
}
