using UnityEngine;

[RequireComponent(typeof(PlayerRuntimeContext))]
public class PlayerDetectionController : MonoBehaviour
{
    private readonly Collider[] _enemyResults = new Collider[32];

    private PlayerRuntimeContext _runtimeContext;
    private Transform _candidateEnemy;
    private float _detectTimer;
    private float _retargetStableTimer;
    private float _retargetCooldownTimer;

    public Transform CurrentEnemy { get; private set; }

    private PlayerSO Config => _runtimeContext ? _runtimeContext.Config : null;

    private void Awake()
    {
        _runtimeContext = GetComponent<PlayerRuntimeContext>();
        if (!_runtimeContext)
            _runtimeContext = gameObject.AddComponent<PlayerRuntimeContext>();
    }

    private void OnEnable()
    {
        GameEventManager.MainInstance.AddEvent<EnemyHealthController>(EventName.EnemyDied, OnEnemyDied);
    }

    private void OnDisable()
    {
        GameEventManager.MainInstance.RemoveEvent<EnemyHealthController>(EventName.EnemyDied, OnEnemyDied);
        ClearCurrentEnemy();
    }

    private void FixedUpdate()
    {
        if (!Config)
            return;

        UpdateTimers();

        _detectTimer -= Time.fixedDeltaTime;
        if (_detectTimer <= 0f)
        {
            _detectTimer = Mathf.Max(0.01f, Config.DetectInterval);
            UpdateCurrentEnemy();
        }

        _runtimeContext.SetCurrentEnemy(CurrentEnemy);
    }

    public void ClearCurrentEnemy()
    {
        SetEnemySelected(CurrentEnemy, false);
        CurrentEnemy = null;
        _runtimeContext?.ClearCurrentEnemy();
        ResetCandidate();
        _retargetCooldownTimer = 0f;
    }

    private void UpdateTimers()
    {
        if (_retargetCooldownTimer > 0f)
            _retargetCooldownTimer -= Time.fixedDeltaTime;
    }

    private void UpdateCurrentEnemy()
    {
        if (!IsValidCurrentEnemy(CurrentEnemy))
        {
            SetCurrentEnemy(FindNearestAliveEnemy());
            return;
        }

        if (!_runtimeContext.HasInput)
        {
            ResetCandidate();
            return;
        }

        Transform nearestEnemy = FindNearestAliveEnemy();
        if (!nearestEnemy || nearestEnemy == CurrentEnemy)
        {
            ResetCandidate();
            return;
        }

        if (!CanRetargetTo(nearestEnemy))
        {
            ResetCandidate();
            return;
        }

        UpdateRetargetCandidate(nearestEnemy);
    }

    private bool IsValidCurrentEnemy(Transform enemy)
    {
        if (!enemy || !Config)
            return false;

        EnemyHealthController health = enemy.GetComponent<EnemyHealthController>();
        if (!health || health.IsDead)
            return false;

        float loseRange = Config.DetectRadius + Config.LoseTargetExtraRange;
        float sqrLoseRange = loseRange * loseRange;
        return (enemy.position - transform.position).sqrMagnitude <= sqrLoseRange;
    }

    private bool CanRetargetTo(Transform newEnemy)
    {
        if (_retargetCooldownTimer > 0f || !CurrentEnemy || !Config)
            return false;

        float currentDistance = Vector3.Distance(transform.position, CurrentEnemy.position);
        float newDistance = Vector3.Distance(transform.position, newEnemy.position);

        bool ratioBetter = newDistance <= currentDistance * Config.RetargetDistanceRatio;
        bool bonusBetter = newDistance + Config.RetargetDistanceBonus <= currentDistance;

        return ratioBetter || bonusBetter;
    }

    private void UpdateRetargetCandidate(Transform candidate)
    {
        if (!Config)
            return;

        if (_candidateEnemy != candidate)
        {
            _candidateEnemy = candidate;
            _retargetStableTimer = 0f;
            return;
        }

        _retargetStableTimer += Mathf.Max(0.01f, Config.DetectInterval);
        if (_retargetStableTimer < Config.RetargetStableTime)
            return;

        SetCurrentEnemy(candidate);
    }

    private void SetCurrentEnemy(Transform enemy)
    {
        if (CurrentEnemy == enemy)
        {
            _runtimeContext.SetCurrentEnemy(CurrentEnemy);
            return;
        }

        SetEnemySelected(CurrentEnemy, false);

        CurrentEnemy = enemy;
        _runtimeContext.SetCurrentEnemy(CurrentEnemy);
        SetEnemySelected(CurrentEnemy, true);

        if (Config)
            _retargetCooldownTimer = Config.RetargetCooldown;

        ResetCandidate();
    }

    private void OnEnemyDied(EnemyHealthController enemy)
    {
        if (!enemy || CurrentEnemy != enemy.transform)
            return;

        ClearCurrentEnemy();
    }

    private static void SetEnemySelected(Transform enemy, bool isSelected)
    {
        if (!enemy)
            return;

        if (enemy.TryGetComponent(out EnemyController enemyController))
            enemyController.SetSelected(isSelected);
    }

    private void ResetCandidate()
    {
        _candidateEnemy = null;
        _retargetStableTimer = 0f;
    }

    private Transform FindNearestAliveEnemy()
    {
        if (!Config)
            return null;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            Config.DetectRadius,
            _enemyResults,
            Config.EnemyLayer,
            QueryTriggerInteraction.Ignore
        );

        Transform nearestEnemy = null;
        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider enemyCollider = _enemyResults[i];
            if (!enemyCollider)
                continue;

            Transform enemyTransform = GetAliveEnemyTransform(enemyCollider);
            if (!enemyTransform)
                continue;

            float sqrDistance = (enemyTransform.position - transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestEnemy = enemyTransform;
            }
        }

        return nearestEnemy;
    }

    private static Transform GetAliveEnemyTransform(Collider enemyCollider)
    {
        EnemyHealthController health = enemyCollider.GetComponentInParent<EnemyHealthController>();
        if (health)
            return health.IsDead ? null : health.transform;

        return null;
    }
}
