using DG.Tweening;
using TweenAnimation;
using UnityEngine;

public class UITextScaleUpAnimation : TweenDoAnimation
{
    [SerializeField] private Transform target;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private float scaleMultiplier = 1f;

    public override void Reset()
    {
        if (target != null)
        {
            target.localScale = Vector3.one;
        }
    }

    protected override Tweener DoAnimation()
    {

        target.localScale = Vector3.one * 0.8f;
        return target
            .DOScale(Vector3.one * scaleMultiplier, duration);
    }
}
