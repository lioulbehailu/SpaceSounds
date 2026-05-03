using UnityEngine;

public class OrbitManager : MonoBehaviour
{
    public Transform planet;        // The planet this orbit is centered around
    public float semiMajorAxis;     // The size of the orbit (radius at its widest point)
    public float eccentricity;      // How stretched the ellipse is (0 = circle, 1 = flat line)
    public float orbitalPeriod;     // How many seconds one full orbit takes
    public bool showOrbitPath;      // Toggle the yellow gizmo path in the editor

    // Converts a raw angle into an actual world position on the ellipse
    private Vector3 GetPositionAtAngle(float t)
    {
        // Ellipse formula — x stretches horizontally, z stretches along the other axis
        float x = semiMajorAxis * (Mathf.Cos(t) - eccentricity);
        float z = semiMajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity) * Mathf.Sin(t);

        // Return the position relative to the planet's location in world space
        return planet.position + new Vector3(x, 0, z);
    }

    // Called by SatelliteOnOrbit.cs every frame to know where to move
    public Vector3 GetPositionAtTime(float time)
    {
        // Mean Anomaly (M): where the satellite "should" be if the orbit were a perfect circle
        float M = (2 * Mathf.PI * time) / orbitalPeriod;

        // Kepler's Equation: iteratively corrects M into the true angle E for an ellipse
        // 5 iterations is enough for smooth results without heavy computation
        float E = M;
        for (int i = 0; i < 5; i++) E = M + eccentricity * Mathf.Sin(E);

        // Convert the solved angle into a world position
        return GetPositionAtAngle(E);
    }

    // Only runs in the Unity Editor — draws the orbit path as yellow dots in the scene view
    void OnDrawGizmos()
    {
        if (!showOrbitPath || planet == null) return;
        Gizmos.color = Color.yellow;

        // Step around the full ellipse in 100 increments and draw a dot at each point
        for (int i = 0; i < 100; i++)
        {
            float t = (i / 100f) * 2 * Mathf.PI;
            Gizmos.DrawSphere(GetPositionAtAngle(t), 0.1f);
        }
    }
}