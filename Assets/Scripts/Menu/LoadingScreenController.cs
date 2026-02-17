using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressSlider;                     // Reference to the UI Slider that shows loading progress

    [Header("Timing")]
    [SerializeField] private float minimumLoadingTime = 2.5f;           // Minimum time the loading screen should be visible (in seconds)

    private void Start()
    {
        if (progressSlider != null)
            progressSlider.value = 0f;

        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        string sceneToLoad = string.IsNullOrEmpty(LoadingData.NextScene) ? "Scene1" : LoadingData.NextScene;

        // Start loading the scene asynchronously but don't allow it to activate until we're ready
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        float elapsed = 0f;

        while (!op.isDone)
        {
            elapsed += Time.deltaTime;

            // Real progress goes de 0 a 0.9
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);

            // Fake progress to ensure the loading screen is visible
            float fakeProgress = Mathf.Clamp01(elapsed / minimumLoadingTime);

            // Use the smaller of the two to show the slider
            float shownProgress = Mathf.Min(fakeProgress, realProgress);

            if (progressSlider != null)
                progressSlider.value = shownProgress;

            // When both the real loading is done and the minimum time has passed, allow scene activation
            if (elapsed >= minimumLoadingTime && op.progress >= 0.9f)
            {
                if (progressSlider != null)
                    progressSlider.value = 1f;

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
