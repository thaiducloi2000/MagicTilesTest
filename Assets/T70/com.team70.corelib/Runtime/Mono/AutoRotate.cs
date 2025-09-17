using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class AutoRotate : MonoBehaviour {

	[SerializeField] private Vector3 _speed;
	[SerializeField] private float _time = default;
	[SerializeField] private Transform _rect = default;

	private Tweener _tweener = default;

	private void OnEnable()
	{
		if (_tweener != null)
		{
			_tweener.Kill();
			_tweener = null;
		}
		_rect.rotation = Quaternion.identity;
		_tweener = _rect.DORotate(_speed, _time).SetUpdate(true).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
	}

	private void OnDisable()
	{
		if (_tweener != null)
		{
			_tweener.Kill();
			_tweener = null;
		}
	}

	private void OnDestroy()
	{
		if (_tweener != null)
		{
			_tweener.Kill();
			_tweener = null;
		}
	}
}
