using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerUpgradeType
{
    AttackSpeed,
    Damage,
    FrontArrow,
    DiagonalArrows,
    MaxHealth
}

[Serializable]
public class PlayerUpgradeOption
{
    [SerializeField] private PlayerUpgradeType _type;
    [SerializeField] private string _title;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private float _amount;
    [SerializeField, Min(0)] private int _maxStacks;

    public PlayerUpgradeType Type => _type;
    public string Title => _title;
    public string Description => _description;
    public float Amount => _amount;
    public int MaxStacks => _maxStacks;
}

[CreateAssetMenu(fileName = "PlayerUpgradeConfig", menuName = "Create/Player/UpgradeConfig", order = 2)]
public class PlayerUpgradeConfig : ScriptableObject
{
    [SerializeField, Range(0f, 1f)] private float _levelUpHealPercent = 0.1f;
    [SerializeField] private List<PlayerUpgradeOption> _options = new List<PlayerUpgradeOption>();

    public float LevelUpHealPercent => _levelUpHealPercent;
    public IReadOnlyList<PlayerUpgradeOption> Options => _options;
}
