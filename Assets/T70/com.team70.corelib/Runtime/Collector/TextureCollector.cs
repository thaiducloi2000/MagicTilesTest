using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "Texture collection.asset", menuName = "T70 Collection/Texture", order = 110)]
public class TextureCollector : CollectorT<Texture2D>
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextureCollector))]
public class TextureCollectorEditor : CollectorEditorT<Texture2D>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif