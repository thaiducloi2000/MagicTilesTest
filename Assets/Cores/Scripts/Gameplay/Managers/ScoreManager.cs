using EventBus;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _score;
    private int _combo;

    public int PerfectScore = 5;
    public int GoodScore = 2;
    public int GreatScore = 1;
    public int ComboBonus = 1;

    private ClickTimingType _lastClickTimingType;

    private void Start()
    {
        _score = 0;
        _combo = 0;
    }

    public void OnTileClick(ClickTimingType timing)
    {
        if(_lastClickTimingType == timing)
        {
            _combo++;
        }
        else
        {
            _lastClickTimingType = timing;
            _combo = 0;
        }
        switch (timing)
        {
            case ClickTimingType.Perfect:
                _score += PerfectScore + (_combo * ComboBonus);
                Debug.Log($"Perfect! Score: {_score}, Combo: {_combo}");
                break;

            case ClickTimingType.Good:
                _score += GoodScore + (_combo * ComboBonus);
                Debug.Log($"Good! Score: {_score}, Combo: {_combo}");
                break;

            case ClickTimingType.Great:
                _score += GreatScore + (_combo * ComboBonus);
                Debug.Log($"GreatScore! Score: {_score}, Combo: {_combo}");
                break;
        }
        EventBus<UIEvent>.PostEvent((int)EventId_UI.OnUiChangePoint, new OnPointChangeData()
        {
            Score = _score,
            Timing  = timing,
            Combo = _combo,
        });
    }
}
