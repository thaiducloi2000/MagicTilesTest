using UnityEngine;
using System.Collections;
 
public class CameraFacingBillboard : MonoBehaviour
{
	public Camera m_Camera;
	
	void OnBecameVisible()
	{
		enabled = true;
		StartCoroutine(LookAtCamera());
	}
	
	void OnBecameInvisible()
	{
		StopAllCoroutines();
		enabled = false;
	}
	
	IEnumerator LookAtCamera()
	{
		if (m_Camera == null) m_Camera = Camera.main;
		if (m_Camera == null) yield break;
		
		while (true)
		{
			transform.LookAt(
				transform.position + m_Camera.transform.rotation * Vector3.forward,
				m_Camera.transform.rotation * Vector3.up
			);
			
			yield return new WaitForSeconds(0.1f);	
		}	
	}
}