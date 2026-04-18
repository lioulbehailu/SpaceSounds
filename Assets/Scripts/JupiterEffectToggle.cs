using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
public class JupiterEffectToggle : MonoBehaviour
{
    public enum EffectType { Frequency, VolumeOrDecibel }
    
    [Header("Setup")]
    public AudioMixer mainMixer;
    public string parameterName;
    public EffectType type = EffectType.VolumeOrDecibel;

    [Header("Values")]
    public float offValue; // e.g. -10000 for Reverb, 22000 for Lowpass
    public float onValue;  // e.g. 0 for Reverb, 500 for Lowpass
    public float transitionSpeed = 5f;

    private bool isActive = false;
    private float currentVal;
    private Camera mainCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;
        currentVal = offValue;
        mainMixer.SetFloat(parameterName, currentVal);
    }

    // Update is called once per frame
void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isActive = !isActive;
            }
        }

        float target = isActive ? onValue : offValue;
        // Use Lerp for Frequency (Smooth Sweeps)
        // Use MoveTowards for Decibels (Clean volume ramps)
        if (type == EffectType.Frequency)
            currentVal = Mathf.Lerp(currentVal, target, Time.deltaTime * transitionSpeed);
        else
            currentVal = Mathf.MoveTowards(currentVal, target, Time.deltaTime * transitionSpeed * 1000f);

        mainMixer.SetFloat(parameterName, currentVal);
} 
}
