using EventBus;
using TMPro;
using TweenAnimation;
using UnityEngine;

public class UIPoint : MonoBehaviourEventListener
{
    [SerializeField] private TweenDoAnimation ChangePointAnimation;
    [SerializeField] private TweenDoAnimation ClickTimingAnimation;

    [SerializeField] private TextMeshProUGUI _pointTmp;
    [SerializeField] private TextMeshProUGUI _ClickTimingTmp;

    private int _currentScore;

    protected override void RegisterEvents()
    {

        EventBus<UIEvent>.AddListener<OnPointChangeData>((int)EventId_UI.OnUiChangePoint, OnChangePoint);
    }

    protected override void UnregisterEvents()
    {
        EventBus<UIEvent>.RemoveListener<OnPointChangeData>((int)EventId_UI.OnUiChangePoint, OnChangePoint);
    }

    private void OnChangePoint(OnPointChangeData data)
    {
        _currentScore = data.Score;

        // Update UI
        if (_pointTmp != null)
        {
            ChangePointAnimation?.ShowAnimation(() =>
            _pointTmp.text = _currentScore.ToString());
        }

        if (_ClickTimingTmp != null)
        {
            ClickTimingAnimation?.ShowAnimation(() =>
            {
                _ClickTimingTmp.text = data.Timing.ToString();
            });
        }
    }
}

public struct OnPointChangeData : IEventData
{
    public int Score;
    public ClickTimingType Timing;
    public int Combo;
}
