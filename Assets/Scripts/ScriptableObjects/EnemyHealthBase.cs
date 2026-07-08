using UnityEngine;

[CreateAssetMenu(fileName = "EnemyHeathData", menuName = "Create/Enemy/EnemyHealth", order = 0)]
public class EnemyHealthBase : ScriptableObject
{
    [SerializeField] private float _maxHp = 100f;
    [SerializeField, Min(0)] private int _killExperience = 6;

    public float MaxHp => _maxHp;
    public int KillExperience => _killExperience;
}
