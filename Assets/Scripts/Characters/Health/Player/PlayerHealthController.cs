using System;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    [SerializeField] private PlayerHealthConfig _config;

    private float _currentHealth;
    private float _bonusMaxHealth;
    private float _lastDamageTime = -999f;
    private HitFlashFeedback _hitFlashFeedback;

    public float MaxHealth => (_config ? _config.MaxHealth : 1f) + _bonusMaxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsDead => _currentHealth <= 0f;

    public event Action<float, float> HealthChangedEvent;
    public event Action DiedEvent;

    private void Start()
    {
        ResolveFeedback();
        ResetHealth();
    }

    private void OnEnable()
    {
        GameEventManager.MainInstance.AddEvent<float, Vector3, Transform>(EventName.PlayerHit, OnPlayerHit);
    }

    private void OnDisable()
    {
        GameEventManager.MainInstance.RemoveEvent<float, Vector3, Transform>(EventName.PlayerHit, OnPlayerHit);
    }

    public void ResetHealth()
    {
        _currentHealth = MaxHealth;
        _lastDamageTime = -999f;
        HealthChangedEvent?.Invoke(_currentHealth, MaxHealth);
    }

    public void ResetBonusMaxHealth()
    {
        _bonusMaxHealth = 0f;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, MaxHealth);
        HealthChangedEvent?.Invoke(_currentHealth, MaxHealth);
    }

    public void HealPercentOfMax(float percent)
    {
        Heal(MaxHealth * Mathf.Clamp01(percent));
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0f, MaxHealth);
        HealthChangedEvent?.Invoke(_currentHealth, MaxHealth);
    }

    public void AddMaxHealth(float amount, bool healSameAmount)
    {
        if (amount <= 0f)
            return;

        _bonusMaxHealth += amount;

        if (healSameAmount)
            _currentHealth += amount;

        _currentHealth = Mathf.Clamp(_currentHealth, 0f, MaxHealth);
        HealthChangedEvent?.Invoke(_currentHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        if (Time.time - _lastDamageTime < (_config ? _config.DamageInvincibleTime : 0f))
            return;

        _lastDamageTime = Time.time;
        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, MaxHealth);
        HealthChangedEvent?.Invoke(_currentHealth, MaxHealth);

        ResolveFeedback();
        if (_hitFlashFeedback)
            _hitFlashFeedback.Play();

        ScreenHitFlash.Play();

        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.PlayerHit);

        if (IsDead)
            DiedEvent?.Invoke();
    }

    private void OnPlayerHit(float damage, Vector3 hitPoint, Transform attacker)
    {
        TakeDamage(damage);
    }

    private void ResolveFeedback()
    {
        if (_hitFlashFeedback)
            return;

        _hitFlashFeedback = GetComponent<HitFlashFeedback>();
        if (!_hitFlashFeedback)
            _hitFlashFeedback = gameObject.AddComponent<HitFlashFeedback>();
    }
}
