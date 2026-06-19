using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PathSparkleEmitter : MonoBehaviour
{
    [SerializeField] private SatelliteFlightPath path;

    [Header("Density")]
    [SerializeField] private int sparklesPerMeter = 30; // density-based, not fixed count
    [SerializeField] private int layers = 2;             // multiple passes = denser cloud look

    [Header("Look")]
    [SerializeField] private Vector2 sparkleSizeRange = new Vector2(0.002f, 0.05f);
    [SerializeField] private float positionJitter = 0.08f;
    [SerializeField] private Vector2 alphaRange = new Vector2(0.15f, 0.6f);
    [SerializeField] private Color baseColor = Color.white;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        SpawnStaticSparkles();
    }

    private void SpawnStaticSparkles()
    {
        if (path == null) return;

        var emitParams = new ParticleSystem.EmitParams();
        int waypointCount = path.GetWaypointCount();

        for (int layer = 0; layer < layers; layer++)
        {
            for (int i = 0; i < waypointCount - 1; i++)
            {
                float segmentLength = path.GetSegmentLength(i);
                int sparkleCount = Mathf.CeilToInt(segmentLength * sparklesPerMeter);

                for (int j = 0; j < sparkleCount; j++)
                {
                    float t = Random.Range(0f, 1f); // random along segment, not evenly spaced
                    Vector3 pos = path.GetPosition(i, t);

                    // Wider jitter per layer creates a soft "cloud" cross-section
                    // instead of a thin hard line
                    pos += Random.insideUnitSphere * positionJitter;

                    emitParams.position = pos;
                    emitParams.startSize = Random.Range(sparkleSizeRange.x, sparkleSizeRange.y);
                    emitParams.startLifetime = 999f;

                    float alpha = Random.Range(alphaRange.x, alphaRange.y);
                    emitParams.startColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                    ps.Emit(emitParams, 1);
                }
            }
        }
    }
}
