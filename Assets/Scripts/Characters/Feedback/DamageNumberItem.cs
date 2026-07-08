using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumberItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private Camera _camera;
    private Coroutine _playCoroutine;

    private void Awake()
    {
        EnsureText();
    }

    private void OnDisable()
    {
        if (_playCoroutine != null)
        {
            StopCoroutine(_playCoroutine);
            _playCoroutine = null;
        }
    }

    public void Play(float damage, Color color, float fontSize, float lifeTime, float riseDistance)
    {
        EnsureText();

        if (!_text)
        {
            Release();
            return;
        }

        int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
        _text.text = roundedDamage.ToString();
        _text.color = color;
        _text.fontSize = fontSize;

        _camera = Camera.main;

        if (_playCoroutine != null)
            StopCoroutine(_playCoroutine);

        _playCoroutine = StartCoroutine(PlayRoutine(lifeTime, riseDistance));
    }

    private IEnumerator PlayRoutine(float lifeTime, float riseDistance)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.up * riseDistance;
        Color startColor = _text.color;
        float timer = 0f;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lifeTime);

            transform.position = Vector3.Lerp(startPosition, endPosition, EaseOut(t));
            FaceCamera();

            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            _text.color = color;

            yield return null;
        }

        _playCoroutine = null;
        Release();
    }

    private void FaceCamera()
    {
        if (!_camera)
            _camera = Camera.main;

        if (!_camera)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }

    private void EnsureText()
    {
        if (_text)
            return;

        _text = GetComponent<TMP_Text>();
    }

    private void Release()
    {
        if (PoolManager.MainInstance)
            PoolManager.MainInstance.Release(this);
        else
            gameObject.SetActive(false);
    }

    private static float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}
