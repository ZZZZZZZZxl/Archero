using UnityEngine;

public class DamageNumberFeedback : MonoBehaviour
{
    [SerializeField] private DamageNumberItem _damageNumberPrefab;
    [SerializeField] private Color _textColor = new Color(1f, 0.92f, 0.18f, 1f);
    [SerializeField] private float _fontSize = 2.8f;
    [SerializeField] private float _lifeTime = 0.55f;
    [SerializeField] private float _riseDistance = 0.85f;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 1.35f, 0f);
    [SerializeField] private float _burstResetTime = 0.08f;
    [SerializeField] private float _burstHorizontalSpacing = 0.35f;
    [SerializeField] private float _burstVerticalSpacing = 0.12f;

    private float _lastPlayTime = -999f;
    private int _burstIndex;

    public void Configure(DamageNumberItem damageNumberPrefab)
    {
        if (damageNumberPrefab)
            _damageNumberPrefab = damageNumberPrefab;
    }

    public void Play(float damage)
    {
        if (!_damageNumberPrefab || !PoolManager.MainInstance)
            return;

        Vector3 spawnPosition = GetSpawnPosition() + GetBurstOffset();
        DamageNumberItem item = PoolManager.MainInstance.Get(
            _damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (item)
            item.Play(damage, _textColor, _fontSize, _lifeTime, _riseDistance);
    }

    private Vector3 GetSpawnPosition()
    {
        Collider targetCollider = GetComponentInChildren<Collider>();
        if (targetCollider)
            return targetCollider.bounds.center + Vector3.up * targetCollider.bounds.extents.y;

        return transform.position + _spawnOffset;
    }

    private Vector3 GetBurstOffset()
    {
        int index = GetBurstIndex();
        if (index <= 0)
            return Vector3.zero;

        Camera camera = Camera.main;
        Vector3 right = camera ? camera.transform.right : transform.right;
        right.y = 0f;

        if (right.sqrMagnitude < 0.0001f)
            right = transform.right;

        right.Normalize();

        int pairIndex = (index + 1) / 2;
        float side = index % 2 == 1 ? -1f : 1f;
        return right * (side * pairIndex * _burstHorizontalSpacing)
               + Vector3.up * (pairIndex * _burstVerticalSpacing);
    }

    private int GetBurstIndex()
    {
        if (Time.time - _lastPlayTime > _burstResetTime)
            _burstIndex = 0;
        else
            _burstIndex++;

        _lastPlayTime = Time.time;
        return _burstIndex;
    }
}
