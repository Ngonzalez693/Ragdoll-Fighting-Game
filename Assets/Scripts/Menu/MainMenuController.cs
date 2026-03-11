using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string playScene = "";                     // Name of the scene to load when "Play" is clicked

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;                   // AudioSource component to play sound effects
    [SerializeField] private AudioClip optionChange;                    // Sound effect for when the mouse hovers over a button
    [SerializeField] private AudioClip optionSelection;                 // Sound effect for when a button is clicked

    [Header("Delay")]
    [SerializeField] private float sceneLoadDelay = 0.3f;               // Delay before loading the next scene after clicking "Play"
    private bool isLoading = false;

    // Helper method to play a sound effect
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // When mouse passes over a button, play the option change sound
    public void OnHoverButton()
    {
        PlaySound(optionChange);
    }

    // Play
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

    // Exit 
    public void ExitGame()
    {
        PlaySound(optionSelection);

        // Close the application
        Application.Quit();
        Debug.Log("Exiting game...");

        // In editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
