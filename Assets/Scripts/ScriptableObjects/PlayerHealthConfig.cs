using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthConfig", menuName = "Create/Player/HealthConfig", order = 0)]
public class PlayerHealthConfig : ScriptableObject
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _damageInvincibleTime = 0.35f;

    public float MaxHealth => _maxHealth;
    public float DamageInvincibleTime => _damageInvincibleTime;
}
