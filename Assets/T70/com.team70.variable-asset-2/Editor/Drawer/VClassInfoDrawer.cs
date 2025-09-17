using System.Linq;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace nano.va2
{
    public class VClassInfoDrawer
    {
        bool DrawChild<TClass>(VClassAssetT<TClass> asset, string id) where TClass : class
        {
            VAsset child = asset.container.GetChild(id);
            var changed = false;

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.DelayedTextField(id, GUILayout.Width(100f));
                
                if (child != null)
                {
                    var vo = new SerializedObject(child);

                    EditorGUI.BeginChangeCheck();
                    {
                        var vp = vo.FindProperty("_value");
                        EditorGUILayout.PropertyField(vp, GUIContent.none);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        vo.ApplyModifiedProperties();
                    }
                    
                    // EditorGUILayout.ObjectField(child, typeof(VAsset), false, GUILayout.Width(50f));
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(18f)))
                    {
                        asset.container.DetachChild(asset, id);
                        changed = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(18f)))
                    {
                        asset.container.AttachChild(asset, id, asset.classInfo.fieldMap[id].FieldType);
                        changed = true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (changed)
            {
                asset.Editor_MarkAsDirty();
                AssetDatabase.SaveAssets();
            }
            
            return changed;
        }
        
        public void Draw<TClass>(SerializedObject obj, VClassAssetT<TClass> asset) where TClass : class
        {
            if (asset.classInfo == null) asset.Init();
            if (asset.container == null) return;
            
            var ids = asset.classInfo.fieldIds;
            for (int i = 0; i < ids.Count; i++)
            {
                DrawChild(asset, ids[i]);
            }

            var lbSize = VGUI.SetLabelWPercent(asset.labelWPercent);
            {
                if (VGUI.DrawProperty(obj, "_value")) // changed
                {
                    asset.SetRawValue(asset.GetRawValue(), false, false);
                }
            }
            EditorGUIUtility.labelWidth = lbSize;
        }
    }
}