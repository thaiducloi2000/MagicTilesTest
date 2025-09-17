using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Attach : MonoBehaviour
{
    public bool isAttached;
    public Transform target;
    public Vector3 offset; 

    public bool position;
    public bool rotation;

    void Update()
    {
        if (!isAttached) return;

        if (position)
        {
            var pos = target.TransformPoint(offset);
            transform.position = pos;
        }
        
        if (rotation) transform.rotation = target.rotation;
    }
}
