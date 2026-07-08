using UnityEngine;

[CreateAssetMenu(fileName = "GameFlowConfig", menuName = "Create/Game/GameFlowConfig", order = 0)]
public class GameFlowConfig : ScriptableObject
{
    [SerializeField] private float _playerLocalSpawnZ = -14.5f;

    public float PlayerLocalSpawnZ => _playerLocalSpawnZ;
    public Vector3 PlayerLocalSpawnPosition => new Vector3(0f, 0f, _playerLocalSpawnZ);
}
