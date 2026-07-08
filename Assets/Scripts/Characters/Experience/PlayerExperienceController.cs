using System;
using UnityEngine;

public class PlayerExperienceController : MonoBehaviour
{
    [SerializeField] private PlayerExperienceConfig _config;

    private int _level = 1;
    private int _currentExperience;

    public int Level => _level;
    public int CurrentExperience => _currentExperience;
    public int ExperienceToNextLevel => _config ? _config.GetExperienceToNextLevel(_level) : 1;
    public bool IsMaxLevel => _config && _level >= _config.MaxLevel;

    public event Action<int, int, int> ExperienceChangedEvent;
    public event Action<int> LevelUpEvent;

    private void OnEnable()
    {
        GameEventManager.MainInstance.AddEvent<int>(EventName.EnemyKilled, AddExperience);
    }

    private void OnDisable()
    {
        GameEventManager.MainInstance.RemoveEvent<int>(EventName.EnemyKilled, AddExperience);
    }

    private void Start()
    {
        NotifyExperienceChanged();
    }

    public void ResetExperience()
    {
        _level = 1;
        _currentExperience = 0;
        NotifyExperienceChanged();
    }

    public void AddExperience(int amount)
    {
        if (!_config || amount <= 0 || IsMaxLevel)
            return;

        _currentExperience += amount;

        while (!IsMaxLevel && _currentExperience >= ExperienceToNextLevel)
        {
            _currentExperience -= ExperienceToNextLevel;
            _level++;
            LevelUpEvent?.Invoke(_level);
        }

        if (IsMaxLevel)
            _currentExperience = 0;

        NotifyExperienceChanged();
    }

    private void NotifyExperienceChanged()
    {
        ExperienceChangedEvent?.Invoke(_level, _currentExperience, ExperienceToNextLevel);
    }
}
