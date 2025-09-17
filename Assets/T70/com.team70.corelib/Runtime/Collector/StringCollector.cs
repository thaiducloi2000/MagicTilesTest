using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
 using com.team70;
 using UnityEditor;
#endif

[CreateAssetMenu(fileName = "string collector.asset", menuName = "T70 Collection/String", order = 111)]
public class StringCollector : CollectorT<string>
{

#if UNITY_EDITOR
    [ContextMenu("BuildStaticStringClass")]
    public void BuildStaticStringClass()
    {
        var filePath = AssetDatabase.GetAssetPath(this);
        filePath = filePath.Remove(filePath.LastIndexOf("/", StringComparison.Ordinal));
        filePath += "/" + $"{T70.ClearSpecialChar(this.name)}.cs";
        Debug.Log(filePath);
        T70.CreateConstString(T70.ClearSpecialChar(this.name), filePath, list.ToArray());
    }
#endif
    
#if UNITY_EDITOR
    [ContextMenu("BuildStaticIndexClass")]
    public void BuildStaticEnumClass()
    {
        var filePath = AssetDatabase.GetAssetPath(this);
        filePath = filePath.Remove(filePath.LastIndexOf("/", StringComparison.Ordinal));
        filePath += "/" + $"{T70.ClearSpecialChar(this.name)}.cs";
        Debug.Log(filePath);
        T70.CreateConstIndex(T70.ClearSpecialChar(this.name), filePath, list.ToArray());
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(StringCollector))]
public class StringCollectorEditor : CollectorEditorT<string>
{
}
#endif