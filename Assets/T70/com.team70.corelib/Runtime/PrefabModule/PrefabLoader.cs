using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.team70;


public class PrefabLoader : MonoBehaviour
{
    public string id;
    [NonSerialized]
    public PrefabModule view;

    private void Start()
    {
        view = PrefabModule.Load(id, this.transform);
    }
}
