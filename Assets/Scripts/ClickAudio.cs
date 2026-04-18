using UnityEngine;
using UnityEngine.InputSystem;

public class ClickAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private Camera mainCam;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainCam = Camera.main;
    }

    void Update()
    {
        // Using 'wasPressedThisFrame' ensures it only triggers ONCE per click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    ToggleAudio();
                }
            }
        }
    }

    void ToggleAudio()
    {
        Debug.Log("ToggleAudio called at: " + Time.time); // Add this
        if (audioSource.isPlaying)
        {
            Debug.Log("Stopping Audio on: " + gameObject.name);
            audioSource.Stop();
        }
        else
        {
            Debug.Log("Playing Audio on: " + gameObject.name);
            audioSource.Play();
        }
    }
}