using System;
using UnityEngine;
using EventBus;
using DG.Tweening;

public class UiGameplayManager : MonoBehaviourEventListener
{
    [SerializeField] private CanvasGroup _uiGameOver;
    protected override void RegisterEvents()
    {
        EventBus<UIEvent>.AddListener<OnGameOverData>((int)EventId_UI.OnUiShowGameOver, OnShowUiGameOver);
        _uiGameOver.alpha = 0;
        _uiGameOver.gameObject.SetActive(false);
    }

    protected override void UnregisterEvents()
    {
        EventBus<UIEvent>.RemoveListener<OnGameOverData>((int)EventId_UI.OnUiShowGameOver, OnShowUiGameOver);
    }

    private void OnShowUiGameOver(OnGameOverData data)
    {
        _uiGameOver.gameObject.SetActive(true);
        DOVirtual.Float(0, 1, .3f, value => 
        {
            _uiGameOver.alpha = value;
        });
    }
}

public struct OnGameOverData : IEventData
{

}
