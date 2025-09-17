using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationOverLifeTime : MonoBehaviour
{
    public Vector3 rotation;

    public void Init()
    {
        rotation = new Vector3(0, 0, -100f);
    }

    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
