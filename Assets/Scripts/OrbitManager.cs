using UnityEngine;

public class OrbitManager : MonoBehaviour
{
    public Transform planet; // The center
    public float semiMajorAxis = 10f;
    public float eccentricity = 0.3f;
    public float orbitalPeriod = 10f;
    public bool showOrbitPath = true; 


    // This method calculates position for anyone who asks
    public Vector3 GetPositionAtTime(float time)
    {
        // 1. Mean Anomaly (M) - The average position
        float M = (2 * Mathf.PI * time) / orbitalPeriod;
       
        // 2. Solve Kepler's Equation (E)
        float E = M;
        for (int i = 0; i < 5; i++) E = M + eccentricity * Mathf.Sin(E);

        // 3. Calculate position relative to the planet
        float x = semiMajorAxis * (Mathf.Cos(E) - eccentricity);
        float z = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity) * Mathf.Sin(E);

        // 4. Update the position
        return planet.position + new Vector3(x, 0, z);
    }


    void OnDrawGizmos()
    {

        if (!showOrbitPath || planet == null) return;

        Gizmos.color = Color.yellow;
        
        // Draw 100 dots to represent the elliptical path
        for (int i = 0; i < 100; i++)
        {
            float t = (i / 100f) * 2 * Mathf.PI;
            float x = semiMajorAxis * (Mathf.Cos(t) - eccentricity);
            float z = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity) * Mathf.Sin(t);
            Gizmos.DrawSphere(planet.position + new Vector3(x, 0, z), 0.1f);
        }
    }

}