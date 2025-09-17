using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nano.va2
{
    public class VRefViewer : MonoBehaviour
    {
        public List<ScriptableObject> listRefs = new List<ScriptableObject>();
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VRefViewer))]
    public class VRefViewerEditor : Editor
    {
        public List<ScriptableObject> FindAllVRefs()
        { 
            var result = new List<ScriptableObject>();
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", null);
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (typeof(IVRef).IsAssignableFrom(type))
                {
                    result.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
                }
            }
            
            return result;
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            var viewer = (VRefViewer)target;
            if (viewer == null) return;
            
            for (var i = 0;i < viewer.listRefs.Count;i++)
            {
                var item = viewer.listRefs[i];
                if (item == null) continue;
                var rect = GUILayoutUtility.GetRect(0, Screen.width, 18f, 18f);
                
                rect.width = rect.width / 2f - 2f;
                EditorGUI.ObjectField(rect, item, typeof(ScriptableObject), false);

                rect.x += rect.width + 4f;
                
                var vref = (IVRef)item;
                UnityObject t1 = vref.currentRef;
                UnityObject t2 = EditorGUI.ObjectField(rect, t1, typeof(UnityObject), true);
                if (t2 == t1)  continue;
                
                if (t2 is GameObject && item is IVRefComponent)
                {
                    t2 = (t2 as GameObject).GetComponent((item as IVRefComponent).targetTypeT);
                    if (t2 == null) return;
                }

                vref.currentRef = t2;
            }

            if (GUILayout.Button("Find All"))
            {   
                viewer.listRefs = FindAllVRefs();
                EditorUtility.SetDirty(viewer);
            }
        }
    }
    #endif
}

