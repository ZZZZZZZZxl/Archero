using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelCatalog", menuName = "Create/Room/LevelCatalog", order = 1)]
public class RoomLevelCatalog : ScriptableObject
{
    [SerializeField] private List<RoomLevelConfig> _levels = new List<RoomLevelConfig>();

    public IReadOnlyList<RoomLevelConfig> Levels => _levels;
    public int Count => _levels.Count;

    public bool TryGetLevel(int index, out RoomLevelConfig levelConfig)
    {
        levelConfig = null;

        if (_levels == null || _levels.Count == 0)
            return false;

        if (index < 0 || index >= _levels.Count)
            return false;

        levelConfig = _levels[index];
        return levelConfig != null;
    }
}
