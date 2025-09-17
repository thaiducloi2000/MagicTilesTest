using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "prefab collection.asset", menuName = "T70 Collection/Prefab", order = 110)]
public class PrefabCollector : AssetCollectorT<GameObject>
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(PrefabCollector))]
public class PrefabCollectorEditor : CollectorEditorT<GameObject>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif