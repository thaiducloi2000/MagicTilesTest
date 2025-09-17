using System;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace nano.va2
{
    public class VGUI
    {
        internal static void FoldOut(ref bool isFolding, string label, float rightMargin = 0f, float? xMin = null)
        {
            var r = GUILayoutUtility.GetRect(0, Screen.width, 18f, 18f);
            if (xMin != null) r.xMin = xMin.Value;
            r.xMax -= rightMargin;
            
            isFolding = GUI.Toggle(r, isFolding, label, EditorStyles.foldoutHeader);
        }

        internal static string GetFileNameFromClipboard()
        {
            var clipboard = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clipboard)) 
            {
                Debug.LogWarning("Invalid clipboard --> must be a valid Identifier: " + clipboard);
                return null;
            }

            if (clipboard.Contains(" "))
            {
                Debug.LogWarning("Invalid clipboard --> must be a class name (current clipboard contains space)");
                return null;
            }

            return clipboard;
        }

        internal static void GenerateScript(string pathFormat, string className, string sourceTemplate)
        { 
            var assetPath   = string.Format(pathFormat, className);
            Directory.CreateDirectory(assetPath.Substring(0, assetPath.LastIndexOf("/")));

            var content     = sourceTemplate.Replace("LOWERCLASSNAME", className.ToLower())
                                            .Replace("CLASSNAME", className);

            File.WriteAllText(assetPath, content);
        }

        internal static string GetSelectedFolder()
        { 
            var folder = Selection.activeObject;
            if (folder == null)
            {
                Debug.LogWarning("Invalid --> must select a folder!");
                return null;
            }

            var path = AssetDatabase.GetAssetPath(folder);
            var isValid = AssetDatabase.IsValidFolder(path);
            if (!isValid)
            {
                Debug.LogWarning("Invalid --> must select a folder!");
                return null;
            }

            return path;
        }

        internal static bool DeleteButton()
        {
            var c = GUI.color;
            GUI.color = new Color(0.8f, 0f, 0f, 1f);
            if (GUILayout.Button("[X] DELETE", GUILayout.Width(100f)))
            {
                GUI.color = c;
                return true;
            }

            GUI.color = c;
            return false;
        }

        static public float requestTime;        
        static public void DelayRefreshAssetDB()
        {
            requestTime = Time.realtimeSinceStartup;
            EditorApplication.update -= DoRefreshAssetDB;
            EditorApplication.update += DoRefreshAssetDB;
        }

        static void DoRefreshAssetDB()
        {
            if (Time.realtimeSinceStartup - requestTime < 3f) return;

            EditorApplication.update -= DoRefreshAssetDB;
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        internal static float SetLabelWPercent(float pct)
        { 
            var w = GUILayoutUtility.GetRect(0, Screen.width, 0, 0).width;
            var lbSize = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = w * pct;
            return lbSize;
        }

        internal static void MiniHelpBox(string message, MessageType type, float paddingY = 2)
        {
            var h = 24f;
            var rect = GUILayoutUtility.GetRect(0, Screen.width, h + 2 * paddingY, h + 2 * paddingY);
            rect.y += 2f;
            rect.height = h;

            EditorGUI.HelpBox(rect, message, type);
        }

        internal static bool DrawObject(UnityEngine.Object targetObj, float wpercent)
        {
            var tmpName = targetObj.name;
            var newName = EditorGUILayout.TextField(tmpName, EditorStyles.objectField, GUILayout.Width(EditorGUIUtility.currentViewWidth * wpercent));
            if (newName == tmpName) return false;
            
            targetObj.name = newName;
            EditorUtility.SetDirty(targetObj);
            DelayRefreshAssetDB();
            return true;
        }


        internal static bool DrawProperty(SerializedObject so, string propertyName, bool hideLabel = false)
        {
            var prop = so.FindProperty(propertyName);

            if (!prop.hasChildren || (prop.propertyType == SerializedPropertyType.String))
            {
                GUILayout.BeginHorizontal();
                {
                    DrawObject(so.targetObject, 0.5f);

                    if (DrawProperty(prop, hideLabel))
                    {
                        so.ApplyModifiedProperties();
                        return true;
                    }
                }
                GUILayout.EndHorizontal();
                return false;
            }
            
            if (DrawProperty(prop, hideLabel))
            {
                so.ApplyModifiedProperties();
                return true;
            }

            var rect = GUILayoutUtility.GetLastRect();
            rect.height = 18f;
            rect.xMin += 18f;
            EditorGUI.ObjectField(rect, so.targetObject, typeof(ScriptableObject), false);
            
            return false;
        }
        
        internal static bool DrawProperty(SerializedProperty prop, bool hideLabel = false)
        {
            //GUILayout.Label(prop.propertyType.ToString());
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(8f);
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.PropertyField(prop, GUIContent.none);//hideLabel ? GUIContent.none : null
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            return EditorGUI.EndChangeCheck();
        }
    }
}
