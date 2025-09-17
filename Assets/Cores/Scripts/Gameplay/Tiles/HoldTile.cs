using UnityEngine;
using UnityEngine.EventSystems;

public class HoldTile : Tile
{
    [SerializeField] private int _perfectPoint;
    private bool _isHolding = false;
    private float _holdTime = 0f;

    private float _deltaHold = 0f;
    private bool _IsCompleteHold = false;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (IsClickSuccess || !_IsDropDown) return;

        _isHolding = true;
        _holdTime = 0f;
        IsClickSuccess = true;

        Debug.Log("Hold started");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if(!_IsDropDown) return;
        _isHolding = false;

        Debug.Log("Hold complete!");
        JudgeResult();
    }

    protected override void Update()
    {
        base.Update();
        if (_IsCompleteHold) return;
        if (_isHolding && _holdTime < _timeHold)
        {
            _holdTime += Time.deltaTime;
            _holdTime = Mathf.Clamp(_holdTime, 0, _timeHold);
            if (_holdTime >= _timeHold)
            {
                Debug.Log("Hold auto-complete!");
                JudgeResult();
            }
        }

        _deltaHold = Mathf.Abs(_holdTime - _timeHold);
    }

    private void JudgeResult()
    {
        _IsCompleteHold = true;

        _OnClickSuccessCallBack?.Invoke(GetTypeClick());

        _Animation.ShowAnimation(FadeImage);
        //_vstTileState.SetState(0, true);
    }

    private void FadeImage()
    {
        _vstTileState.SetState(0, true);
    }

    public override void StartDropDown()
    {
        base.StartDropDown();
        ResetTile();
    }
    private void ResetTile()
    {
        IsClickSuccess = false;
        _isHolding = false;
        _IsCompleteHold = false;
        _holdTime = 0f;
    }

    protected override ClickTimingType GetTypeClick()
    {
        if (_deltaHold <= 0.1f)
        {
            return ClickTimingType.Perfect;
        }
        else if (_deltaHold <= 0.7f)
        {
            return ClickTimingType.Good;
        }
        return ClickTimingType.Great;
    }
}
