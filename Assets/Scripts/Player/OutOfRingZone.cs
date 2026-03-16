using UnityEngine;

public class OutOfRingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.FallOutOfRing();
        }
    }
}