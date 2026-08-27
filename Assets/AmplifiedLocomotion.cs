using UnityEngine;

public class AmplifiedLocomotion : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private float movementScale = 1.2f;

    private Vector3 _lastCameraLocalPos;

    private void Start()
    {
        if (mainCamera != null)
            _lastCameraLocalPos = mainCamera.localPosition;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 currentLocalPos = mainCamera.localPosition;
        Vector3 delta = currentLocalPos - _lastCameraLocalPos;

        // Ignore vertical head bobbing to prevent height shifts
        delta.y = 0;

        // Apply extra movement offset to the parent rig
        Vector3 worldDelta = transform.TransformDirection(delta);
        transform.position += worldDelta * (movementScale - 1.0f);

        _lastCameraLocalPos = currentLocalPos;
    }
}
