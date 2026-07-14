using UnityEngine;
using System.Collections.Generic;

public class MagneticRay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxRayLength = 0.8f;
    [SerializeField] private float magnetRadius = 0.3f;
    [SerializeField] private float bendSpeed = 8f;
    [SerializeField] private float maxBendAngle = 30f;

    [Header("References")]
    [SerializeField] private Transform rayOriginTransform;

    // Shared across ALL MagneticRay instances — prevents two rays claiming same target
    private static HashSet<Transform> claimedTargets = new HashSet<Transform>();

    private Transform currentTarget = null;
    private Quaternion originalRotation;
    private bool isHoldingSomething = false;

    void Start()
    {
        if (rayOriginTransform != null)
            originalRotation = rayOriginTransform.localRotation;
    }

    void OnDisable()
    {
        // Release claim when controller is disabled
        ReleaseClaim();
    }

    // Call this from SpaceInteractionXR when grab starts/ends
    public void SetHoldingState(bool holding)
    {
        isHoldingSomething = holding;

        if (holding)
        {
            // Release magnetic claim and reset ray immediately
            ReleaseClaim();
            rayOriginTransform.localRotation = originalRotation;
        }
    }

    void Update()
    {
        if (rayOriginTransform == null) return;

        // STOP all computation while holding a satellite
        if (isHoldingSomething)
            return;

        Transform newTarget = FindTarget();

        // If our previous target was claimed by someone else, release it
        if (currentTarget != null && !claimedTargets.Contains(currentTarget))
            currentTarget = null;

        // Update claim
        if (newTarget != currentTarget)
        {
            ReleaseClaim();
            currentTarget = newTarget;

            if (currentTarget != null)
                claimedTargets.Add(currentTarget);
        }

        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - rayOriginTransform.position).normalized;

            // Check angle between current forward and direction to target
            float angleToTarget = Vector3.Angle(rayOriginTransform.parent.forward, dir);

            // Only bend if target is within maxBendAngle of natural forward direction
            if(angleToTarget <= maxBendAngle)
            {
                rayOriginTransform.rotation = Quaternion.Slerp(
                    rayOriginTransform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * bendSpeed);
            }
            else
            {
                // Target exists but is too far off-axis — reset to straight
                ReleaseClaim();
                rayOriginTransform.localRotation = Quaternion.Slerp(
                    rayOriginTransform.localRotation,
                    originalRotation,
                    Time.deltaTime * bendSpeed);
            }

        }
        else
        {
            rayOriginTransform.localRotation = Quaternion.Slerp(
                rayOriginTransform.localRotation,
                originalRotation,
                Time.deltaTime * bendSpeed);
        }
    }

    private void ReleaseClaim()
    {
        if (currentTarget != null)
        {
            claimedTargets.Remove(currentTarget);
            currentTarget = null;
        }
    }

    private Transform FindTarget()
    {
        Vector3 origin = rayOriginTransform.position;
        Vector3 direction = rayOriginTransform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin, magnetRadius, direction, maxRayLength);

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Satellite")) continue;

            float dist = Vector3.Distance(origin, hit.transform.position);
            if (dist > maxRayLength) continue;

            // Skip if already claimed by the other controller
            if (claimedTargets.Contains(hit.transform) && hit.transform != currentTarget)
                continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = hit.transform;
            }
        }

        return best;
    }

    public Transform GetCurrentTarget() => currentTarget;
    public bool HasTarget() => currentTarget != null;
}
