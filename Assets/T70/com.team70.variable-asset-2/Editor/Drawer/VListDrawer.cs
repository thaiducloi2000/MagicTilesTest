using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace nano.va2
{
    public class VListDrawer
    {
        public int previewIndex;
        
        public void Draw<T>(List<T> list, Action<int, int, T> drawer)
        {
            DrawIList(list, (idx1, idx2, data)=> { drawer(idx1, idx2, (T)data); });
        }
        
        public void DrawIList(IList list, Action<int, int, object> drawer)
        {
            if (list == null || list.Count == 0)
            {
                EditorGUILayout.HelpBox("List is Empty", MessageType.Warning);
                return;
            }
            
            var oIndex = previewIndex;
            previewIndex = EditorGUILayout.IntSlider(oIndex, 0, list.Count-1);

            // Debug.Log(typeof(T) + " --> " + list[previewIndex]);
            drawer(oIndex, previewIndex, list[previewIndex]);
        }
    }

    public class VListDrawerJson : VListDrawer
    {
        public string previewJson;

        public void Draw<T>(List<T> list)
        {
            DrawIList(list);
        }
        
        public void DrawIList(IList list)
        {
            DrawIList(list, (pIndex, cIndex, data)=> 
            {
                if (string.IsNullOrEmpty(previewJson) || (pIndex != cIndex))
                {
                    previewJson = JsonUtility.ToJson(data, true);
                }

                EditorGUILayout.TextArea(previewJson);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Count: " + list.Count);
                    if (GUILayout.Button("Copy to Clipboard"))
                    {
                        EditorGUIUtility.systemCopyBuffer = previewJson;
                    }    
                }
                GUILayout.EndHorizontal();
            });
        }
    }
}