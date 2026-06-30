using UnityEngine;
using UnityEngine.UI;

public class SmoothFollowUI : MonoBehaviour
{
    [Header("Targets")]
    public Transform controllerTarget;

    [Header("Tuning")]
    public Vector3 localOffset = new Vector3(0.2f, 0.1f, 0.3f);
    public float followSpeed = 6f;
    public float rotationSpeed = 8f;
    [Tooltip("Euler angle offset applied on top of the controller rotation. Y and Z are automatically mirrored for the left hand.")]
    public Vector3 rotationOffset = Vector3.zero;
    [HideInInspector] public bool mirrorRotation;

    [Header("Data Injection Component")]
    // The single target image component that will display our pre-made sheet
    [SerializeField] private Image displayImage;

    void LateUpdate()
    {
        if (controllerTarget == null) return;

        Vector3 targetPosition = controllerTarget.TransformPoint(localOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        Vector3 effectiveOffset = mirrorRotation
            ? new Vector3(rotationOffset.x, -rotationOffset.y, -rotationOffset.z)
            : rotationOffset;
        Quaternion targetRotation = controllerTarget.rotation * Quaternion.Euler(effectiveOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Accepts the custom texture payload passed from whichever satellite was grabbed
    public void UpdateSpriteContent(Sprite newCheatSheetSprite)
    {
        if (displayImage != null && newCheatSheetSprite != null)
        {
            displayImage.sprite = newCheatSheetSprite;
        }
    }
}