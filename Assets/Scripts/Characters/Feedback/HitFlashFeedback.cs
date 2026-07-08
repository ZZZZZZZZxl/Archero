using System.Collections;
using UnityEngine;

public class HitFlashFeedback : MonoBehaviour
{
    [SerializeField] private Color _flashColor = new Color(1f, 0.18f, 0.12f, 1f);
    [SerializeField] private float _flashDuration = 0.12f;
    [SerializeField] private bool _ignoreSelectionIndicators = true;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private Renderer[] _renderers;
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void Play()
    {
        if (_renderers == null || _renderers.Length == 0)
            return;

        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlashColor(_flashColor);
        yield return new WaitForSeconds(_flashDuration);
        ClearFlashColor();
        _flashCoroutine = null;
    }

    private void SetFlashColor(Color color)
    {
        foreach (Renderer targetRenderer in _renderers)
        {
            if (!targetRenderer)
                continue;

            if (ShouldIgnore(targetRenderer))
                continue;

            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _propertyBlock.SetColor(ColorId, color);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void ClearFlashColor()
    {
        foreach (Renderer targetRenderer in _renderers)
        {
            if (!targetRenderer)
                continue;

            if (ShouldIgnore(targetRenderer))
                continue;

            targetRenderer.SetPropertyBlock(null);
        }
    }

    private bool ShouldIgnore(Renderer targetRenderer)
    {
        if (!_ignoreSelectionIndicators)
            return false;

        string objectName = targetRenderer.gameObject.name;
        return objectName.Contains("Target") || objectName.Contains("Arrow") || objectName.Contains("Select");
    }

    private void OnDisable()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        ClearFlashColor();
    }
}
