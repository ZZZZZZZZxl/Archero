using UnityEngine;

public enum GameFlowState
{
    Playing,
    Paused,
    UpgradeSelecting,
    GameOver
}

public class GameFlowManager : MonoBehaviour
{
    [SerializeField] private GameFlowConfig _config;
    [SerializeField] private RoomLevelSpawner _roomLevelSpawner;
    [SerializeField] private DoorController _door;
    [SerializeField] private Transform _playerRoot;
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerRuntimeContext _playerRuntimeContext;
    [SerializeField] private PlayerHealthController _playerHealth;
    [SerializeField] private PlayerExperienceController _playerExperience;
    [SerializeField] private PlayerUpgradeController _playerUpgrade;
    [SerializeField] private bool _returnToMainMenuAfterLastLevel = true;

    public static GameFlowManager MainInstance { get; private set; }

    private int _currentLevelIndex;
    private static readonly Vector3 PlayerRootStartPosition = Vector3.zero;
    private static readonly Quaternion PlayerRootStartRotation = Quaternion.identity;
    private static readonly Vector3 PlayerRootStartScale = Vector3.one;
    private static readonly Vector3 DefaultPlayerLocalSpawnPosition = new Vector3(0f, 0f, -14.5f);

    public GameFlowState State { get; private set; }
    public bool IsPlaying => State == GameFlowState.Playing;
    public int CurrentLevelIndex => _currentLevelIndex;

    private void Awake()
    {
        MainInstance = this;
        ResolveReferences();
        StartGame();
    }

    private void OnEnable()
    {
        BindPlayerHealth();
    }

    private void OnDisable()
    {
        UnbindPlayerHealth();
    }

    private void OnDestroy()
    {
        if (MainInstance == this)
            MainInstance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void StartGame()
    {
        _currentLevelIndex = 0;
        ResetPlayerForNewRun();
        LoadCurrentLevel();
        SetState(GameFlowState.Playing);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadMainMenu();
    }

    public void PauseGame()
    {
        if (State != GameFlowState.Playing)
            return;

        SetState(GameFlowState.Paused);
    }

    public void ResumeGame()
    {
        if (State != GameFlowState.Paused)
            return;

        SetState(GameFlowState.Playing);
    }

    public void BeginUpgradeSelection()
    {
        if (State != GameFlowState.Playing)
            return;

        SetState(GameFlowState.UpgradeSelecting);
    }

    public void CompleteUpgradeSelection()
    {
        if (State != GameFlowState.UpgradeSelecting)
            return;

        SetState(GameFlowState.Playing);
    }

    public void EnterNextLevel()
    {
        if (State != GameFlowState.Playing)
            return;

        int nextLevelIndex = _currentLevelIndex + 1;
        if (!_roomLevelSpawner || !_roomLevelSpawner.CanLoadLevel(nextLevelIndex))
        {
            if (_returnToMainMenuAfterLastLevel)
                ReturnToMainMenu();

            return;
        }

        _currentLevelIndex = nextLevelIndex;
        ResetPlayerForNextLevel();
        LoadCurrentLevel();
        SetState(GameFlowState.Playing);
    }

    public void GameOver()
    {
        ReturnToMainMenu();
    }

    private void TogglePause()
    {
        if (State == GameFlowState.Playing)
        {
            PauseGame();
            return;
        }

        if (State == GameFlowState.Paused)
            ResumeGame();
    }

    private void SetState(GameFlowState state)
    {
        State = state;
        Time.timeScale = state == GameFlowState.Playing ? 1f : 0f;

        UIManager ui = UIManager.MainInstance;
        if (ui)
            ui.SetState(state);
    }

    private void LoadCurrentLevel()
    {
        ResolveReferences();
        ResetPlayerTargetData();

        if (_door)
            _door.ResetClosed();

        TeleportPlayerToSpawn();

        if (_roomLevelSpawner)
            _roomLevelSpawner.LoadLevel(_currentLevelIndex);

        TeleportPlayerToSpawn();
    }

    private void ResetPlayerForNewRun()
    {
        ResetPlayerForNextLevel();

        if (_playerUpgrade)
            _playerUpgrade.ResetUpgrades();

        if (_playerHealth)
            _playerHealth.ResetHealth();

        if (_playerExperience)
            _playerExperience.ResetExperience();
    }

    private void ResetPlayerForNextLevel()
    {
        ResolveReferences();
        ResetPlayerTargetData();
        TeleportPlayerToSpawn();
    }

    private void ResolveReferences()
    {
        if (!_roomLevelSpawner)
            _roomLevelSpawner = FindObjectOfType<RoomLevelSpawner>();

        if (!_door)
            _door = FindObjectOfType<DoorController>();

        if (!_playerRoot)
        {
            GameObject playerRootObject = GameObject.Find("PlayerParent");
            if (playerRootObject)
                _playerRoot = playerRootObject.transform;
        }

        if (!_player)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController)
                _player = playerController.transform;
        }

        if (!_playerHealth && _player)
            _playerHealth = _player.GetComponent<PlayerHealthController>();

        if (!_playerHealth)
            _playerHealth = FindObjectOfType<PlayerHealthController>();

        if (!_playerRuntimeContext && _player)
            _playerRuntimeContext = _player.GetComponent<PlayerRuntimeContext>();

        if (!_playerRuntimeContext)
            _playerRuntimeContext = FindObjectOfType<PlayerRuntimeContext>();

        if (!_playerExperience && _player)
            _playerExperience = _player.GetComponent<PlayerExperienceController>();

        if (!_playerExperience)
            _playerExperience = FindObjectOfType<PlayerExperienceController>();

        if (!_playerUpgrade && _player)
            _playerUpgrade = _player.GetComponent<PlayerUpgradeController>();

        if (!_playerUpgrade)
            _playerUpgrade = FindObjectOfType<PlayerUpgradeController>();
    }

    private void BindPlayerHealth()
    {
        ResolveReferences();

        if (!_playerHealth)
            return;

        _playerHealth.DiedEvent -= OnPlayerDied;
        _playerHealth.DiedEvent += OnPlayerDied;
    }

    private void UnbindPlayerHealth()
    {
        if (!_playerHealth)
            return;

        _playerHealth.DiedEvent -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        GameOver();
    }

    private void ResetPlayerTargetData()
    {
        if (_playerRuntimeContext)
            _playerRuntimeContext.ResetRuntimeData();

        PlayerDetectionController detection = _player
            ? _player.GetComponent<PlayerDetectionController>()
            : FindObjectOfType<PlayerDetectionController>();

        if (detection)
            detection.ClearCurrentEnemy();
    }

    private void TeleportPlayerToSpawn()
    {
        if (!_playerRoot)
            return;

        CharacterController characterController = _player
            ? _player.GetComponent<CharacterController>()
            : _playerRoot.GetComponentInChildren<CharacterController>();

        bool controllerWasEnabled = characterController && characterController.enabled;
        if (controllerWasEnabled)
            characterController.enabled = false;

        _playerRoot.SetPositionAndRotation(PlayerRootStartPosition, PlayerRootStartRotation);
        _playerRoot.localScale = PlayerRootStartScale;

        if (_player && _player != _playerRoot)
        {
            _player.localPosition = GetPlayerLocalSpawnPosition();
            _player.localRotation = Quaternion.identity;
        }

        Physics.SyncTransforms();

        if (controllerWasEnabled)
            characterController.enabled = true;
    }

    private Vector3 GetPlayerLocalSpawnPosition()
    {
        return _config ? _config.PlayerLocalSpawnPosition : DefaultPlayerLocalSpawnPosition;
    }
}
