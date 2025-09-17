using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityObject = UnityEngine.Object;

namespace nano.va2
{
    public class VEventDrawer
    {
        public string vProperty;
        public Editor vEditor;
        public bool isExpand = false;

        public void Draw(SerializedObject so, VAsset asset, bool allowDelete, ref VEvent evt)
        {
            var eventAssetName = asset.GetChildEventName(vProperty);
            VGUI.FoldOut(ref isExpand, eventAssetName, 50f);
            
            //EditorGUI.BeginDisabledGroup(evt == null);
            {
                var rect = GUILayoutUtility.GetLastRect();
                rect.xMin = rect.xMax - 50f;

                if (evt == null)
                {
                    if (GUI.Button(rect, "[+]"))
                    {
                        evt = VUtils.CreateChild<VEvent>(asset, eventAssetName);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    if (GUI.Button(rect, "Raise"))
                    {
                        evt.Trigger();
                    }
                }
            }
            //EditorGUI.EndDisabledGroup();
            if (!isExpand) return;
            
            // if (evt == null)
            // {
            //     if (GUILayout.Button("[+] " + vProperty))
            //     {
            //         evt = VGUI.CreateChild<VEvent>(asset, (string.IsNullOrEmpty(asset.subName) ? asset.name : asset.subName) + "." + vProperty);
            //         AssetDatabase.SaveAssets();
            //     }

            //     return;
            // }
            
            if (vEditor == null)
            {
                so.Update();
                vEditor = Editor.CreateEditor(so.FindProperty(vProperty).objectReferenceValue);
            }

            if (vEditor != null) 
            {
                //VGUI.DrawObject(vEditor.target, 0.95f);
                vEditor.OnInspectorGUI();
            }

            if (allowDelete && VGUI.DeleteButton())
            {
                Delete(ref evt);
            }
        }

        void Delete(ref VEvent evt)
        {
            UnityObject.DestroyImmediate(evt, true);
            evt = null;
            vEditor = null;
                
            AssetDatabase.SaveAssets();
            EditorGUIUtility.ExitGUI();
        }
    }
    
    public class VAssetEventDrawer
    {
        public class DrawerInfo
        {
            public FieldInfo info;
            public VEvent vEvent;
            public VEventDrawer vDrawer;

            public void Draw(SerializedObject so, VAsset asset)
            {
                var pEvent = vEvent;
                vDrawer.Draw(so, asset, true, ref pEvent);

                if (pEvent != vEvent)
                {
                    info.SetValue(asset, pEvent);
                    asset.Editor_MarkAsDirty();
                    vEvent = (VEvent)info.GetValue(asset);
                }
            }
        }

        [NonSerialized] private List<DrawerInfo> listDrawers;
        public void Draw(SerializedObject so, VAsset asset)
        {
            if (listDrawers == null)
            {
                Refresh(asset);
            }

            for (int i = 0;i < listDrawers.Count; i++)
            {
                listDrawers[i].Draw(so, asset);
            }
        }

        void Refresh(VAsset asset)
        {
            var assetType = asset.GetType();

            // find all members of type VEvent
            FieldInfo[] fields = assetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            
            listDrawers = new List<DrawerInfo>();
            for (int i = 0;i < fields.Length; i++)
            {
                var f = fields[i];
                if (f.FieldType != typeof(VEvent)) continue;
                
                listDrawers.Add(new DrawerInfo()
                {
                    info = f,
                    vEvent = (VEvent)f.GetValue(asset),
                    vDrawer = new VEventDrawer(){ vProperty = f.Name }
                });
            }
        }
    }
}