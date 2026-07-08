using System.Collections.Generic;
using GGG.Tool.Singleton;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, Transform> _roots = new Dictionary<GameObject, Transform>();
    

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        if (!prefab)
            return null;

        GameObject item = Get(prefab.gameObject, position, rotation, parent);
        return item.GetComponent<T>();
    }
    public T Get<T>(T prefab, Vector3 position, Transform parent = null) where T : Component
    {
        if (!prefab)
            return null;

        GameObject item = Get(prefab.gameObject, position, parent);
        return item.GetComponent<T>();
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!prefab)
            return null;

        GameObject item = GetFromPool(prefab);
        Transform itemTransform = item.transform;
        itemTransform.SetParent(parent, false);
        itemTransform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);

        return item;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Transform parent = null)
    {
        if (!prefab)
            return null;

        GameObject item = GetFromPool(prefab);
        Transform itemTransform = item.transform;
        itemTransform.SetParent(parent, false);
        itemTransform.position = position;
        item.SetActive(true);

        return item;
    }

    public void Release(Component item)
    {
        if (!item)
            return;

        Release(item.gameObject);
    }

    public void Release(GameObject item)
    {
        if (!item)
            return;

        PoolItem poolItem = item.GetComponent<PoolItem>();
        if (!poolItem || !poolItem.Prefab)
        {
            Destroy(item);
            return;
        }

        GameObject prefab = poolItem.Prefab;
        EnsurePool(prefab);

        item.SetActive(false);
        item.transform.SetParent(GetRoot(prefab), false);
        _pools[prefab].Enqueue(item);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (!prefab || count <= 0)
            return;

        EnsurePool(prefab);

        for (int i = 0; i < count; i++)
        {
            GameObject item = CreateItem(prefab);
            item.SetActive(false);
            item.transform.SetParent(GetRoot(prefab), false);
            _pools[prefab].Enqueue(item);
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        EnsurePool(prefab);

        Queue<GameObject> pool = _pools[prefab];
        while (pool.Count > 0)
        {
            GameObject item = pool.Dequeue();
            if (item)
                return item;
        }

        return CreateItem(prefab);
    }

    private GameObject CreateItem(GameObject prefab)
    {
        GameObject item = Instantiate(prefab);
        PoolItem poolItem = item.GetComponent<PoolItem>();
        if (!poolItem)
            poolItem = item.AddComponent<PoolItem>();

        poolItem.SetPrefab(prefab);
        return item;
    }

    private void EnsurePool(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
            _pools.Add(prefab, new Queue<GameObject>());

        GetRoot(prefab);
    }

    private Transform GetRoot(GameObject prefab)
    {
        if (_roots.TryGetValue(prefab, out Transform root) && root)
            return root;

        GameObject rootObject = new GameObject(prefab.name + "_Pool");
        rootObject.transform.SetParent(transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;

        _roots[prefab] = rootObject.transform;
        return rootObject.transform;
    }
}
