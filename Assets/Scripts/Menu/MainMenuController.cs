using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string playScene = "";                     // Name of the scene to load when "Play" is clicked
    [SerializeField] private string multiplayerScene = "";              // Name of the scene to load when "Multiplayer" is clicked

    // Play, Multiplayer, Settings, Exit methods
    public void PlayGame()
    {
        LoadingData.NextScene = "Scene1";
        SceneManager.LoadScene(playScene);
    }

    public void Multiplayer()
    {
        LoadingData.NextScene = "Scene1";
        SceneManager.LoadScene(multiplayerScene);
    }

    public void Settings()
    {
        Debug.Log("Settings: coming soon...");
    }

    public void ExitGame()
    {
        // Close the application
        Application.Quit();
        Debug.Log("Exiting game...");

        // In editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
