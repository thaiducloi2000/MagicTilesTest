using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeTest : MonoBehaviour
{
    public string text;
    public string locId;

    #if UNITY_EDITOR
    [NonSerialized] private string lastLocId;
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (lastLocId == locId) return;

        lastLocId = locId;
        text = LocalizeV2.Get(locId);
    }
    #endif
}
