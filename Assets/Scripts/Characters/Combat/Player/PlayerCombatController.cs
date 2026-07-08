using GGG.Tool;
using UnityEngine;

[RequireComponent(typeof(PlayerRuntimeContext))]
public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private PlayerWeaponConfig _weaponConfig;
    [SerializeField] private ProjectileArrow _arrowPrefab;
    [SerializeField] private Transform _firePoint;

    private float _passTime;
    private Animator _animator;
    private PlayerRuntimeContext _runtimeContext;
    private PlayerUpgradeController _upgradeController;
    private int _shootStateHash;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _runtimeContext = GetComponent<PlayerRuntimeContext>();
        if (!_runtimeContext)
            _runtimeContext = gameObject.AddComponent<PlayerRuntimeContext>();

        _upgradeController = GetComponent<PlayerUpgradeController>();
        _shootStateHash = Animator.StringToHash(AnimationName.Shoot);
    }

    private void Update()
    {
        Attack();
    }

    private void LateUpdate()
    {
        ResetAnimatorSpeedAfterShoot();
    }


    #region Attack
    
    private void Attack()
    {
        if (!CheckAttack())
        {
            ResetAttack();
            return;
        }

        if (!_runtimeContext.CurrentEnemy)
        {
            ResetAttack();
            return;
        }

        DoAttack();
    }

    private bool CheckAttack()
    {
        if (!_weaponConfig || !_arrowPrefab || !_firePoint)
            return false;

        return !_runtimeContext.HasInput;
    }

    private void DoAttack()
    {
        _passTime += Time.deltaTime;
        if (_passTime < GetAttackTimer())
            return;

        Shoot();
        _passTime = 0f;
    }

    private void Shoot()
    {
        if (!_animator)
            return;

        _animator.speed = GetAttackSpeedMultiplier();
        _animator.CrossFade(AnimationName.Shoot, 0.2f);
    }

    private void ResetAttack()
    {
        _passTime = 0f;
        if (!_animator)
            return;

        _animator.SetBool(AnimationParams.Shoot, false);
        _animator.speed = 1f;
    }

    private float GetAttackTimer()
    {
        return _weaponConfig.AttackTimer / GetAttackSpeedMultiplier();
    }

    private float GetAttackSpeedMultiplier()
    {
        return _upgradeController ? _upgradeController.AttackSpeedMultiplier : 1f;
    }

    private void ResetAnimatorSpeedAfterShoot()
    {
        if (!_animator || Mathf.Approximately(_animator.speed, 1f))
            return;

        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        bool isShoot = currentState.shortNameHash == _shootStateHash;

        if (_animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
            isShoot = isShoot || nextState.shortNameHash == _shootStateHash;
        }

        if (!isShoot)
            _animator.speed = 1f;
    }

    #endregion


    #region AnimationEvent

    private void CreateArrow()
    {
        Transform enemy = _runtimeContext.CurrentEnemy;
        if (!enemy) return;
        Vector3 targetPoint = DevelopmentToos.GetTargetPoint(enemy);
        Vector3 direction = targetPoint - _firePoint.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        direction.y = 0;

        CreateArrowPattern(direction);

        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.PlayerShoot);
    }

    private void CreateArrowPattern(Vector3 direction)
    {
        int frontArrowCount = 1 + (_upgradeController ? _upgradeController.ExtraFrontArrowCount : 0);
        for (int i = 0; i < frontArrowCount; i++)
        {
            Vector3 offset = GetFrontArrowOffset(direction, i, frontArrowCount);
            CreateSingleArrow(direction, offset);
        }

        int diagonalPairCount = _upgradeController ? _upgradeController.DiagonalArrowPairCount : 0;
        for (int i = 0; i < diagonalPairCount; i++)
        {
            float angle = 25f + i * 15f;
            CreateSingleArrow(RotateDirection(direction, -angle), Vector3.zero);
            CreateSingleArrow(RotateDirection(direction, angle), Vector3.zero);
        }
    }

    private void CreateSingleArrow(Vector3 direction, Vector3 positionOffset)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.y = 0f;
        direction.Normalize();

        ProjectileArrow arrow = PoolManager.MainInstance.Get(
            _arrowPrefab,
            _firePoint.position + positionOffset,
            Quaternion.LookRotation(direction)
        );

        arrow.InitConfig(
            direction,
            _weaponConfig.FlySpeed,
            _weaponConfig.MaxDistance,
            GetDamage(),
            _weaponConfig.TargetLayerMask,
            _weaponConfig.KnockbackDistance,
            _weaponConfig.KnockbackDuration,
            _weaponConfig.StayTime,
            _weaponConfig.EnvLayerMask
        );
    }

    private float GetDamage()
    {
        float multiplier = _upgradeController ? _upgradeController.DamageMultiplier : 1f;
        return _weaponConfig.Damage * multiplier;
    }

    private static Vector3 GetFrontArrowOffset(Vector3 direction, int index, int count)
    {
        if (count <= 1)
            return Vector3.zero;

        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        float centeredIndex = index - (count - 1) * 0.5f;
        const float spacing = 0.18f;
        return right * centeredIndex * spacing;
    }

    private static Vector3 RotateDirection(Vector3 direction, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.up) * direction;
    }

    #endregion
}
