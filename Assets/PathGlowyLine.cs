using UnityEngine;

public class PathGlowyLine : MonoBehaviour
{
    [SerializeField] private SatelliteFlightPath path;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float glowWidth = 0.05f;
    [SerializeField] private float proximityDistance = 0.5f;

    private LineRenderer lineRenderer;
    private float currentAlpha = 0f;

    public static PathGlowyLine Instance { get; private set; }
    void Awake() => Instance = this;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = glowWidth;
        lineRenderer.endWidth = glowWidth;
        lineRenderer.positionCount = path.GetWaypointCount();

        for (int i = 0; i < path.GetWaypointCount(); i++)
            lineRenderer.SetPosition(i, path.GetPosition(
                Mathf.Clamp(i, 0, path.GetWaypointCount() - 2),
                i == path.GetWaypointCount() - 1 ? 1f : 0f));

        SetLineAlpha(0f);
    }

    void Update()
    {
        float targetAlpha = IsAnyHeldSatelliteNearPath() ? 1f : 0f;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        SetLineAlpha(currentAlpha);
    }

    private bool IsAnyHeldSatelliteNearPath()
    {
        foreach (var sat in SatelliteOnOrbit.CurrentlyHeld)
        {
            if (sat == null) continue;
            if (IsNearPath(sat.transform.position))
                return true;
        }
        return false;
    }


    private bool IsNearPath(Vector3 pos)
    {
        for (int i = 0; i < path.GetWaypointCount() - 1; i++)
        {
            Vector3 a = path.GetPosition(i, 0f);
            Vector3 b = path.GetPosition(i, 1f);
            Vector3 closest = ClosestPointOnSegment(pos, a, b);
            if (Vector3.Distance(pos, closest) <= proximityDistance)
                return true;
        }
        return false;
    }

    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }

    private void SetLineAlpha(float alpha)
    {
        Color c = glowColor;
        c.a = glowColor.a * alpha;
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }
}
