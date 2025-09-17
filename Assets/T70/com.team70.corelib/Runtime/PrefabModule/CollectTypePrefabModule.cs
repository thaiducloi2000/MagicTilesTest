using com.team70;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CollectTypePrefabModule : MonoBehaviour
{
    public PrefabModule thisPrefabModule;

    //[Button]
    private void GetPrefabModule()
    {
        thisPrefabModule = this.GetComponent<PrefabModule>();
    }

    //[Button]
    public void DebugList()
    {
        GetPrefabModule();
        var name = "";
        foreach (var element in thisPrefabModule.lstElement)
        {
            if (element.component.Count == 0 || element.component[0] == null) continue;
            element.componentType = element.component[0].GetType().ToString();
            name += element.id + " - ";
            /*foreach (var item in element.component)
            {
                Debug.Log(item.GetType());
            }*/
        }
        Debug.Log(name);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        DestroyImmediate(this);
    }
    
    //[Button]
    /*public static void UpdatePrefab()
    {
#if UNITY_EDITOR
 
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
#endif
    }*/
}