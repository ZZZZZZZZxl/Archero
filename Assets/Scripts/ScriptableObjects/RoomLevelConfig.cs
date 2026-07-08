using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelConfig", menuName = "Create/Room/LevelConfig", order = 0)]
public class RoomLevelConfig : ScriptableObject
{
    [SerializeField] private bool _useRandomSeed = true;
    [SerializeField] private int _seed = 1;
    [SerializeField] private Vector2 _spawnAreaCenter = new Vector2(0f, -1.2f);
    [SerializeField] private Vector2 _spawnAreaSize = new Vector2(7.2f, 8.5f);
    [SerializeField] private float _spawnY = 0f;
    [SerializeField] private float _playerSafeRadius = 2.4f;
    [SerializeField] private float _defaultSpawnRadius = 1.2f;
    [SerializeField] private int _maxPlaceAttemptsPerObject = 60;
    [SerializeField] private List<RoomSpawnEntry> _obstacles = new List<RoomSpawnEntry>();
    [SerializeField] private List<RoomSpawnEntry> _enemies = new List<RoomSpawnEntry>();

    public bool UseRandomSeed => _useRandomSeed;
    public int Seed => _seed;
    public Vector2 SpawnAreaCenter => _spawnAreaCenter;
    public Vector2 SpawnAreaSize => _spawnAreaSize;
    public float SpawnY => _spawnY;
    public float PlayerSafeRadius => _playerSafeRadius;
    public float DefaultSpawnRadius => _defaultSpawnRadius;
    public int MaxPlaceAttemptsPerObject => Mathf.Max(1, _maxPlaceAttemptsPerObject);
    public IReadOnlyList<RoomSpawnEntry> Obstacles => _obstacles;
    public IReadOnlyList<RoomSpawnEntry> Enemies => _enemies;
}

[Serializable]
public class RoomSpawnEntry
{
    [SerializeField] private GameObject _prefab;
    [SerializeField, Min(0)] private int _count = 1;
    [SerializeField, Min(0f)] private float _spawnRadius = 1.2f;
    [SerializeField] private bool _randomYaw = true;

    public GameObject Prefab => _prefab;
    public int Count => _count;
    public float SpawnRadius => _spawnRadius;
    public bool RandomYaw => _randomYaw;
}
