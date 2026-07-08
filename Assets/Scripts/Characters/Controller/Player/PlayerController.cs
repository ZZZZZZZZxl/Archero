using GGG.Tool;
using UnityEngine;

[RequireComponent(typeof(PlayerRuntimeContext))]
public class PlayerController : CharacterMovementBase
{
    [SerializeField] private VirtualJoystick JoyStick;
    [SerializeField] private PlayerSO _data;

    private float _moveSpeed;
    private float _rotationVelocityY;
    private Vector3 _inputDirection;
    private Vector3 _targetDirection;
    private PlayerRuntimeContext _runtimeContext;

    public PlayerSO Data => _data;
    public PlayerRuntimeContext RuntimeContext => _runtimeContext;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
        _runtimeContext = GetComponent<PlayerRuntimeContext>();
        if (!_runtimeContext)
            _runtimeContext = gameObject.AddComponent<PlayerRuntimeContext>();

        _runtimeContext.Configure(_data);
    }

    protected override void Update()
    {
        base.Update();
        ReadJoyStick();
        UpdateMoveAnimationParam();
        UpdateRotation();
    }

    private void ReadJoyStick()
    {
        if (!JoyStick)
        {
            DevelopmentToos.Debug("没有 JoyStick");
            return;
        }

        _moveSpeed = JoyStick.SpeedFactor;
        _runtimeContext.SetHasInput(JoyStick.HasInput);
        _inputDirection = JoyStick.Direction;
    }

    private void UpdateMoveAnimationParam()
    {
        if (!_animator)
        {
            DevelopmentToos.Debug("没有 Animator");
            return;
        }

        _animator.SetFloat(AnimationParams.MoveSpeed, _moveSpeed, _data.AnimationDampTime, Time.deltaTime);
        _animator.SetBool(AnimationParams.HasInput, _runtimeContext.HasInput);
    }

    private void UpdateRotation()
    {
        float rotationSmoothTime;

        if (_runtimeContext.HasInput)
        {
            _targetDirection.Set(_inputDirection.x, 0f, _inputDirection.y);
            rotationSmoothTime = _data.MoveRotationSmoothTime;
        }
        else if (_runtimeContext.CurrentEnemy)
        {
            _targetDirection = _runtimeContext.CurrentEnemy.position - transform.position;
            _targetDirection.y = 0f;
            rotationSmoothTime = _data.AimRotationSmoothTime;
        }
        else
        {
            return;
        }

        if (_targetDirection.sqrMagnitude < 0.0001f)
            return;

        float deltaAngle = DevelopmentToos.GetDeltaAngle(transform, _targetDirection);
        if (Mathf.Abs(deltaAngle) <= _data.StopAngle)
            return;

        SmoothRotation(rotationSmoothTime);
    }

    private void SmoothRotation(float smoothTime)
    {
        float targetRotationY = Mathf.Atan2(_targetDirection.x, _targetDirection.z) * Mathf.Rad2Deg;

        float rotationY = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetRotationY,
            ref _rotationVelocityY,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (_data == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _data.DetectRadius);
    }
}
