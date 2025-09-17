using System.Collections;
using UnityEngine;
using UnityEditor;

namespace nano.va2
{
    public class VAssetEditorBase : Editor
    {
        public bool isChild;
    }

    public class VAssetEditor<T> : VAssetEditorBase where T : VAsset
    {
        // SUPPORT FOR EVENT
        VAssetEventDrawer eventDrawer;
        protected void DrawEvent(VAsset asset)
        {
            if (eventDrawer == null) eventDrawer = new VAssetEventDrawer();
            eventDrawer.Draw(serializedObject, asset);
        }
        
        protected void DrawEvent(bool allowDelete, ref VEventDrawer drawer, string property, ref VEvent evt)
        {
            if (drawer == null) drawer = new VEventDrawer { vProperty = property };
            drawer.Draw(serializedObject, (T)target, allowDelete, ref evt);
        }

        
        // SUPPORT FOR LIST
        VListDrawerJson jsonDrawer;
        void DrawJSON(VAsset asset)
        {
            if (asset is IPrimityT)
            {
                // VGUI.MiniHelpBox("Primity Type", MessageType.Info, 2f);
                return;
            }
            
            if (asset is IListAssetT)
            {
				if (asset is ISupportJson)
				{
					if (jsonDrawer == null) jsonDrawer = new VListDrawerJson();
					jsonDrawer.DrawIList(asset.GetValueT<IList>());
					return;
				}
            }
            
            // VGUI.MiniHelpBox("Unsupported Type", MessageType.Warning, 2f);
        }

        void DrawImportExport(VAsset asset)
        {
            GUILayout.BeginHorizontal();
            {
                if (asset is ISupportJson)
                {
                    if (GUILayout.Button("Copy JSON", EditorStyles.miniButton))
                    {
                        var json = (asset as ISupportJson).ToJson();
                        EditorGUIUtility.systemCopyBuffer = json;
                        Debug.Log("Copied:\n" + json + "\n");
                    }

                    if (GUILayout.Button("Paste JSON", EditorStyles.miniButton))
                    {
                        var json = EditorGUIUtility.systemCopyBuffer;
                        if ((asset as ISupportJson).FromJson(json))
                        {
                            Debug.Log("Pasted:\n" + json + "\n");
                            #if UNITY_EDITOR
                            EditorUtility.SetDirty(asset);
                            #endif
                        }
                        else
                        {
                            Debug.LogWarning("Invalid JSON: \n" + json + "\n");
                        }
                    }
                }
                
                if (asset is ISupportTSV)
                {
                    GUILayout.Space(32f);
                    
                    if (GUILayout.Button("Copy TSV", EditorStyles.miniButton))
                    {
                        var tsv = (asset as ISupportTSV).ToTSV();
                        EditorGUIUtility.systemCopyBuffer = tsv;
                        Debug.Log("Copied:\n" + tsv + "\n");
                    }

                    if (GUILayout.Button("Paste TSV", EditorStyles.miniButton))
                    {
                        var tsv = EditorGUIUtility.systemCopyBuffer;
                        if ((asset as ISupportTSV).FromTSV(tsv))
                        {
                            Debug.Log("Pasted:\n" + tsv + "\n");
                            #if UNITY_EDITOR
                            EditorUtility.SetDirty(asset);
                            #endif
                        }
                        else
                        {
                            Debug.LogWarning("Invalid TSV: \n\n" + tsv + "\n\n");
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        

        // SUPPORT FOR DRAW DEFAULT
        protected void DrawDefault(ref bool drawDefault, ref bool allowDelete)
        {
            if (drawDefault)
            {
                VGUI.FoldOut(ref drawDefault, "Default Inspector", 0f, 2f);
                DrawDefaultInspector();
            }
        }

        // DEFAULT DRAWER FOR CUSTOM ASSET
        protected virtual void OnDraw(T asset)
        {
            serializedObject.Update();

            if (VGUI.DrawProperty(serializedObject, "_value", true))
            {
                asset.SetRawValue(asset.GetRawValue(), false, true);
            }
        }

        public override void OnInspectorGUI()
        {
            if (this == null) return;
            if (target == null) return;

            var asset = (T)target;
            if (asset == null) return;

            var p = asset.parent;
            if (p != null && VGUI.DeleteButton())
            {
                var container = (p as IParentAsset).GetContainer();
                if (container != null) container.DetachChild(p, asset.idFromParent);
                DestroyImmediate(asset, true);
                
                if (p!= null) EditorUtility.SetDirty(p);

                AssetDatabase.SaveAssets();
                EditorGUIUtility.ExitGUI();
            }
            
            // Toggle ReadOnly
            if (asset is IVRef) // reference asset
            {
                VRefDrawer.Draw(asset);
            }
            else
            {
                // DATA asset
                asset.readOnly = GUILayout.Toggle(asset.readOnly, "ReadOnly");
                
                DrawImportExport(asset);

                EditorGUI.BeginDisabledGroup(asset.readOnly);
                {
                    DrawJSON(asset);
                    OnDraw(asset);
                }
                EditorGUI.EndDisabledGroup();
            }
            
            if (!isChild)
            {
                VGUI.MiniHelpBox("Actions / Events", MessageType.Info, 2f);

                DrawEvent(asset);
                DrawDefault(ref asset.drawDefault, ref asset.allowDelete);
            }
            
            
        }
    }

    public class VClassAssetEditor<TClass, TAsset> : VAssetEditor<TAsset> where TAsset: VClassAssetT<TClass> where TClass : class
    {
        VClassInfoDrawer drawer;
        
        protected override void OnDraw(TAsset asset)
        {
            if (drawer == null) drawer = new VClassInfoDrawer();
            
            serializedObject.Update();
            drawer.Draw<TClass>(serializedObject, asset);
        }
    }

    [CustomEditor(typeof(VEvent))]
    public class VEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (this == null) return;
            if (target == null) return;

            VGUI.DrawObject(target, 1f);
            base.OnInspectorGUI();
            
            VEvent e = target as VEvent;

            if (GUILayout.Button("Raise"))
            {
                e.Trigger();
            }
        }
    }
}

