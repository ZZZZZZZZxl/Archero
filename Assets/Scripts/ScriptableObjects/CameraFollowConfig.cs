using UnityEngine;

[CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "Create/Camera/FollowConfig", order = 0)]
public class CameraFollowConfig : ScriptableObject
{
    [Header("View")]
    [SerializeField] private bool _useOrthographic = true;
    [SerializeField] private bool _fitByWorldWidth = false;
    [SerializeField] private float _orthographicWorldWidth = 8.5f;
    [SerializeField] private float _orthographicSize = 6.2f;
    [SerializeField] private float _minOrthographicSize = 5.5f;
    [SerializeField] private float _maxOrthographicSize = 8f;
    [SerializeField] private Color _backgroundColor = new Color(0.08f, 0.11f, 0.16f, 1f);

    [Header("Follow")]
    [SerializeField] private float _standardPositionX = 0f;
    [SerializeField] private float _standardOffsetZ = -5.99f;
    [SerializeField] private float _deadZoneZ = 1.25f;
    [SerializeField] private float _moveFollowSpeed = 10f;
    [SerializeField] private float _moveStartAcceleration = 35f;
    [SerializeField] private float _stopCatchUpSpeed = 14f;
    [SerializeField] private float _movingThreshold = 0.001f;
    [SerializeField] private float _snapDistance = 0.02f;

    [Header("Bounds")]
    [SerializeField] private Vector2 _bounds = new Vector2(-15f, -8.86f);

    public bool UseOrthographic => _useOrthographic;
    public bool FitByWorldWidth => _fitByWorldWidth;
    public float OrthographicWorldWidth => _orthographicWorldWidth;
    public float OrthographicSize => _orthographicSize;
    public float MinOrthographicSize => _minOrthographicSize;
    public float MaxOrthographicSize => _maxOrthographicSize;
    public Color BackgroundColor => _backgroundColor;
    public float StandardPositionX => _standardPositionX;
    public float StandardOffsetZ => _standardOffsetZ;
    public float DeadZoneZ => _deadZoneZ;
    public float MoveFollowSpeed => _moveFollowSpeed;
    public float MoveStartAcceleration => _moveStartAcceleration;
    public float StopCatchUpSpeed => _stopCatchUpSpeed;
    public float MovingThreshold => _movingThreshold;
    public float SnapDistance => _snapDistance;
    public Vector2 Bounds => _bounds;
}
