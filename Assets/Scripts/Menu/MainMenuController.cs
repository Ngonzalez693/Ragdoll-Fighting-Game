using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string playScene = "";

    [Header("SFX Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip optionChange;
    [SerializeField] private AudioClip optionSelection;

    [Header("Music Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Delay")]
    [SerializeField] private float sceneLoadDelay = 0.3f;

    private bool isLoading = false;

    private void Start()
    {
        ApplySavedMuteState();

        if (playMusicOnStart)
        {
            StartMenuMusic();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private void StartMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;

        if (musicSource.clip != menuMusic)
        {
            musicSource.clip = menuMusic;
        }

        musicSource.loop = true;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void ApplySavedMuteState()
    {
        bool isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    public void OnHoverButton()
    {
        PlaySound(optionChange);
    }

    public void PlayGame()
    {
        if (isLoading) return;
        isLoading = true;

        PlaySound(optionSelection);
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        LoadingData.NextScene = "FightScene";
        SceneManager.LoadScene(playScene);
    }

    public void ExitGame()
    {
        PlaySound(optionSelection);

        Application.Quit();
        Debug.Log("Exiting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}