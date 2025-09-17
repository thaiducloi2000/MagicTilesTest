using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Color collection.asset", menuName = "T70 Collection/Color", order = 110)]
public class ColorCollector : CollectorT<Color>
{
	
	[ContextMenu("Refresh Cache")]
	public override void Refresh()
	{
		base.Refresh();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(ColorCollector))]
public class ColorCollectorEditor : CollectorEditorT<Color>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif