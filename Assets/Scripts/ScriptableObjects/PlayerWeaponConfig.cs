using UnityEngine;

[CreateAssetMenu(fileName = "PlayerWeaponConfig", menuName = "Create/Player/WeaponConfigs", order = 0)]
public class PlayerWeaponConfig : ScriptableObject
{
    [SerializeField, Header("攻击设置")] private float _attackTimer = 0.45f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _maxDistance = 8f;
    [SerializeField] private float _flySpeed = 12f;
    [SerializeField] private LayerMask _targetLayerMask;

    [SerializeField, Header("击退设置")] private float _knockbackDistance = 0.25f;
    [SerializeField] private float _knockbackDuration = 0.08f;

    [SerializeField, Header("停墙设置")] private float _stayTime = 1f;
    [SerializeField] private LayerMask _envLayerMask;

    public float AttackTimer => _attackTimer;
    public float Damage => _damage;
    public float MaxDistance => _maxDistance;
    public float FlySpeed => _flySpeed;
    public LayerMask TargetLayerMask => _targetLayerMask;
    public float KnockbackDistance => _knockbackDistance;
    public float KnockbackDuration => _knockbackDuration;
    public float StayTime => _stayTime;
    public LayerMask EnvLayerMask => _envLayerMask;
}
