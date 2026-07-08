using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _lookTarget;
    [SerializeField] private CameraFollowConfig _config;
    
    private float _targetCameraZ;
    private float _currentFollowSpeed;
    private Vector3 _initialPosition;
    private float _lastTargetZ;
    private bool _targetMovingZ;
    private Camera _camera;
    private PlayerRuntimeContext _targetContext;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ResolveTargetContext();
        _initialPosition = transform.position;

        if (_lookTarget && _config)
        {
            float startCameraZ = GetStandardCameraZ();
            _targetCameraZ = startCameraZ;
            transform.position = new Vector3(GetStandardCameraX(), _initialPosition.y, startCameraZ);
            _lastTargetZ = _lookTarget.position.z;
        }
        else
        {
            _targetCameraZ = transform.position.z;
            _lastTargetZ = _lookTarget ? _lookTarget.position.z : transform.position.z;
        }
    }

    private void LateUpdate()
    {
        if (!_lookTarget || !_config)
            return;

        ResolveTargetContext();
        ApplyCameraView();
        UpdateTargetCameraZ();
        FollowZ();
    }

    private void ApplyCameraView()
    {
        if (!_camera)
            return;

        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = _config.BackgroundColor;
        _camera.orthographic = _config.UseOrthographic;
        if (!_camera.orthographic)
            return;

        float orthographicSize = _config.OrthographicSize;
        if (_config.FitByWorldWidth && _camera.aspect > 0f)
            orthographicSize = _config.OrthographicWorldWidth / (2f * _camera.aspect);

        _camera.orthographicSize = Mathf.Clamp(
            orthographicSize,
            _config.MinOrthographicSize,
            _config.MaxOrthographicSize
        );
    }

    private void UpdateTargetCameraZ()
    {
        float standardCameraZ = GetStandardCameraZ();
        _targetMovingZ = Mathf.Abs(_lookTarget.position.z - _lastTargetZ) > _config.MovingThreshold;

        if (!_targetMovingZ)
        {
            _targetCameraZ = standardCameraZ;
            _currentFollowSpeed = 0f;
        }
        else
        {
            float cameraDistanceFromStandard = standardCameraZ - _targetCameraZ;

            if (cameraDistanceFromStandard > _config.DeadZoneZ)
                _targetCameraZ = standardCameraZ - _config.DeadZoneZ;
            else if (cameraDistanceFromStandard < -_config.DeadZoneZ)
                _targetCameraZ = standardCameraZ + _config.DeadZoneZ;

            _currentFollowSpeed = Mathf.MoveTowards(
                _currentFollowSpeed,
                _config.MoveFollowSpeed,
                _config.MoveStartAcceleration * Time.deltaTime
            );
        }

        _lastTargetZ = _lookTarget.position.z;
    }

    private float GetStandardCameraZ()
    {
        return _lookTarget.position.z + _config.StandardOffsetZ;
    }

    private void FollowZ()
    {
        float distance = Mathf.Abs(_targetCameraZ - transform.position.z);
        if (distance <= _config.SnapDistance)
        {
            transform.position = new Vector3(
                GetStandardCameraX(),
                _initialPosition.y,
                _targetCameraZ
            );
            return;
        }

        bool hasInput = _targetContext && _targetContext.HasInput;
        float followSpeed = !hasInput
            ? _config.StopCatchUpSpeed
            : Mathf.Max(_currentFollowSpeed, 0.01f);
        

        float cameraZ = Mathf.MoveTowards(
            transform.position.z,
            _targetCameraZ,
            followSpeed * Time.deltaTime
        );
        
        cameraZ = Mathf.Clamp(cameraZ, _config.Bounds.x, _config.Bounds.y);

        transform.position = new Vector3(
            GetStandardCameraX(),
            _initialPosition.y,
            cameraZ
        );
    }

    private float GetStandardCameraX()
    {
        return _config ? _config.StandardPositionX : _initialPosition.x;
    }

    private void ResolveTargetContext()
    {
        if (_targetContext || !_lookTarget)
            return;

        _targetContext = _lookTarget.GetComponentInParent<PlayerRuntimeContext>();
    }
}
