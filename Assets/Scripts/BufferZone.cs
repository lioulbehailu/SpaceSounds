using UnityEngine;

public class BufferZone : MonoBehaviour
{
    // Called when something LEAVES the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Satellite"))
        {
            Destroy(other.gameObject);
            // Or use: other.gameObject.SetActive(false);
            // if you want object pooling instead
        }
    }
}