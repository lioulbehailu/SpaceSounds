using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MagneticRay : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private float maxRayLength = 0.8f;
    [SerializeField] private float magnetRadius = 4f;
    [SerializeField] private float bendSpeed = 8f;
    [SerializeField] private float coneAngle = 60f;

    [Header("Curve Settings")]
    [SerializeField] private int lineResolution = 24;
    [SerializeField] private float curveHeight = 0.25f;

    [Header("Line Renderer — assign the one on THIS gameobject")]
    [SerializeField] private LineRenderer lineRenderer;

    private Transform currentTarget = null;
    private Vector3 smoothEndPoint;
    private float smoothCurve = 0f;

    void Start()
    {
        smoothEndPoint = transform.position + transform.forward * maxRayLength;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = lineResolution;
            lineRenderer.useWorldSpace = true;
        }
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        currentTarget = FindTarget(origin, forward);

        Vector3 desiredEnd = currentTarget != null
            ? currentTarget.position
            : origin + forward * maxRayLength;

        // Smooth endpoint movement
        smoothEndPoint = Vector3.Lerp(
            smoothEndPoint, desiredEnd, Time.deltaTime * bendSpeed);

        // Smooth curve animation
        float desiredCurve = currentTarget != null ? 1f : 0f;
        smoothCurve = Mathf.Lerp(smoothCurve, desiredCurve, Time.deltaTime * bendSpeed);

        // Update visuals
        if (lineRenderer != null)
        {
            DrawBezier(origin, smoothEndPoint, smoothCurve);
        }
    }

    private void DrawBezier(Vector3 start, Vector3 end, float curveAmount)
    {
        Vector3 mid = (start + end) * 0.5f;

        // Control point bends upward relative to the ray direction
        // so it always arcs naturally regardless of controller orientation
        Vector3 rayDir = (end - start).normalized;
        Vector3 perpUp = Vector3.Cross(rayDir, transform.right).normalized;
        Vector3 controlPoint = mid + perpUp * (curveHeight * curveAmount);

        for (int i = 0; i < lineResolution; i++)
        {
            float t = i / (float)(lineResolution - 1);
            lineRenderer.SetPosition(i, Bezier(start, controlPoint, end, t));
        }
    }

    private Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }

    private Transform FindTarget(Vector3 origin, Vector3 direction)
    {
        Transform best = null;
        float bestScore = float.MaxValue;

        // Sample along the full ray length
        int samples = 6;
        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 point = origin + direction * (maxRayLength * t);
            Collider[] hits = Physics.OverlapSphere(point, magnetRadius);

            foreach (Collider hit in hits)
            {
                bool valid = hit.CompareTag("Satellite")
                          || hit.CompareTag("SnapZone");

                if (!valid) continue;

                Vector3 toTarget = hit.transform.position - origin;
                float angle = Vector3.Angle(direction, toTarget);
                if (angle > coneAngle) continue;

                float score = toTarget.magnitude + angle * 0.05f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = hit.transform;
                }
            }
        }

        return best;
    }

    public Transform GetCurrentTarget() => currentTarget;
    public bool HasTarget() => currentTarget != null;
}
