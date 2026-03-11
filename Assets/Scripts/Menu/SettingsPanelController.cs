using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Mute Button")]
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;                   // AudioSource component to play sound effects
    [SerializeField] private AudioClip optionSelection;                 // Sound effect for when a button is clicked

    private bool isMuted = false;                                       // Tracks whether the audio is currently muted

    // Helper method to play a sound effect
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void Start()
    {
        // Oculta el panel al iniciar
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Cargar estado guardado
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplyAudioState();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        PlaySound(optionSelection);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        PlaySound(optionSelection);
        ApplyAudioState();
    }

    private void ApplyAudioState()
    {
        AudioListener.volume = isMuted ? 0f : 1f;

        if (muteButtonImage != null)
        {
            muteButtonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }
}