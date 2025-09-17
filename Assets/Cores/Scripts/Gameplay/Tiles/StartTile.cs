using EventBus;
using UnityEngine.EventSystems;

public class StartTile : Tile
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        _Animation.ShowAnimation(FadeImage);
        EventBus<UIEvent>.PostEvent<OnUiClickStartData>((int)EventId_UI.OnUiClickStart);
    }


    private void FadeImage()
    {
        _vstTileState.SetState(0, true);
    }
}
