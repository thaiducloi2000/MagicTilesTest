using EventBus;
using UnityEngine;

public class GameManager : MonoBehaviourEventListener
{
    [SerializeField] private ScoreManager _scoreManager;
    private GameState _currentState;
    private void Awake()
    {
        _currentState = GameState.Wait;
    }
    protected override void RegisterEvents()
    {
        EventBus<GameplayEvent>.AddListener<OnClickMissingData>((int)EventId_Gameplay.ClickTileFall, OnPlayerClickFall);
        EventBus<GameplayEvent>.AddListener<OnClickSuccessData>((int)EventId_Gameplay.ClickTileSuccess, OnPlayerClickSuccess);
        EventBus<UIEvent>.AddListener<OnUiClickStartData>((int)EventId_UI.OnUiClickStart, OnPlayerClickStart);
    }

    protected override void UnregisterEvents()
    {
        EventBus<GameplayEvent>.RemoveListener<OnClickMissingData>((int)EventId_Gameplay.ClickTileFall, OnPlayerClickFall);
        EventBus<GameplayEvent>.RemoveListener<OnClickSuccessData>((int)EventId_Gameplay.ClickTileSuccess, OnPlayerClickSuccess);
        EventBus<UIEvent>.RemoveListener<OnUiClickStartData>((int)EventId_UI.OnUiClickStart, OnPlayerClickStart);
    }

    private void OnPlayerClickFall(OnClickMissingData data)
    {
        _currentState = GameState.End;
        EventBus<GameplayEvent>.PostEvent((int)EventId_Gameplay.OnGameStateChange, new OnGameStateChange() { State = _currentState });
        EventBus<UIEvent>.PostEvent<OnGameOverData>((int)EventId_UI.OnUiShowGameOver);
    }

    private void OnPlayerClickSuccess(OnClickSuccessData data)
    {
        _scoreManager.OnTileClick(data.Type);
    }

    private void OnPlayerClickStart(OnUiClickStartData data)
    {
        _currentState = GameState.Start;
        EventBus<GameplayEvent>.PostEvent((int)EventId_Gameplay.OnGameStateChange, new OnGameStateChange() { State = _currentState });
    }
}


public struct OnGameStateChange : IEventData
{
    public GameState State;
}

public struct OnUiClickStartData : IEventData
{

}

public enum GameState
{
    Wait,
    Start,
    End
}