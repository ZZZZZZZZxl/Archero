using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-100)]
public class RoomLevelSpawner : MonoBehaviour
{
    [SerializeField] private RoomLevelCatalog _levelCatalog;
    [SerializeField, Min(0)] private int _levelIndex;
    [SerializeField] private RoomLevelConfig _levelConfig;
    [SerializeField] private Transform _playerSpawn;
    [SerializeField] private Transform _obstacleRoot;
    [SerializeField] private Transform _enemyRoot;
    [SerializeField] private EnemyChaseConfig _defaultEnemyChaseConfig;
    [SerializeField] private DamageNumberItem _defaultDamageNumberPrefab;
    [SerializeField] private GameObject _defaultSelectIndicatorPrefab;
    [SerializeField] private bool _generateOnAwake;
    [SerializeField] private bool _clearGeneratedObjectsBeforeSpawn = true;

    private readonly List<PlacedObject> _placedObjects = new List<PlacedObject>();
    private System.Random _random;
    private RoomLevelConfig _currentLevelConfig;

    public RoomLevelCatalog LevelCatalog => _levelCatalog;
    public int LevelIndex => _levelIndex;
    public RoomLevelConfig LevelConfig => ResolveLevelConfig();
    public RoomLevelConfig CurrentLevelConfig => _currentLevelConfig ? _currentLevelConfig : ResolveLevelConfig();

    private void Awake()
    {
        if (_generateOnAwake && Application.isPlaying)
            Generate();
    }

    [ContextMenu("Generate Room Now")]
    public void Generate()
    {
        _currentLevelConfig = ResolveLevelConfig();
        if (!_currentLevelConfig)
        {
            Debug.LogWarning("RoomLevelSpawner has no level config.", this);
            return;
        }

        ResolveRoots();

        if (_clearGeneratedObjectsBeforeSpawn)
            ClearGeneratedObjects();

        RoomController room = GetComponent<RoomController>();
        if (room)
            room.ResetRoomState();

        _placedObjects.Clear();
        _random = CreateRandom();

        RegisterPlayerSafeArea();
        SpawnEntries(_currentLevelConfig.Obstacles, _obstacleRoot, false);
        SpawnEntries(_currentLevelConfig.Enemies, _enemyRoot, true);

        if (room)
            room.RebuildEnemyBindings();
    }

    public bool CanLoadLevel(int levelIndex)
    {
        if (_levelCatalog)
            return _levelCatalog.TryGetLevel(levelIndex, out _);

        return levelIndex == 0 && _levelConfig;
    }

    public bool LoadLevel(int levelIndex)
    {
        _levelIndex = Mathf.Max(0, levelIndex);
        Generate();
        return _currentLevelConfig != null;
    }

    [ContextMenu("Clear Generated Objects")]
    public void ClearGeneratedObjects()
    {
        ResolveRoots();
        ClearGeneratedObjects(_obstacleRoot);
        ClearGeneratedObjects(_enemyRoot);
    }

    private void ResolveRoots()
    {
        if (!_playerSpawn)
        {
            GameObject player = GameObject.Find("PlayerParent");
            if (player)
                _playerSpawn = player.transform;
        }

        if (!_obstacleRoot)
            _obstacleRoot = FindOrCreateChild("GeneratedObstacles");

        if (!_enemyRoot)
            _enemyRoot = FindOrCreateChild("GeneratedEnemies");
    }

    private Transform FindOrCreateChild(string childName)
    {
        Transform child = transform.Find(childName);
        if (child)
            return child;

        GameObject childObject = new GameObject(childName);
        childObject.transform.SetParent(transform);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject.transform;
    }

    private System.Random CreateRandom()
    {
        if (_currentLevelConfig.UseRandomSeed)
            return new System.Random();

        return new System.Random(_currentLevelConfig.Seed);
    }

    private void RegisterPlayerSafeArea()
    {
        if (!_playerSpawn || _currentLevelConfig.PlayerSafeRadius <= 0f)
            return;

        _placedObjects.Add(new PlacedObject(GetPlayerSafeAreaCenter(), _currentLevelConfig.PlayerSafeRadius));
    }

    private Vector3 GetPlayerSafeAreaCenter()
    {
        PlayerController player = _playerSpawn.GetComponentInChildren<PlayerController>();
        if (!player)
            player = FindObjectOfType<PlayerController>();

        return player ? player.transform.position : _playerSpawn.position;
    }

    private void SpawnEntries(IReadOnlyList<RoomSpawnEntry> entries, Transform parent, bool isEnemy)
    {
        if (entries == null || parent == null)
            return;

        foreach (RoomSpawnEntry entry in entries)
            SpawnEntry(entry, parent, isEnemy);
    }

    private void SpawnEntry(RoomSpawnEntry entry, Transform parent, bool isEnemy)
    {
        GameObject prefab = entry?.Prefab;
        if (entry == null || entry.Count <= 0)
            return;

        if (!prefab)
        {
            Debug.LogError($"RoomLevelSpawner found an invalid spawn prefab in {_currentLevelConfig.name}. Please assign a GameObject prefab.", this);
            return;
        }

        float spawnRadius = entry.SpawnRadius > 0f ? entry.SpawnRadius : _currentLevelConfig.DefaultSpawnRadius;

        for (int i = 0; i < entry.Count; i++)
        {
            Vector3 position = FindSpawnPosition(spawnRadius);
            Quaternion rotation = entry.RandomYaw
                ? Quaternion.Euler(0f, RandomRange(0f, 360f), 0f)
                : prefab.transform.rotation;

            UnityEngine.Object spawnedObject = Instantiate((UnityEngine.Object)prefab, position, rotation, parent);
            GameObject spawned = spawnedObject as GameObject;
            if (!spawned)
            {
                Debug.LogError($"RoomLevelSpawner can only spawn GameObject prefabs. Invalid prefab: {prefab.name}", this);
                continue;
            }

            spawned.name = prefab.name;

            if (!spawned.GetComponent<GeneratedRoomObject>())
                spawned.AddComponent<GeneratedRoomObject>();

            if (isEnemy)
                PrepareEnemy(spawned);
            else
                PrepareObstacle(spawned);

            _placedObjects.Add(new PlacedObject(position, spawnRadius));
        }
    }

    private void PrepareObstacle(GameObject obstacle)
    {
        if (!obstacle)
            return;

        if (!obstacle.GetComponentInChildren<Collider>())
            Debug.LogError($"{obstacle.name} is missing Collider. Configure it on the obstacle prefab.", obstacle);

        if (!IsTrapObstacle(obstacle) && !obstacle.GetComponentInChildren<NavMeshObstacle>())
            Debug.LogError($"{obstacle.name} is missing NavMeshObstacle. Configure it on the obstacle prefab.", obstacle);
    }

    private static bool IsTrapObstacle(GameObject obstacle)
    {
        if (!obstacle)
            return false;

        int trapLayer = LayerMask.NameToLayer("Trap");
        return obstacle.GetComponentInChildren<TrapDamageController>() || (trapLayer >= 0 && obstacle.layer == trapLayer);
    }

    private void PrepareEnemy(GameObject enemy)
    {
        if (!enemy)
            return;

        EnemyChaseController chase = enemy.GetComponent<EnemyChaseController>();
        if (!chase)
            chase = enemy.AddComponent<EnemyChaseController>();

        if (!chase.HasConfig)
            chase.Configure(_defaultEnemyChaseConfig);

        EnsureEnemyPhysics(enemy, chase.Config);
        EnsureEnemySelection(enemy);

        if (!enemy.GetComponent<EnemyHealthController>())
            enemy.AddComponent<EnemyHealthController>();

        if (!enemy.GetComponent<EnemyCombatController>())
            enemy.AddComponent<EnemyCombatController>();

        DamageNumberFeedback damageNumber = enemy.GetComponent<DamageNumberFeedback>();
        if (!damageNumber)
            damageNumber = enemy.AddComponent<DamageNumberFeedback>();
        damageNumber.Configure(_defaultDamageNumberPrefab);
    }

    private void EnsureEnemyPhysics(GameObject enemy, EnemyChaseConfig chaseConfig)
    {
        Collider rootCollider = enemy.GetComponent<Collider>();
        if (!rootCollider)
        {
            CapsuleCollider collider = enemy.AddComponent<CapsuleCollider>();
            collider.radius = 0.65f;
            collider.height = 2f;
            collider.center = Vector3.up;
        }

        Rigidbody rigidbody = enemy.GetComponent<Rigidbody>();
        if (!rigidbody)
            rigidbody = enemy.AddComponent<Rigidbody>();

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (!agent)
            agent = enemy.AddComponent<NavMeshAgent>();

        agent.radius = Mathf.Max(0.45f, agent.radius);
        agent.speed = chaseConfig ? chaseConfig.MoveSpeed : agent.speed;
        agent.stoppingDistance = chaseConfig ? chaseConfig.StopDistance : agent.stoppingDistance;
        agent.angularSpeed = 720f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    private void EnsureEnemySelection(GameObject enemy)
    {
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (!enemyController)
            enemyController = enemy.AddComponent<EnemyController>();

        if (enemyController.HasSelectNode || !_defaultSelectIndicatorPrefab)
            return;

        GameObject indicator = Instantiate(_defaultSelectIndicatorPrefab, enemy.transform);
        indicator.name = _defaultSelectIndicatorPrefab.name;
        indicator.transform.localPosition = new Vector3(0f, 2.7f, 0f);
        indicator.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        indicator.transform.localScale = Vector3.one * 0.15f;

        enemyController.SetSelectNode(indicator.transform);
    }

    private Vector3 FindSpawnPosition(float radius)
    {
        for (int i = 0; i < _currentLevelConfig.MaxPlaceAttemptsPerObject; i++)
        {
            Vector3 candidate = GetRandomPointInSpawnArea();

            if (TryProjectToNavMesh(candidate, out Vector3 navPosition))
                candidate = navPosition;

            if (CanPlace(candidate, radius))
                return candidate;
        }

        return GetRandomPointInSpawnArea();
    }

    private Vector3 GetRandomPointInSpawnArea()
    {
        Vector2 center = _currentLevelConfig.SpawnAreaCenter;
        Vector2 halfSize = _currentLevelConfig.SpawnAreaSize * 0.5f;

        float x = RandomRange(center.x - halfSize.x, center.x + halfSize.x);
        float z = RandomRange(center.y - halfSize.y, center.y + halfSize.y);

        return transform.TransformPoint(new Vector3(x, _currentLevelConfig.SpawnY, z));
    }

    private bool TryProjectToNavMesh(Vector3 position, out Vector3 navPosition)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            navPosition = hit.position;
            return true;
        }

        navPosition = position;
        return false;
    }

    private bool CanPlace(Vector3 candidate, float radius)
    {
        foreach (PlacedObject placedObject in _placedObjects)
        {
            Vector3 delta = candidate - placedObject.Position;
            delta.y = 0f;

            float minDistance = radius + placedObject.Radius;
            if (delta.sqrMagnitude < minDistance * minDistance)
                return false;
        }

        return true;
    }

    private float RandomRange(float min, float max)
    {
        return Mathf.Lerp(min, max, (float)_random.NextDouble());
    }

    private RoomLevelConfig ResolveLevelConfig()
    {
        if (_levelCatalog && _levelCatalog.TryGetLevel(_levelIndex, out RoomLevelConfig catalogLevel))
            return catalogLevel;

        if (_levelCatalog)
            Debug.LogWarning($"RoomLevelSpawner could not resolve level index {_levelIndex} from {_levelCatalog.name}. Using direct level config fallback.", this);

        return _levelConfig;
    }

    private void ClearGeneratedObjects(Transform root)
    {
        if (!root)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            child.gameObject.SetActive(false);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private struct PlacedObject
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public PlacedObject(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}
