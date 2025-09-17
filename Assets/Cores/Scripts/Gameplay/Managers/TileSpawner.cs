using System;
using System.Collections.Generic;
using EventBus;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Pool;

public enum SpawnState
{
    None,
    Start,
    Spawning,
    Rest,
    End,
}

public class TileSpawner : MonoBehaviourEventListener
{
    [Header("Tile Prefabs")]
    [SerializeField] private TapTile tapTilePrefab;
    [SerializeField] private HoldTile holdTilePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnLanes;
    [SerializeField] private float spawnInterval = 1.0f;
    //[SerializeField] private float dropSpeed = 10f;

    private Dictionary<string, IObjectPool<Tile>> tiles = new Dictionary<string, IObjectPool<Tile>>();
    private SpawnState SpawnState;
    private float _timer;
    private Action OnStopDrop;
    bool _isPlayingMusic;
    #region Setup Spawner
    protected override void RegisterEvents()
    {

        EventBus<GameplayEvent>.AddListener<OnGameStateChange>((int)EventId_Gameplay.OnGameStateChange, OnGameStateChangeListener);
        SetupPoolTile();
        SpawnState = SpawnState.None;
        _isPlayingMusic = false;
    }

    private void OnClickMissing()
    {
        EventBus<GameplayEvent>.PostEvent<OnClickMissingData>((int)EventId_Gameplay.ClickTileFall);

        SoundManager.Instance.StopBGMusic();
    }

    private void OnClickSuccess(ClickTimingType type)
    {
        if (!_isPlayingMusic)
        {
            _isPlayingMusic = true;
            SoundManager.Instance.PlayBGMusic(SoundName.BGM_Test);
        }
        EventBus<GameplayEvent>.PostEvent((int)EventId_Gameplay.ClickTileSuccess,new OnClickSuccessData()
        {
            Type = type,
        });
    }

    protected override void UnregisterEvents()
    {
        EventBus<GameplayEvent>.RemoveListener<OnGameStateChange>((int)EventId_Gameplay.OnGameStateChange, OnGameStateChangeListener);
        //ClearPool();
        SpawnState = SpawnState.End;
    }

    private void SetupPoolTile()
    {
        tiles = new Dictionary<string, IObjectPool<Tile>>();
        tiles.Add(tapTilePrefab.gameObject.name, CreatePool(tapTilePrefab));
        tiles.Add(holdTilePrefab.gameObject.name, CreatePool(holdTilePrefab));
    }

    private void ClearPool()
    {
        if (tiles != null)
        {
            foreach (var tile in tiles.Values)
            {

                tile?.Clear();
            }
        }
    }
    #endregion
    #region Pool Tile
    private IObjectPool<Tile> CreatePool(Tile prefab)
    {
        return new ObjectPool<Tile>(() => CreatePooledItem(prefab), OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject);
    }

    private Tile CreatePooledItem(Tile prefabs)
    {
        Tile obj = Instantiate(prefabs);
        obj.name = prefabs.name;
        return obj;
    }

    private void OnReturnedToPool(Tile obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void OnTakeFromPool(Tile obj)
    {
        obj.gameObject.SetActive(true);
    }

    private void OnDestroyPoolObject(Tile obj)
    {
        Destroy(obj.gameObject);
    }
    #endregion
    #region Function
    private Tile GetTileFromPool(bool IsHold)
    {
        string key = IsHold ? holdTilePrefab.name : tapTilePrefab.name;
        if (tiles.TryGetValue(key, out var pool))
        {
            Tile tile = pool.Get();
            tile.Setup(OnClickSuccess, OnClickMissing, pool);
            OnStopDrop += tile.StopDropDown;
            return tile;
        }
        else
        {
            Tile newType = IsHold ? holdTilePrefab : tapTilePrefab;
            IObjectPool<Tile> newPool = CreatePool(newType);
            tiles.Add(key, newPool);
            Tile tile = pool.Get();
            OnStopDrop += tile.StopDropDown;
            tile.Setup(OnClickSuccess, OnClickMissing, pool);
            return tile;
        }
    }

    private void SpawnRandomTile()
    {
        int laneIndex = Random.Range(0, spawnLanes.Length);
        Transform lane = spawnLanes[laneIndex];

        bool isHold = (Random.value > 0.7f);

        Tile tile = GetTileFromPool(isHold);
        spawnInterval = tile._delayTime;
        tile.transform.position = lane.position;

        tile.StartDropDown();
    }

    private void OnGameStateChangeListener(OnGameStateChange data)
    {
        Debug.Log($"State {data.State}");
        if(data.State == GameState.Start)
        {
            SpawnState = SpawnState.Spawning;
        }
        else
        {
            SpawnState = SpawnState.None;
            OnStopDrop?.Invoke();
        }
    }
    #endregion

    #region Game Loop
    private void Update()
    {
        if (SpawnState != SpawnState.Spawning) return;
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnRandomTile();
        }
    }
    #endregion
}

public struct OnClickSuccessData : IEventData
{
    public ClickTimingType Type;
}

public struct OnClickMissingData : IEventData
{

}
