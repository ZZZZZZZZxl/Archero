using UnityEngine;
using UnityEngine.SceneManagement;

public static class AudioListenerBootstrap
{
    private static AudioListener _fallbackListener;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureFallbackBeforeFirstScene()
    {
        if (_fallbackListener)
            return;

        GameObject listenerObject = new GameObject("RuntimeAudioListener");
        Object.DontDestroyOnLoad(listenerObject);
        _fallbackListener = listenerObject.AddComponent<AudioListener>();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>();
        AudioListener sceneListener = null;

        foreach (AudioListener listener in listeners)
        {
            if (!listener || listener == _fallbackListener || !listener.enabled || !listener.gameObject.activeInHierarchy)
                continue;

            sceneListener = listener;
            break;
        }

        if (sceneListener)
        {
            if (_fallbackListener)
                Object.Destroy(_fallbackListener.gameObject);

            _fallbackListener = null;
            return;
        }

        Camera mainCamera = Camera.main;
        if (!mainCamera)
            mainCamera = Object.FindObjectOfType<Camera>();

        if (!mainCamera)
            return;

        AudioListener cameraListener = mainCamera.GetComponent<AudioListener>();
        if (!cameraListener)
            cameraListener = mainCamera.gameObject.AddComponent<AudioListener>();

        cameraListener.enabled = true;

        if (_fallbackListener && _fallbackListener != cameraListener)
        {
            Object.Destroy(_fallbackListener.gameObject);
            _fallbackListener = null;
        }
    }
}
