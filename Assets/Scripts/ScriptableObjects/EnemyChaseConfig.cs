using UnityEngine;

[CreateAssetMenu(fileName = "EnemyChaseConfig", menuName = "Create/Enemy/ChaseConfig", order = 0)]
public class EnemyChaseConfig : ScriptableObject
{
    [SerializeField] private float _moveSpeed = 2.2f;
    [SerializeField] private float _turnSpeed = 720f;
    [SerializeField] private float _stopDistance = 0.85f;
    [SerializeField] private float _damageRadius = 1.05f;
    [SerializeField] private float _touchDamage = 10f;
    [SerializeField] private float _damageInterval = 0.8f;

    public float MoveSpeed => _moveSpeed;
    public float TurnSpeed => _turnSpeed;
    public float StopDistance => _stopDistance;
    public float DamageRadius => _damageRadius;
    public float TouchDamage => _touchDamage;
    public float DamageInterval => _damageInterval;
}
