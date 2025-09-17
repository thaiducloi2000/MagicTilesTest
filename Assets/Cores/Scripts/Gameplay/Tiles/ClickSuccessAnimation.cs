using DG.Tweening;
using TweenAnimation;
using UnityEngine;

public class ClickSuccessAnimation : TweenDoAnimation
{
    [SerializeField] private Transform target;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private float targetScale = 0.8f;

    private const float _originalScale = 1f;
    protected override Tweener DoAnimation()
    {
        return target
            .DOScale(Vector3.one * targetScale, duration);
    }

    public override void Reset()
    {
        if (target != null)
            target.localScale = Vector3.one * _originalScale; ;
    }
}
