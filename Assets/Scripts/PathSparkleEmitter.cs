using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class PathSparkleEmitter : MonoBehaviour
{
    [SerializeField] private SatelliteFlightPath path;
    [Header("Density")]
    [SerializeField] private int sparklesPerFrame;
    [SerializeField] private float positionJitter = 0.08f;
    [SerializeField] private Vector2 sparkleSizeRange = new Vector2(0.002f, 0.05f);
    [SerializeField] private Vector2 alphaRange = new Vector2(0.15f, 0.6f);
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private float travelSpeed = 1.5f; // match satellite speed

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (path == null || ps == null) return;

        var emitParams = new ParticleSystem.EmitParams();
        Vector3 dir = path.GetPathDirection();
        float pathLength = path.GetTotalLength();
        float fullLifetime = pathLength / travelSpeed;

        for (int i = 0; i < sparklesPerFrame; i++)
        {
            int waypointIndex = Random.Range(0, path.GetWaypointCount() - 1);
            float t = Random.Range(0f, 1f);
            Vector3 pos = path.GetPosition(waypointIndex, t);
            pos += Random.insideUnitSphere * positionJitter;

            // Calculate how far along the path this particle spawns (0=start, 1=end)
            float segmentStart = 0f;
            for (int s = 0; s < waypointIndex; s++)
                segmentStart += path.GetSegmentLength(s);
            segmentStart += t * path.GetSegmentLength(waypointIndex);
            float distanceRemaining = pathLength - segmentStart;
            float adjustedLifetime = distanceRemaining / travelSpeed;

            emitParams.position = pos;
            emitParams.velocity = dir * travelSpeed;
            emitParams.startSize = Random.Range(sparkleSizeRange.x, sparkleSizeRange.y);
            emitParams.startLifetime = adjustedLifetime;
            float alpha = Random.Range(alphaRange.x, alphaRange.y);
            emitParams.startColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            ps.Emit(emitParams, 1);
        }
    }
}
