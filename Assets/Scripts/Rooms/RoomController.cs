using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private bool _autoCollectEnemiesFromChildren = true;
    [SerializeField] private List<EnemyHealthController> _enemies = new List<EnemyHealthController>();
    [SerializeField] private Transform _rewardRoot;
    [FormerlySerializedAs("rewardControllerPrefab")]
    [SerializeField] private RewardController _rewardPrefab;
    [SerializeField] private int _rewardDropCount = 1;
    [SerializeField] private float _collectRewardDelay = 0.15f;
    [SerializeField] private float _maxCollectRewardWaitTime = 2f;

    private readonly List<RewardController> _rewards = new List<RewardController>();
    private int _aliveEnemyCount;
    private bool _isCleared;
    private bool _isCollectingRewards;
    private int _pendingRewardCount;

    public bool IsCleared => _isCleared;

    private void Awake()
    {
        EnsureRewardRoot();
        RebuildEnemyBindings();
    }

    private void OnDestroy()
    {
        UnbindEnemies();
    }

    private void BindEnemies()
    {
        _aliveEnemyCount = 0;
        _rewards.Clear();

        foreach (EnemyHealthController enemy in _enemies)
        {
            if (!enemy || !enemy.gameObject.activeInHierarchy || enemy.IsDead)
                continue;

            _aliveEnemyCount++;
            enemy.DiedEvent -= OnEnemyDied;
            enemy.DiedEvent += OnEnemyDied;
        }
    }

    public void RebuildEnemyBindings()
    {
        UnbindEnemies();

        if (_autoCollectEnemiesFromChildren)
        {
            _enemies.Clear();
            GetComponentsInChildren(true, _enemies);
        }

        BindEnemies();

        if (_aliveEnemyCount == 0 && !_isCleared)
            StartCollectRewards();
    }

    public void ResetRoomState()
    {
        StopAllCoroutines();
        UnbindEnemies();
        ClearRewards();

        _aliveEnemyCount = 0;
        _isCleared = false;
        _isCollectingRewards = false;
        _pendingRewardCount = 0;
    }

    private void UnbindEnemies()
    {
        foreach (EnemyHealthController enemy in _enemies)
        {
            if (enemy)
                enemy.DiedEvent -= OnEnemyDied;
        }
    }

    private void OnEnemyDied(EnemyHealthController enemy)
    {
        if (_isCleared)
            return;

        if (enemy)
        {
            enemy.DiedEvent -= OnEnemyDied;
            DropRewards(enemy.transform.position);
            Destroy(enemy.gameObject);
        }

        _aliveEnemyCount = Mathf.Max(0, _aliveEnemyCount - 1);

        if (_aliveEnemyCount == 0)
            StartCollectRewards();
    }

    private void EnsureRewardRoot()
    {
        if (_rewardRoot)
            return;

        Transform existingRewardRoot = transform.Find("Rewards");
        if (existingRewardRoot)
        {
            _rewardRoot = existingRewardRoot;
            return;
        }

        GameObject rewardRootObject = new GameObject("Rewards");
        rewardRootObject.transform.SetParent(transform);
        rewardRootObject.transform.localPosition = Vector3.zero;
        rewardRootObject.transform.localRotation = Quaternion.identity;
        rewardRootObject.transform.localScale = Vector3.one;
        _rewardRoot = rewardRootObject.transform;
    }

    private void DropRewards(Vector3 centerPosition)
    {
        if (!_rewardPrefab || _rewardDropCount <= 0)
            return;

        EnsureRewardRoot();

        for (int i = 0; i < _rewardDropCount; i++)
        {
            Vector3 dropPosition = centerPosition + GetDropOffset(i);
            RewardController reward = PoolManager.MainInstance.Get(
                _rewardPrefab,
                dropPosition,
                Quaternion.identity,
                _rewardRoot);

            if (reward)
                _rewards.Add(reward);
        }
    }

    private Vector3 GetDropOffset(int index)
    {
        if (_rewardDropCount <= 1)
            return Vector3.zero;

        float angle = index * Mathf.PI * 2f / _rewardDropCount;
        const float radius = 0.25f;
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
    }

    private void StartCollectRewards()
    {
        if (_isCollectingRewards || _isCleared)
            return;

        StartCoroutine(CollectRewardsRoutine());
    }

    private IEnumerator CollectRewardsRoutine()
    {
        _isCollectingRewards = true;

        if (_collectRewardDelay > 0f)
            yield return new WaitForSeconds(_collectRewardDelay);

        _rewards.RemoveAll(reward => !reward || reward.State == RewardState.Collected);
        _pendingRewardCount = _rewards.Count;

        if (_pendingRewardCount == 0)
        {
            CompleteRoomClear();
            yield break;
        }

        if (!_player)
        {
            Debug.LogWarning(
                "RoomController has no player assigned. Rewards cannot fly to the player, so the room will clear directly.",
                this);
            CompleteRoomClear();
            yield break;
        }

        foreach (RewardController reward in _rewards)
        {
            if (!reward)
            {
                OnRewardCollected(null);
                continue;
            }

            reward.CollectTo(_player, OnRewardCollected);
        }

        float waitTimer = 0f;
        while (_pendingRewardCount > 0 && waitTimer < _maxCollectRewardWaitTime)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        CompleteRoomClear();
    }

    private void OnRewardCollected(RewardController reward)
    {
        if (_pendingRewardCount <= 0)
            return;

        _pendingRewardCount--;
    }

    private void CompleteRoomClear()
    {
        if (_isCleared)
            return;

        _pendingRewardCount = 0;
        _isCollectingRewards = false;
        _rewards.Clear();
        ClearRoom();
    }

    private void ClearRoom()
    {
        if (_isCleared)
            return;

        _isCleared = true;
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.RoomClear);

        GameEventManager.MainInstance.Call(EventName.RoomCleared, this);
    }

    private void ClearRewards()
    {
        _rewards.RemoveAll(reward => !reward);

        foreach (RewardController reward in _rewards)
        {
            if (!reward)
                continue;

            if (PoolManager.MainInstance)
                PoolManager.MainInstance.Release(reward);
            else
                Destroy(reward.gameObject);
        }

        _rewards.Clear();

        if (!_rewardRoot)
            return;

        for (int i = _rewardRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = _rewardRoot.GetChild(i);
            if (!child)
                continue;

            if (PoolManager.MainInstance)
                PoolManager.MainInstance.Release(child.gameObject);
            else
                Destroy(child.gameObject);
        }
    }
}
