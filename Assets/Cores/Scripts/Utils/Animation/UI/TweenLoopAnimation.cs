using DG.Tweening;
namespace TweenAnimation
{
    public abstract class TweenLoopAnimation : TweenAnimation
    {
        public Ease AnimationEase = Ease.OutBack;
        public bool IsIgnoreTimeScale;
        public bool IsAuto;

        protected virtual void OnEnable()
        {
            AutoStart();
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        private void AutoStart()
        {
            if (!IsAuto) return;
            StartLoop();
        }

        public void StartLoop()
        {
            Play();

            _Tweener = DoAnimation()
                .SetEase(AnimationEase)
                .SetUpdate(IsIgnoreTimeScale)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    StartLoop();
                });
        }

        protected override void Stop()
        {
            base.Stop();
            _Tweener = null;
        }

        protected abstract Tweener DoAnimation();
    }
}
