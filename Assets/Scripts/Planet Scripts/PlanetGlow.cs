using UnityEngine;

public class PlanetGlow : MonoBehaviour
{
    [Header("Renderer Settings")]
    [Tooltip("The Mesh Renderer component of the main planet body.")]
    [SerializeField] private MeshRenderer planetRenderer;

    [Header("Glow Settings")]
    [Tooltip("How bright the edge glow will be when fully activated.")]
    [SerializeField] private float activeIntensity = 5.0f;

    [Tooltip("How fast the glow transitions on and off (lower is faster).")]
    [SerializeField] private float transitionSpeed = 5.0f;

    private Material targetMaterial;
    private int glowIntensityID;
    private float currentGlow = 0f;
    private float targetGlow = 0f;

    void Start()
    {
        // 1. Cache the material property ID for high performance
        glowIntensityID = Shader.PropertyToID("_Glow_Intensity");

        // 2. Fetch the material instance safely
        if (planetRenderer != null)
        {
            // Using .material automatically clones it locally so this specific
            // planet glows without affecting other planets using the same shader.
            targetMaterial = planetRenderer.material;

            // Ensure it starts completely off
            targetMaterial.SetFloat(glowIntensityID, 0f);
        }
        else
        {
            Debug.LogError($"PlanetGlow on {gameObject.name} is missing a assigned Planet Renderer!", this);
        }
    }

    void Update()
    {
        if (targetMaterial == null) return;

        // 3. Smoothly interpolate the intensity to prevent sudden visual snapping
        currentGlow = Mathf.Lerp(currentGlow, targetGlow, Time.deltaTime * transitionSpeed);
        targetMaterial.SetFloat(glowIntensityID, currentGlow);
    }

    public void ToggleGlow(bool shouldGlow)
    {
        targetGlow = shouldGlow ? activeIntensity : 0f;
    }

    // Clean up the cloned material when the object is destroyed to prevent memory leaks
    private void OnDestroy()
    {
        if (targetMaterial != null)
        {
            Destroy(targetMaterial);
        }
    }

    private void OnEnable()
    {
        // Wait until the end of frame or check if instance exists
        if (PlanetManager.Instance != null)
        {
            PlanetManager.Instance.RegisterPlanet(this);
        }
    }

    private void OnDisable()
    {
        if (PlanetManager.Instance != null)
        {
            PlanetManager.Instance.UnregisterPlanet(this);
        }
    }
}
