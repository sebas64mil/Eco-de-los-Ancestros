using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly Queue<GameObject> pool = new();
    private readonly GameObject prefab;
    private readonly Transform container;

    public ObjectPool(GameObject prefab, int initialSize, Transform container)
    {
        this.prefab = prefab;
        this.container = container;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, container);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(prefab);
        }

        if (obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.SetPool(this);
        }

        obj.transform.SetParent(null);
        obj.SetActive(true);

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(container);
        pool.Enqueue(obj);
    }
}
