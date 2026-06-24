using UnityEngine;
public class SatelliteFlightMover : MonoBehaviour
{
    private SatelliteFlightPath path;
    private int currentWaypoint = 0;
    private float segmentProgress = 0f;
    private bool isActive = true;
    [Header("Re-entry Smoothing")]
    [SerializeField] private float easeInSpeed = 4f;
    private bool isEasingIn = false;
    private Vector3 easeStartPos;
    private float easeTimer = 0f;
    private const float easeDuration = 0.4f;
    [Header("Fade")]
    [SerializeField] private float fadeInDistance = 5f;
    [SerializeField] private float fadeOutDistance = 5f;
    private Renderer[] renderers;

    public void Initialize(SatelliteFlightPath flightPath, int startWaypoint, float startProgress)
    {
        path = flightPath;
        currentWaypoint = startWaypoint;
        segmentProgress = startProgress;
        isActive = true;
        isEasingIn = false;
        renderers = GetComponentsInChildren<Renderer>();
        HandleFade();
    }

    void Update()
    {
        if (!isActive || path == null) return;
        if (isEasingIn)
        {
            easeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(easeTimer / easeDuration);
            Vector3 targetPos = path.GetPosition(currentWaypoint, segmentProgress);
            transform.position = Vector3.Lerp(easeStartPos, targetPos, t);
            if (t >= 1f)
                isEasingIn = false;
            AdvanceProgress();
            return;
        }
        AdvanceProgress();
        transform.position = path.GetPosition(currentWaypoint, segmentProgress);
        HandleFade();
    }

    private void AdvanceProgress()
    {
        float segmentLength = path.GetSegmentLength(currentWaypoint);
        if (segmentLength <= 0.01f) segmentLength = 0.01f;
        segmentProgress += (Time.deltaTime * path.GetTravelSpeed()) / segmentLength;
        if (segmentProgress >= 1f)
        {
            segmentProgress = 0f;
            currentWaypoint++;
            if (currentWaypoint >= path.GetWaypointCount() - 1)
            {
                path.DespawnSatellite(gameObject);
            }
        }
    }

    public void PauseFlight()
    {
        isActive = false;
    }

    public void ResumeFlight(SatelliteFlightPath flightPath, int waypointIndex, float progress)
    {
        path = flightPath;
        currentWaypoint = waypointIndex;
        segmentProgress = progress;
        isActive = true;
        isEasingIn = true;
        easeTimer = 0f;
        easeStartPos = transform.position;
    }

    private void HandleFade()
    {
        if (path == null || renderers == null) return;
        float distanceTravelled = 0f;
        for (int i = 0; i < currentWaypoint; i++)
            distanceTravelled += path.GetSegmentLength(i);
        distanceTravelled += segmentProgress * path.GetSegmentLength(currentWaypoint);
        float totalLength = path.GetTotalLength();
        float distanceRemaining = totalLength - distanceTravelled;
        float fadeIn = Mathf.Clamp01(distanceTravelled / fadeInDistance);
        float fadeOut = Mathf.Clamp01(distanceRemaining / fadeOutDistance);
        float alpha = Mathf.Min(fadeIn, fadeOut);
        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer ren in renderers)
        {
            foreach (Material mat in ren.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}
