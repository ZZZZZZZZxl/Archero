using UnityEngine;

[CreateAssetMenu(fileName = "PlayerExperienceConfig", menuName = "Create/Player/ExperienceConfig", order = 1)]
public class PlayerExperienceConfig : ScriptableObject
{
    [SerializeField, Min(1)] private int _baseExperienceToLevelUp = 18;
    [SerializeField, Min(1f)] private float _levelExperienceGrowth = 1.22f;
    [SerializeField, Min(1)] private int _maxLevel = 30;

    public int MaxLevel => _maxLevel;

    public int GetExperienceToNextLevel(int currentLevel)
    {
        int clampedLevel = Mathf.Clamp(currentLevel, 1, _maxLevel);
        float scaledExperience = _baseExperienceToLevelUp * Mathf.Pow(_levelExperienceGrowth, clampedLevel - 1);
        return Mathf.Max(1, Mathf.RoundToInt(scaledExperience));
    }
}
