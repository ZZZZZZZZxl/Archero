using System;
using UnityEngine;

public enum RewardState
{
    Idle,
    Collecting,
    Collected,
    Stay
}

public class RewardController : MonoBehaviour
{
    [SerializeField] private int _amount = 1;
    [SerializeField] private float _collectDuration = 0.5f;
    [SerializeField] private float _collectArcHeight = 0.55f;
    [SerializeField] private float _collectTargetHeight = 0.75f;
    [SerializeField] private float _minCollectDuration = 0.15f;

    public int Amount => _amount;
    public RewardState State => _state;

    private RewardState _state = RewardState.Idle;
    private Transform _target;
    private Action<RewardController> _onCollected;
    private Vector3 _collectStartPosition;
    private Vector3 _collectControlPoint;
    private Vector3 _startScale;
    private Vector3 _initialScale;
    private float _collectTimer;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void Update()
    {
        if (_state == RewardState.Collecting)
            UpdateCollecting();
    }

    public void CollectTo(Transform target, Action<RewardController> onCollected)
    {
        if (_state != RewardState.Idle)
            return;

        if (!target)
        {
            CompleteCollect();
            return;
        }

        _target = target;
        _onCollected = onCollected;
        _state = RewardState.Collecting;

        _collectTimer = 0f;
        _collectStartPosition = transform.position;
        _startScale = transform.localScale;

        Vector3 targetPosition = GetTargetPosition();
        Vector3 middlePoint = (_collectStartPosition + targetPosition) * 0.5f;
        _collectControlPoint = middlePoint + Vector3.up * _collectArcHeight;
    }

    private void UpdateCollecting()
    {
        if (!_target)
        {
            CompleteCollect();
            return;
        }

        _collectTimer += Time.deltaTime;
        float duration = Mathf.Max(_minCollectDuration, _collectDuration);
        float t = Mathf.Clamp01(_collectTimer / duration);
        float easedT = t * t * (3f - 2f * t);

        Vector3 targetPosition = GetTargetPosition();
        Vector3 firstSegment = Vector3.Lerp(_collectStartPosition, _collectControlPoint, easedT);
        Vector3 secondSegment = Vector3.Lerp(_collectControlPoint, targetPosition, easedT);
        transform.position = Vector3.Lerp(firstSegment, secondSegment, easedT);

        transform.localScale = Vector3.Lerp(_startScale, _startScale * 0.45f, easedT);

        if (t >= 1f)
            CompleteCollect();
    }

    private Vector3 GetTargetPosition()
    {
        return _target.position + Vector3.up * _collectTargetHeight;
    }

    private void CompleteCollect()
    {
        if (_state == RewardState.Collected)
            return;

        _state = RewardState.Collected;
        _onCollected?.Invoke(this);
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.CoinCollect);
        PoolManager.MainInstance.Release(this);
    }

    private void ResetState()
    {
        _state = RewardState.Idle;
        _target = null;
        _onCollected = null;
        _collectStartPosition = Vector3.zero;
        _collectControlPoint = Vector3.zero;
        _startScale = _initialScale;
        _collectTimer = 0f;
        transform.localScale = _initialScale;
    }
}
