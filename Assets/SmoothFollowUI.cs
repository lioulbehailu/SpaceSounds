using UnityEngine;
using UnityEngine.UI;

public class SmoothFollowUI : MonoBehaviour
{
    [Header("Targets")]
    public Transform controllerTarget; 
    public Transform headCamera;       

    [Header("Tuning")]
    public Vector3 localOffset = new Vector3(0.2f, 0.1f, 0.3f); 
    public float followSpeed = 6f;
    public float rotationSpeed = 8f;

    [Header("Data Injection Component")]
    // The single target image component that will display our pre-made sheet
    [SerializeField] private Image displayImage;

    void Start()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (controllerTarget == null || headCamera == null) return;

        Vector3 targetPosition = controllerTarget.TransformPoint(localOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        Vector3 directionToCamera = headCamera.position - transform.position;
        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera); 
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
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