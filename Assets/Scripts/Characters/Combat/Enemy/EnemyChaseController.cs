using GGG.Tool;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaseController : MonoBehaviour
{
    [SerializeField] private EnemyChaseConfig _config;
    [SerializeField] private Transform _target;

    private NavMeshAgent _agent;
    private float _lastDamageTime = -999f;
    private bool _movementLocked;
    private bool _hasWarnedMissingAgent;

    public EnemyChaseConfig Config => _config;
    public bool HasConfig => _config != null;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!_agent)
            _agent = gameObject.AddComponent<NavMeshAgent>();

        if (_agent)
        {
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }
    }

    private void OnEnable()
    {
        _lastDamageTime = Time.time;
    }

    public void Configure(EnemyChaseConfig config)
    {
        if (config)
            _config = config;

        ApplyAgentConfig();
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;

        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        if (locked)
            _agent.ResetPath();
    }

    private void Start()
    {
        ResolveTarget();
    }

    private void Update()
    {
        if (!CanChase())
            return;

        Chase();
        TriggerDamage();
    }

    private bool CanChase()
    {
        ResolveTarget();

        if (!_target || !_config)
            return false;

        if (GameFlowManager.MainInstance && !GameFlowManager.MainInstance.IsPlaying)
            return false;

        return !_movementLocked;
    }

    private void Chase()
    {
        MoveToTarget();
        RotateToTarget();
    }

    private void MoveToTarget()
    {
        Vector3 delta = _target.position - transform.position;
        delta.y = 0f;

        if (delta.magnitude <= _config.StopDistance)
        {
            if (_agent && _agent.enabled && _agent.isOnNavMesh)
                _agent.ResetPath();

            return;
        }

        if (_agent && _agent.enabled && _agent.isOnNavMesh)
        {
            _agent.speed = _config.MoveSpeed;
            _agent.stoppingDistance = _config.StopDistance;
            _agent.SetDestination(_target.position);
            return;
        }

        WarnMissingAgentOnce();
    }

    private void RotateToTarget()
    {
        Vector3 direction = DevelopmentToos.DirectionForTarget(transform, _target);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _config.TurnSpeed * Time.deltaTime
        );
    }

    private void TriggerDamage()
    {
        Vector3 delta = _target.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > _config.DamageRadius * _config.DamageRadius)
            return;

        TryDamageToPlayer(_target);
    }

    private void TryDamageToPlayer(Component other)
    {
        if (!_config || Time.time - _lastDamageTime < _config.DamageInterval)
            return;

        if (!other.GetComponentInParent<PlayerController>())
            return;

        _lastDamageTime = Time.time;
        GameEventManager.MainInstance.Call(
            EventName.PlayerHit,
            _config.TouchDamage,
            transform.position,
            transform
        );
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamageToPlayer(collision.collider);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamageToPlayer(other);
    }

    private void ResolveTarget()
    {
        if (_target)
            return;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player)
            _target = player.transform;
    }

    private void ApplyAgentConfig()
    {
        if (!_agent || !_config)
            return;

        _agent.speed = _config.MoveSpeed;
        _agent.stoppingDistance = _config.StopDistance;
        _agent.angularSpeed = _config.TurnSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    private void WarnMissingAgentOnce()
    {
        if (_hasWarnedMissingAgent)
            return;

        _hasWarnedMissingAgent = true;
        Debug.LogWarning(
            $"{name} cannot chase because its NavMeshAgent is missing, disabled, or not on NavMesh.",
            this
        );
    }
}
