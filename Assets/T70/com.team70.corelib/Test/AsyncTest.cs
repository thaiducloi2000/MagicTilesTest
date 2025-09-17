using System.Collections;
using System.Collections.Generic;
using com.team70;
using UnityEngine;

public class AsyncTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        
        for (var i = 0; i < 100; i++)
        {
            var id0 = i;
            var idx = 100 - i;
            Async.Call(() => {Debug.Log($"{Async.gameTime} : call {id0}"); }, idx * 0.01f);    
        }
    }
}
