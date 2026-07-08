using UnityEngine;

public class PoolItem : MonoBehaviour
{
    private GameObject _prefab;

    public GameObject Prefab => _prefab;

    public void SetPrefab(GameObject prefab)
    {
        _prefab = prefab;
    }
}
