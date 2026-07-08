using System;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    [SerializeField] private EnemyHealthBase _healthBase;
    [SerializeField] private float _fallbackMaxHp = 20f;

    private float _maxHp;
    private float _currentHp = 1;

    private bool _isDead => _currentHp <= 0;
    public bool IsDead => _isDead;
    public int KillExperience => _healthBase ? _healthBase.KillExperience : 0;

    public event Action<EnemyHealthController> DiedEvent;

    private void Start()
    {
        InitHealth();
    }

    private void InitHealth()
    {
        _maxHp = _healthBase ? _healthBase.MaxHp : _fallbackMaxHp;
        _currentHp = _maxHp;
    }


    public void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0f, _maxHp);

        if (_isDead)
            Die();
    }
    

    private void Die()
    {
        GameEventManager.MainInstance.Call(EventName.EnemyDied, this);
        GameEventManager.MainInstance.Call(EventName.EnemyKilled, KillExperience);
        DiedEvent?.Invoke(this);
    }
}
