using UnityEngine;

public class PunchHitbox : MonoBehaviour
{
    [SerializeField] private PlayerHealth ownerHealth;

    [Header("Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitSoundCooldown = 0.15f;

    private float lastHitSoundTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (ownerHealth == null) return;

        PlayerHealth targetHealth = other.GetComponentInParent<PlayerHealth>();

        if (targetHealth == null) return;

        if (targetHealth == ownerHealth) return;

        Debug.Log($"{ownerHealth.gameObject.name} golpeó a {targetHealth.gameObject.name}");

        PlayHitSound();
        targetHealth.ReceiveHit();
    }

    private void PlayHitSound()
    {
        if (hitAudioSource == null || hitSound == null) return;

        if (Time.time < lastHitSoundTime + hitSoundCooldown)
            return;

        lastHitSoundTime = Time.time;
        hitAudioSource.PlayOneShot(hitSound);
    }
}