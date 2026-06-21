using UnityEngine;

public class PathGlowyLine : MonoBehaviour
{
    [SerializeField] private SatelliteFlightPath path;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private float glowWidth = 0.05f;

    private LineRenderer lineRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    public static PathGlowyLine Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = glowWidth;
        lineRenderer.endWidth = glowWidth;
        lineRenderer.positionCount = path.GetWaypointCount();

        // Set waypoint positions
        lineRenderer.positionCount = path.GetWaypointCount();
        for (int i = 0; i < path.GetWaypointCount(); i++)
            lineRenderer.SetPosition(i, path.GetPosition(Mathf.Clamp(i, 0, path.GetWaypointCount() - 2), i == path.GetWaypointCount() - 1 ? 1f : 0f));

        SetLineAlpha(0f);
    }

    public void SetGlowActive(bool active)
    {
        targetAlpha = active ? 1f : 0f;
    }

    void Update()
    {
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        SetLineAlpha(currentAlpha);
    }

    private void SetLineAlpha(float alpha)
    {
        Color c = glowColor;
        c.a = glowColor.a * alpha; 
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }
}
