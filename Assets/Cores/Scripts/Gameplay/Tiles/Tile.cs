using System;
using nano.vs2;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

public enum ClickTimingType
{
    Great,
    Good,
    Perfect,
}

public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected float _timeHold;
    [SerializeField] protected int _point;
    [SerializeField] protected VisualState _vstTileState;
    public float _delayTime;
    protected IObjectPool<Tile> _pool;

    protected Action<ClickTimingType> _OnClickSuccessCallBack;
    protected Action _OnMissingClickCallback;
    [SerializeField] private float _speedDropDown;
    [SerializeField] private float _maxDistanceBottom;
    public bool IsClickSuccess { get; protected set; }
    protected bool _IsDropDown = false;
    [SerializeField] protected ClickSuccessAnimation _Animation;
    protected float _clickPoint;
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        _clickPoint = eventData.position.y;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }

    protected virtual void UpdateTileState(int state)
    {
    }

    public virtual void Setup(Action<ClickTimingType> ClickSuccessCallback, Action OnMissingCallback, IObjectPool<Tile> pool, float speedDropDown = 10f)
    {
        _OnClickSuccessCallBack = ClickSuccessCallback;
        _OnMissingClickCallback = OnMissingCallback;
        _speedDropDown = speedDropDown;
        _pool = pool;
    }

    public virtual void StartDropDown()
    {
        IsClickSuccess = false;
        _IsDropDown = true;
        _vstTileState.SetState(1, true);
        _Animation.Reset();
    }

    public virtual void StopDropDown()
    {
        _IsDropDown = false;
    }

    protected virtual void Update()
    {
        if (!_IsDropDown) return;
        transform.Translate(Vector3.down * _speedDropDown * Time.deltaTime);

        if (!IsClickSuccess && transform.position.y <= _maxDistanceBottom)
        {
            _IsDropDown = false;
            if (!IsClickSuccess)
            {
                _OnMissingClickCallback?.Invoke();
            }
            _pool?.Release(this);
            Clear();
        }
    }

    protected virtual void Clear()
    {
        _OnClickSuccessCallBack = null;
        _OnMissingClickCallback = null;
    }

    protected virtual ClickTimingType GetTypeClick()
    {
        if(_clickPoint >= 7.5f)
        {
            return ClickTimingType.Perfect;
        }
        else if(_clickPoint >= 5.5f)
        {
            return ClickTimingType.Good;
        }
        return ClickTimingType.Great;
    }
}
