using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenHitFlash : MonoBehaviour
{
    [SerializeField] private Color _flashColor = new Color(1f, 0f, 0f, 0.16f);
    [SerializeField] private float _fadeDuration = 0.18f;

    private static ScreenHitFlash _instance;

    private Image _image;
    private Coroutine _flashCoroutine;

    public static void Play()
    {
        EnsureInstance();
        if (_instance)
            _instance.PlayInternal();
    }

    private static void EnsureInstance()
    {
        if (_instance)
            return;

        GameObject canvasObject = new GameObject("ScreenHitFlashCanvas");
        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("RedFlash");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = Color.clear;

        _instance = canvasObject.AddComponent<ScreenHitFlash>();
        _instance._image = image;
        _instance.Clear();
    }

    private void PlayInternal()
    {
        if (!_image)
            return;

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        Clear();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Clear();
    }

    private void Clear()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        if (_image)
            _image.color = Color.clear;
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;
        _image.color = _flashColor;

        while (timer < _fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / _fadeDuration);
            Color color = _flashColor;
            color.a = Mathf.Lerp(_flashColor.a, 0f, t);
            _image.color = color;
            yield return null;
        }

        _image.color = Color.clear;
        _flashCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
