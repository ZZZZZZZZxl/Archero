using UnityEngine;

[DisallowMultipleComponent]
public class PlayerRuntimeContext : MonoBehaviour
{
    private readonly PlayerReusableData _reusableData = new PlayerReusableData();

    [SerializeField] private PlayerSO _config;

    public static PlayerRuntimeContext MainInstance { get; private set; }

    public PlayerSO Config => _config;
    public PlayerReusableData ReusableData => _reusableData;
    public bool HasInput => _reusableData.HasInput;
    public Transform CurrentEnemy => _reusableData.Enemy;

    private void Awake()
    {
        if (MainInstance && MainInstance != this)
        {
            Debug.LogWarning("Scene has more than one PlayerRuntimeContext. The first one remains the main instance.", this);
            return;
        }

        MainInstance = this;
    }

    private void OnDestroy()
    {
        if (MainInstance == this)
            MainInstance = null;
    }

    public void Configure(PlayerSO config)
    {
        if (config)
            _config = config;
    }

    public void SetHasInput(bool hasInput)
    {
        _reusableData.HasInput = hasInput;
    }

    public void SetCurrentEnemy(Transform enemy)
    {
        _reusableData.Enemy = enemy;
    }

    public void ClearCurrentEnemy()
    {
        _reusableData.Enemy = null;
    }

    public void ResetRuntimeData()
    {
        _reusableData.Enemy = null;
        _reusableData.HasInput = false;
    }
}
