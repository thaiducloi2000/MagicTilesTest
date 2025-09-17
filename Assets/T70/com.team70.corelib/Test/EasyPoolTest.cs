using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyPoolTest : MonoBehaviour
{
	[ContextMenu("Test")] void Test()
    {
		for (int i = 0; i< 100; i++)
		{
			var go = EasyPool.Get("Cube", transform);
			go.transform.localPosition = new Vector3(0, 0, i);
			EasyPool.Return(go, i * 0.1f);
		}
    }
}
