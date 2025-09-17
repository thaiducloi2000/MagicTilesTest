// #define USE_LEAN_POOL
#define USE_DEFAULT_POOL

#if UNITY_EDITOR
	// #define VERBOSE_LOG
	// #define SHOW_INFO
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Async = com.team70.Async;

#if USE_CURVY_POOL
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevTools;
#endif

#if USE_LEAN_POOL
using Lean.Pool;
#endif

//TODO
// + SUPPORT IPOOLABLE

public partial class EasyPool : MonoBehaviour
{
    private static EasyPool _instance;

    // SHOULD USE WEAK REFERENCE
    private static readonly Dictionary<GameObject, string> map = new Dictionary<GameObject, string>();

    // PUBLIC APIs
    public static void Init()
    {
	    if (_instance == null)
	    {
#if VERBOSE_LOG
		    Debug.LogWarning("[Editor] Can not Get from EasyPool (instance == null)");
#endif
		    return;
	    }
	    
	    _instance.internal_Init();
    }

    public static bool CheckExistPoolId(string poolId)
    {
	    if (_instance == null)
	    {
#if VERBOSE_LOG
		    Debug.LogWarning("[Editor] Can not Get from EasyPool (instance == null)");
#endif
		    return false;
	    }
	    return _instance.InternalCheckExistPoolId(poolId);
    }

    public static GameObject Get(string poolId, Transform parent = null)
    {
        if (_instance == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning("[Editor] Can not Get from EasyPool (instance == null)");
			#endif
            return null;
        }

        var go = internal_Get(poolId, parent);
        if (go == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning("[Editor] Instantiate error for pool: {poolId}");
			#endif
            return null;
        }

        if (!map.ContainsKey(go)) map.Add(go, poolId);
        return go;
    }
    
    public static GameObject Get(GameObject sample, Transform parent = null)
    {
        if (_instance == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning("[Editor] Can not Get from EasyPool (instance == null)");
			#endif
			
            return null;
        }

        _instance.AddPool(sample);

        var poolId = GetId(sample);
        var go = internal_Get(poolId, parent);
        if (go == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning($"[Editor] Instantiate error for pool: {poolId}");
			#endif
            return null;
        }

        if (!map.ContainsKey(go)) map.Add(go, poolId);
        return go;
    }

    public static string GetId(GameObject sample)
    {
        return sample.name;
    }

    /*[Obsolete] public static T GetComponentT<T>(string poolId, Transform parent = null) where T : Component
    {
        return Get<T>(poolId, parent);
    }*/
    
    public static T Get<T>(string poolId, Transform parent = null) where T : Component
    {
        var objectReturn = Get(poolId, parent);
        return objectReturn == null ? null : objectReturn.GetComponent<T>();
    }
    
    public static T Get<T>(GameObject sample, Transform parent = null) where T : Component
    {
        var objectReturn = Get(sample, parent);
        return objectReturn == null ? null : objectReturn.GetComponent<T>();
    }

    public static void Return(GameObject go, float? delaySecs = null, Action callBack = null)
    {
#if SHOW_INFO
	    //Debug.Log($"EasyPool Return: go => {go.name}");
#endif
        if (_instance == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning($"[Editor] Can not Return to EasyPool (instance == null)");
			#endif
            return;
        }

        if (go == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning($"[Editor] GameObject returned to pool should not be null!");
			#endif
            return;
        }

        if (!map.TryGetValue(go, out var poolId))
        {
			#if VERBOSE_LOG
			Debug.LogWarning($"[Editor] GameObject is not originated from EasyPool!");
			#endif
            return;
        }

		if (delaySecs != null)
		{
			Async.Call
			(() =>
				{
					internal_Return(poolId, go);
					callBack?.Invoke();
				},
				delaySecs.Value,
				go.GetInstanceID().ToString()
			);
		}
		else
		{
			internal_Return(poolId, go);
		}
    }
	
    // PUBLIC INSTANCE
    public bool autoInit = true;
    public List<GameObject> prefabs = new List<GameObject>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoInit) internal_Init();
    }
}

#if USE_LEAN_POOL
public partial class EasyPool : MonoBehaviour
{
    public Dictionary<string, LeanGameObjectPool> mapPool = new Dictionary<string, LeanGameObjectPool>();
    
    private void internal_Init()
    {
        for (var i = 0; i < prefabs.Count; i++)
        {
            var p = prefabs[i];
            var leanPool = gameObject.AddComponent<LeanGameObjectPool>();
            leanPool.Prefab = p;
            mapPool.Add(p.name, leanPool);
        }
    }
    private static GameObject internal_Get(string poolId, Transform parent = null)
    {
        LeanGameObjectPool pool;
        if (_instance.mapPool.TryGetValue(poolId, out pool))
        {
            return pool.Spawn(Vector3.zero, Quaternion.identity);
        }
        
    #if VERBOSE_LOG
        Debug.LogWarning($"[Editor] Pool not found: {poolId}" );
    #endif
        return null;
    }

    private static void internal_Return(string poolId, GameObject go)
    {
        LeanGameObjectPool pool;
        if (_instance.mapPool.TryGetValue(poolId, out pool))
        {
            pool.Despawn(go);
        }
    }
}
#endif

#if USE_DEFAULT_POOL
public partial class EasyPool
{
	private bool _inited;
	
    // COMMON INTERFACE
    private void internal_Init()
    {
	    if(_inited) return;
	    _inited = true;
	    
        InitPool(prefabs);
    }

    private static GameObject internal_Get(string poolId, Transform parent = null)
    {
        GameObject pool = _instance.GetFromPool(poolId, parent);
        if (pool == null)
        {
#if VERBOSE_LOG
        Debug.LogWarning($"[Editor] Pool not found: {poolId}" );
#endif
        }

        return pool;
    }

    private static void internal_Return(string poolId, GameObject go)
    {
        _instance.ReturnToPool(poolId, go);
    }

    private static void internal_AddPool(GameObject prefab)
    {
        _instance.AddPool(prefab);
    }
    
    // DEFAULT IMPLEMENTATION
    public interface IPoolable
    {
        void OnBeforeGet();
        void OnAfterReturn();
    }

    [Serializable] public class Pool
    {
        public string poolId;
        public GameObject prefab;
        public Transform defaultParent;
        public int prewarm;
        public bool autoActive;

        [NonSerialized] public Queue<GameObject> lstPrefab = new Queue<GameObject>();
    }

    Dictionary<string, Pool> poolDictionary = new Dictionary<string, Pool>();
	public List<Pool> prebuiltPools = new List<Pool>();
#if SHOW_INFO
	public List<GameObject> lstItemInPool = new List<GameObject>();
#endif
	
    private void InitPool(IEnumerable<GameObject> initPrefabs)
    {
        CleanPool();

		// customized
		foreach (var p in prebuiltPools)
		{
			poolDictionary.Add(p.poolId, p);
#if SHOW_INFO
			var arrParse = p.lstPrefab.ToArray();
			foreach (var item in arrParse)
			{
				//Debug.Log($"Item Add: {item.name}");
				lstItemInPool.Add(item);
			}
#endif
		}
		
		StartCoroutine(PrewarmRoutine());
		
        foreach (var prefab in initPrefabs)
        {
            AddPool(prefab);
        }
    }

    private IEnumerator PrewarmRoutine()
    {
	    foreach (var p in prebuiltPools)
	    {
		    for (var i = 0; i < p.prewarm; i++)
		    {
			    AddObject(p.poolId, 1, p.defaultParent);
			    yield return null;
		    }
	    }
    }
    
    private void AddPool(GameObject sample)
    {
        var poolId = GetId(sample);
        if (poolDictionary.ContainsKey(poolId))
		{
#if VERBOSE_LOG
        //Debug.LogWarning($"[Editor] Pool existed: {poolId}" );
#endif
			return;
		}

		var pool = new Pool
		{
			poolId = poolId,
			prefab = sample
		};

		poolDictionary.Add(pool.poolId, pool);
#if SHOW_INFO
	    var arrParse = pool.lstPrefab.ToArray();
	    foreach (var item in arrParse)
	    {
		    //Debug.Log($"Item Add: {item.name}");
		    lstItemInPool.Add(item);
	    }
#endif
    }

    private bool InternalCheckExistPoolId(string poolId)
    {
	    if (CheckExistPool(poolId) == false) return false;
	    Pool pool = GetPool(poolId);
	    var lstPrefab = GetListPrefab(pool);
	    if (lstPrefab == null) return false;
	    GameObject result = null;
	    while (lstPrefab.Count > 0)
	    {
		    result = lstPrefab.Peek();
		    if (result == null)
		    {
			    result = lstPrefab.Dequeue();
#if SHOW_INFO
			    //Debug.Log($"Item remove: {result.name}");
			    lstItemInPool.Remove(result);
#endif
			    if (result != null) break;
		    }
		    if (result != null) break;
	    }
	    if (result == null)
	    {
		    return false;
	    }
	    return true;
    }
    
    private GameObject GetFromPool(string poolId, Transform parent = null)
    {
        Pool pool = GetPool(poolId);
        if (pool == null) return null;

        if (parent == null) parent = pool.defaultParent;
        
        var lstPrefab = GetListPrefab(pool);
        if (lstPrefab == null) return null;

		GameObject result = null;
		while (lstPrefab.Count > 0)
		{
			result = lstPrefab.Dequeue();
#if SHOW_INFO
			//Debug.Log($"Item remove: {result.name}");
			lstItemInPool.Remove(result);
#endif
			if (result != null) break;
			// invalid item in pool (null ? destroyed?) --> should we log warning here?
		}

		if (result == null) 
		{
			AddObject(poolId, 1, parent);
			result = lstPrefab.Dequeue();
#if SHOW_INFO
			//Debug.Log($"Item remove: {result.name}");
			lstItemInPool.Remove(result);
#endif
		}
		
        if (pool.autoActive) result.SetActive(true);
		result.transform.SetParent(parent, false);
        return result;
    }
	
    private void ReturnToPool(string poolId, GameObject objectToReturn)
    {
		if (objectToReturn == null)
		{
			#if VERBOSE_LOG
			Debug.LogWarning("[Editor] EasyPool.ReturnToPool() error - returned object should not be null!");
			#endif
			return;
		}
		
        Pool pool = GetPool(poolId);
        if (pool == null) return;

        var lstPrefab = GetListPrefab(pool);
        if (lstPrefab == null) return;

#if VERBOSE_LOG
			Debug.LogWarning("[Editor] EasyPool.ReturnToPool() ");
#endif
        if (pool.autoActive) objectToReturn.SetActive(false);
        lstPrefab.Enqueue(objectToReturn);
#if SHOW_INFO
	    //Debug.Log($"Item Add: {objectToReturn.name}");
	    lstItemInPool.Add(objectToReturn);
#endif
    }

    void AddObject(string poolId, int count, Transform parent = null)
    {
        Pool pool = GetPool(poolId);
        if (pool == null) return;

        if (pool.prefab == null)
        {
			#if VERBOSE_LOG
			Debug.LogWarning($"[Editor] prefab with tag: {poolId} not exist in pool: {poolId}");
			#endif
            return;
        }

        for (var i = 0; i < count; i++)
        {
	        var go = Instantiate(pool.prefab, parent);
	        ReturnToPool(poolId, go);
        }
    }

    private void CleanPool()
    {
        if (poolDictionary == null)
        {
            poolDictionary = new Dictionary<string, Pool>();
            return;
        }

        foreach (var pool in poolDictionary)
        {
            var lst = pool.Value.lstPrefab;
            foreach (var go in lst)
            {
#if USE_CURVY_POOL
                go.Destroy();
#else
                Destroy(go);
#endif
            }
        }
#if SHOW_INFO
	    lstItemInPool.Clear();
#endif
    }

    bool CheckExistPool(string poolId)
    {
	    if (poolDictionary.TryGetValue(poolId, out _) == false)
	    {
		    return false;
	    }

	    return true;
    }
    
    Pool GetPool(string poolId)
    {
	    return poolDictionary.TryGetValue(poolId, out Pool pool) == false ? null : pool;
    }

    Queue<GameObject> GetListPrefab(Pool pool)
    {
        if (pool == null) return null;
        return pool.lstPrefab ?? new Queue<GameObject>();
    }
}
#endif