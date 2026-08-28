using UnityEngine;

public class SnapZoneCounter : MonoBehaviour
{
    private int satelliteCount = 0;
    public bool IsOccupied => satelliteCount > 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Satellite"))
        {
            satelliteCount++;
            Debug.Log($"[SnapZone] Satellite entered. Count: {satelliteCount}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Satellite"))
        {
            satelliteCount = Mathf.Max(0, satelliteCount - 1);
            Debug.Log($"[SnapZone] Satellite left. Count: {satelliteCount}");
        }
    }

    public void ForceReset()
    {
        satelliteCount = 0;
        Debug.Log("[SnapZone] Count manually reset to 0.");
    }
}
