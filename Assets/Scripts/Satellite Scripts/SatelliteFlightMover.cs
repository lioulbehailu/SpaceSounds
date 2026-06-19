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

    [Header("Trail Effect")]
    [SerializeField] private ParticleSystem cometTrail;


    public void Initialize(SatelliteFlightPath flightPath, int startWaypoint, float startProgress)
    {
        path = flightPath;
        currentWaypoint = startWaypoint;
        segmentProgress = startProgress;
        isActive = true;
        isEasingIn = false;

        if (cometTrail != null) cometTrail.Play();
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
        if (cometTrail != null) cometTrail.Stop();

    }

    // Called when a thrown satellite re-enters near this path
    public void ResumeFlight(SatelliteFlightPath flightPath, int waypointIndex, float progress)
    {
        path = flightPath;
        currentWaypoint = waypointIndex;
        segmentProgress = progress;
        isActive = true;

        // Smooth ease-in instead of instant snap
        isEasingIn = true;
        easeTimer = 0f;
        easeStartPos = transform.position;

        if (cometTrail != null) cometTrail.Play();

    }

}
