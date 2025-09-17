using DG.Tweening;
using Sirenix.OdinInspector;
using System;

namespace TweenAnimation
{
    public abstract class TweenDoAnimation : TweenAnimation
    {
        public Action OnCompleteCallback;
        public Ease AnimationEase = Ease.OutBack;
        public bool IsIgnoreTimeScale;

        [Button]
        public void ShowAnimation(Action Callback = null, float delay = 0)
        {
            Play();
            OnCompleteCallback = Callback;
            _Tweener = DoAnimation().SetEase(AnimationEase).SetUpdate(IsIgnoreTimeScale).SetDelay(delay).OnComplete(() =>
            {
                OnCompleteCallback?.Invoke();
                Stop();
            });
        }

        protected abstract Tweener DoAnimation();
        public abstract void Reset();
    }
}