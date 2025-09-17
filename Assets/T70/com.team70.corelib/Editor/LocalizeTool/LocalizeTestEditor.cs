
using UnityEditor;

[CustomEditor(typeof(LocalizeTest))]
public class LocalizeTestEditor : Editor
{
    private static string[] allLocIds;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var lt = (LocalizeTest)target;
        if (lt == null) return;

        if (allLocIds == null)
        {
            allLocIds = LocalizeV2.GetAllLocIds();
        }
    }
}
