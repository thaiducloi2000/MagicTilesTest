using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
	public Transform lookTarget;
	[Range(0.1f, 2f)] public float speed = 0.1f;
	
	public float h;
	public float radius;
	public float angle;
	
	void Awake()
	{
		ReadRadius();	
	}
	
	void ReadRadius()
	{
		var v = transform.localPosition - lookTarget.localPosition;
		
		h = v.y;
		v.y = 0f;
		radius = v.magnitude;
	}
	
	void Update()
	{
		angle += speed * Time.deltaTime;
		transform.localPosition = new Vector3
		(
			radius * Mathf.Sin(angle),
			h,
			radius * Mathf.Cos(angle)
		);
		
		transform.LookAt(lookTarget);
	}
}
