using UnityEngine;
using UnityEngine.EventSystems;

public class TapTile : Tile
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!_IsDropDown) return;
        Debug.Log("Click Tile");
        IsClickSuccess = true;
        _Animation.ShowAnimation(FadeImage);
        _OnClickSuccessCallBack?.Invoke(GetTypeClick());
    }

    private void FadeImage()
    {
        _vstTileState.SetState(0, true);
    }
}
