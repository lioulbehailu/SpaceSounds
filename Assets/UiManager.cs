using UnityEngine;

public class UiManager : MonoBehaviour
{
    // The static global access point
    public static UiManager Instance { get; private set; }

    [Header("Global UI References")]
    [SerializeField] private GameObject satelliteCheatSheetCanvas; 
    
    private SmoothFollowUI followScript;

    void Awake()
    {
        // Enforce that only one instance of this manager ever exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache the follow script for faster performance later
        if (satelliteCheatSheetCanvas != null)
        {
            followScript = satelliteCheatSheetCanvas.GetComponent<SmoothFollowUI>();
        }
    }

    // Public methods so your spawned prefabs can safely talk to the UI
    public GameObject GetCanvasObject() => satelliteCheatSheetCanvas;
    public SmoothFollowUI GetFollowScript() => followScript;
}