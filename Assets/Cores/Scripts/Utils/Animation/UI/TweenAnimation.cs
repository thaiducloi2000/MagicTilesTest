using DG.Tweening;
using UnityEngine;
namespace TweenAnimation
{
    public abstract class TweenAnimation : MonoBehaviour
    {
        protected Tweener _Tweener;

        protected virtual void Play()
        {
            KillTweenerIfActive();
        }

        protected virtual void Stop()
        {
            KillTweenerIfActive();
        }

        private void OnDestroy()
        {
            if (_Tweener != null || _Tweener.IsActive())
            {
                _Tweener.Kill();
            }
        }

        private void KillTweenerIfActive()
        {
            if (_Tweener != null && _Tweener.IsActive())
            {
                _Tweener.Kill();
            }
        }
    }
}
