using EventBus;

#region Marker Class for Event bus System 
public class GameplayEvent : IEventChannel { }
public class UIEvent : IEventChannel { }
#endregion

#region EventId_Gameplay
public enum EventId_Gameplay
{
    OnGameStateChange = 1,

    // Interact 
    ClickTileSuccess = 1000,
    ClickTileFall,
}
#endregion

#region EventId_UI
public enum EventId_UI
{
    OnUiClickStart = 1000,

    OnUiChangePoint = 2000,

    OnUiShowGameOver = 3000,
}
#endregion
