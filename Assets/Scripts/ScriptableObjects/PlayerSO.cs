// namespace DefaultNamespace;

using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Create/Player/PlayerData", order = 0)]
public class PlayerSO : ScriptableObject
{
    [SerializeField, Header("移动")] private float _stopAngle = 1f;
    [SerializeField] private float _animationDampTime = 0.08f;
    [SerializeField] private float _moveRotationSmoothTime = 0.08f;
    [SerializeField] private float _aimRotationSmoothTime = 0.035f;
    
    [SerializeField, Header("敌人检测")] private LayerMask _enemyLayer;
    [SerializeField] private float _detectRadius = 8f;
    [SerializeField] private float _detectInterval = 0.05f;
    [SerializeField] private float _loseTargetExtraRange = 1.5f;
    [SerializeField] private float _retargetDistanceRatio = 0.75f;
    [SerializeField] private float _retargetDistanceBonus = 0.75f;
    [SerializeField] private float _retargetStableTime = 0.15f;
    [SerializeField] private float _retargetCooldown = 0.25f;

    
    public float StopAngle => _stopAngle;
    public float AnimationDampTime => _animationDampTime;
    public LayerMask EnemyLayer => _enemyLayer;
    public float DetectRadius => _detectRadius;
    public float DetectInterval => _detectInterval;
    public float LoseTargetExtraRange => _loseTargetExtraRange;
    public float RetargetDistanceRatio => _retargetDistanceRatio;
    public float RetargetDistanceBonus => _retargetDistanceBonus;
    public float RetargetStableTime => _retargetStableTime;
    public float RetargetCooldown => _retargetCooldown;
    public float MoveRotationSmoothTime => _moveRotationSmoothTime;
    public float AimRotationSmoothTime => _aimRotationSmoothTime;
}
