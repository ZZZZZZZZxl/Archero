using GGG.Tool;
using UnityEngine;

public class ProjectileArrow : MonoBehaviour
{
    private float _speed;
    private float _damage;
    private float _maxDistance;
    private float _currentDistance;
    private bool _hasLaunched;
    private Vector3 _direction;
    private LayerMask _targetLayerMask;
    private float _knockBackDistance;
    private float _knockBackDuration;
    private float _stayTime;
    private LayerMask _envLayerMask;

    private float _stayPassTime;
    private bool _hasStay;
    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void Update()
    {
        if (!_hasLaunched)
            return;

        if (_hasStay)
        {
            UpdateStay();
            return;
        }

        Fly();
        CheckEnd();
    }

    public void InitConfig(
        Vector3 direction,
        float speed,
        float maxDistance,
        float damage,
        LayerMask targetLayerMask,
        float knockBackDistance,
        float knockBackDuration,
        float stayTime,
        LayerMask envLayerMask
    )
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        _direction = direction.normalized;
        _speed = speed;
        _maxDistance = maxDistance;
        _damage = damage;
        _targetLayerMask = targetLayerMask;
        _currentDistance = 0f;
        _hasLaunched = true;
        _knockBackDistance = knockBackDistance;
        _knockBackDuration = knockBackDuration;
        _stayTime = stayTime;
        _envLayerMask = envLayerMask;
        _stayPassTime = 0f;
        _hasStay = false;
        transform.localScale = _initialScale;
        transform.rotation = Quaternion.LookRotation(_direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasLaunched || _hasStay)
            return;

        if (DevelopmentToos.IsSameLayerMask(other.gameObject, _targetLayerMask))
        {
            OnCollideEnemy(other);
            return;
        }

        if (DevelopmentToos.IsSameLayerMask(other.gameObject, _envLayerMask))
            OnCollideEnv();
    }

    private void UpdateStay()
    {
        _stayPassTime += Time.deltaTime;
        if (_stayPassTime >= _stayTime)
            DestroyGameObject();
    }

    private void OnCollideEnv()
    {
        _stayPassTime = 0f;
        _hasStay = true;
    }

    private void OnCollideEnemy(Collider other)
    {
        EnemyCombatController enemyCombatCtrl = other.GetComponentInParent<EnemyCombatController>();
        if (enemyCombatCtrl)
        {
            enemyCombatCtrl.OnHit(
                _damage,
                _direction,
                _knockBackDistance,
                _knockBackDuration
            );
        }

        DestroyGameObject();
    }

    private void DestroyGameObject()
    {
        _hasLaunched = false;

        if (PoolManager.MainInstance)
            PoolManager.MainInstance.Release(this);
        else
            gameObject.SetActive(false);
    }

    private void Fly()
    {
        float moveDistance = Time.deltaTime * _speed;
        transform.position += _direction * moveDistance;
        _currentDistance += moveDistance;
    }

    private void CheckEnd()
    {
        if (_currentDistance >= _maxDistance)
            DestroyGameObject();
    }

    private void ResetState()
    {
        _speed = 0f;
        _damage = 0f;
        _maxDistance = 0f;
        _currentDistance = 0f;
        _hasLaunched = false;
        _direction = Vector3.zero;
        _targetLayerMask = 0;
        _knockBackDistance = 0f;
        _knockBackDuration = 0f;
        _stayTime = 0f;
        _envLayerMask = 0;
        _stayPassTime = 0f;
        _hasStay = false;
        transform.localScale = _initialScale;
    }
}
