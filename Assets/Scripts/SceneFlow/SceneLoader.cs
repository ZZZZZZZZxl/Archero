using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static SceneLoaderRunner _runner;
    private static bool _isLoading;

    public static bool IsLoading => _isLoading;

    public static void LoadMainMenu()
    {
        Load(SceneNames.MainMenu);
    }

    public static void LoadGame()
    {
        Load(SceneNames.Play);
    }

    public static void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Load(activeScene.name);
    }

    public static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || _isLoading)
            return;

        Time.timeScale = 1f;
        EnsureRunner();
        _runner.StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private static IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (operation == null)
        {
            Debug.LogError($"SceneLoader failed to load scene: {sceneName}");
            _isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;

        _isLoading = false;
    }

    private static void EnsureRunner()
    {
        if (_runner)
            return;

        GameObject runnerObject = new GameObject("SceneLoader");
        Object.DontDestroyOnLoad(runnerObject);
        _runner = runnerObject.AddComponent<SceneLoaderRunner>();
    }

    private sealed class SceneLoaderRunner : MonoBehaviour
    {
    }
}
