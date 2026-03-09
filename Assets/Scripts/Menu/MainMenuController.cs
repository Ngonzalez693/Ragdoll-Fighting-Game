using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string playScene = "";                     // Name of the scene to load when "Play" is clicked

    // Play, Multiplayer, Settings, Exit methods
    public void PlayGame()
    {
        LoadingData.NextScene = "FightScene";
        SceneManager.LoadScene(playScene);
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
