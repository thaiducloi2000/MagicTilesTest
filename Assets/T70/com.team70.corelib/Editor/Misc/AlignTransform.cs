using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AlignTransform
{
    [MenuItem("T70/Tools/Transform/Align Position", false, -90)]
    public static void AlignPosition()
    {
        Align(true, false);
    }

    [MenuItem("T70/Tools/Transform/Align Rotation", false, -90)]
    public static void AlignRotation()
    {
        Align(false, true);
    }
    
    [MenuItem("T70/Tools/Transform/Align Both", false, -90)]
    public static void AlignBoth()
    {
        Align(true, true);
    }
    
    public static void Align(bool position, bool rotation)
    {
        var s= Selection.gameObjects;
        if (s.Length != 2)
        {
            Debug.LogWarning("Must select 2 gameObjects");
            return;
        }
        
        var active = Selection.activeGameObject;
        var alignTarget = active == s[0] ? s[1] : s[0];

        Debug.Log(alignTarget + " --> " + active);
        
        Undo.RecordObject(alignTarget.transform, "Align objects");
        if (position) alignTarget.transform.position = active.transform.position;
        if (rotation) alignTarget.transform.rotation = active.transform.rotation;
    }
}
