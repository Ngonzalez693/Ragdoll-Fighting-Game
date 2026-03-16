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

    [Header("SFX Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip optionSelection;

    private bool isMuted = false;

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

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

        ApplyAudioState();
        PlaySound(optionSelection);
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