using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Sprite collection.asset", menuName = "T70 Collection/Sprite", order = 110)]
public class SpriteCollector : AssetCollectorT<Sprite>
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteCollector))]
public class SpriteCollectorEditor : CollectorEditorT<Sprite>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif