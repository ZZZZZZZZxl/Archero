using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerExperienceController))]
[RequireComponent(typeof(PlayerHealthController))]
public class PlayerUpgradeController : MonoBehaviour
{
    [SerializeField] private PlayerUpgradeConfig _config;

    private readonly Dictionary<PlayerUpgradeType, int> _stacks = new Dictionary<PlayerUpgradeType, int>();
    private PlayerExperienceController _experienceController;
    private PlayerHealthController _healthController;
    private int _pendingUpgradeCount;
    private bool _isChoosingUpgrade;

    public float AttackSpeedMultiplier { get; private set; } = 1f;
    public float DamageMultiplier { get; private set; } = 1f;
    public int ExtraFrontArrowCount { get; private set; }
    public int DiagonalArrowPairCount { get; private set; }

    public event Action<IReadOnlyList<PlayerUpgradeOption>> UpgradeChoicesReadyEvent;

    private void Awake()
    {
        _experienceController = GetComponent<PlayerExperienceController>();
        _healthController = GetComponent<PlayerHealthController>();
    }

    private void OnEnable()
    {
        if (_experienceController)
            _experienceController.LevelUpEvent += OnLevelUp;
    }

    private void OnDisable()
    {
        if (_experienceController)
            _experienceController.LevelUpEvent -= OnLevelUp;
    }

    public void ResetUpgrades()
    {
        _stacks.Clear();
        AttackSpeedMultiplier = 1f;
        DamageMultiplier = 1f;
        ExtraFrontArrowCount = 0;
        DiagonalArrowPairCount = 0;
        _pendingUpgradeCount = 0;
        _isChoosingUpgrade = false;

        if (_healthController)
            _healthController.ResetBonusMaxHealth();
    }

    public void ApplyUpgrade(PlayerUpgradeOption option)
    {
        if (option == null || !CanSelect(option))
            return;

        AddStack(option.Type);

        switch (option.Type)
        {
            case PlayerUpgradeType.AttackSpeed:
                AttackSpeedMultiplier += option.Amount;
                break;
            case PlayerUpgradeType.Damage:
                DamageMultiplier += option.Amount;
                break;
            case PlayerUpgradeType.FrontArrow:
                ExtraFrontArrowCount += Mathf.Max(1, Mathf.RoundToInt(option.Amount));
                break;
            case PlayerUpgradeType.DiagonalArrows:
                DiagonalArrowPairCount += Mathf.Max(1, Mathf.RoundToInt(option.Amount));
                break;
            case PlayerUpgradeType.MaxHealth:
                if (_healthController)
                {
                    _healthController.AddMaxHealth(option.Amount, true);
                    if (UIManager.MainInstance)
                        UIManager.MainInstance.RefreshHealth();
                }
                break;
        }

        _pendingUpgradeCount = Mathf.Max(0, _pendingUpgradeCount - 1);
        _isChoosingUpgrade = false;

        if (_pendingUpgradeCount > 0)
        {
            ShowNextUpgradeChoice();
            return;
        }

        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.CompleteUpgradeSelection();
    }

    private void OnLevelUp(int level)
    {
        if (_healthController && _config)
            _healthController.HealPercentOfMax(_config.LevelUpHealPercent);

        _pendingUpgradeCount++;
        ShowNextUpgradeChoice();
    }

    private void ShowNextUpgradeChoice()
    {
        if (_isChoosingUpgrade)
            return;

        List<PlayerUpgradeOption> choices = BuildChoices(3);
        if (choices.Count == 0)
        {
            _pendingUpgradeCount = 0;
            _isChoosingUpgrade = false;
            if (GameFlowManager.MainInstance)
                GameFlowManager.MainInstance.CompleteUpgradeSelection();
            return;
        }

        _isChoosingUpgrade = true;

        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.BeginUpgradeSelection();

        if (UIManager.MainInstance)
        {
            UIManager.MainInstance.ShowUpgradeChoices(choices, ApplyUpgrade);
        }
        else
        {
            ApplyUpgrade(choices[0]);
            return;
        }

        UpgradeChoicesReadyEvent?.Invoke(choices);
    }

    private List<PlayerUpgradeOption> BuildChoices(int count)
    {
        List<PlayerUpgradeOption> pool = new List<PlayerUpgradeOption>();
        if (_config == null)
            return pool;

        foreach (PlayerUpgradeOption option in _config.Options)
        {
            if (CanSelect(option))
                pool.Add(option);
        }

        List<PlayerUpgradeOption> choices = new List<PlayerUpgradeOption>();
        while (pool.Count > 0 && choices.Count < count)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            choices.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return choices;
    }

    private bool CanSelect(PlayerUpgradeOption option)
    {
        if (option == null)
            return false;

        int maxStacks = option.MaxStacks;
        return maxStacks <= 0 || GetStack(option.Type) < maxStacks;
    }

    private void AddStack(PlayerUpgradeType type)
    {
        _stacks[type] = GetStack(type) + 1;
    }

    private int GetStack(PlayerUpgradeType type)
    {
        return _stacks.TryGetValue(type, out int stack) ? stack : 0;
    }
}
