using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;

namespace nano.va2
{
    [CustomPropertyDrawer(typeof(VAsset))]
    public class VAssetPropertyDrawer : PropertyDrawer
    {
        
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {   
            var so = property.serializedObject;
            if (so == null || so.targetObject == null)
            {
                return;
            }

            var vasset = (VAsset)property.objectReferenceValue;
            if (vasset == null)
            {
                EditorGUI.PropertyField(rect, property, true);
                return;
            }

            // GUI.Label(rect, property.propertyPath);

            var isVRef      = vasset is IVRef;
            var isPrimity   = vasset is IPrimityT;
            var isInArray   = property.propertyPath.EndsWith("]", true, CultureInfo.CurrentCulture);
            
            if (isVRef || isPrimity || isInArray)
            {
                var rect2a = rect;
                var rect2b = rect;
                rect2a.width /= 2f;
                rect2b.xMin += rect2b.width / 2f;

                property.objectReferenceValue = EditorGUI.ObjectField(rect2a, vasset, typeof(VAsset), false);

                if (isVRef)
                {
                    var oldRef = vasset?.GetValueT<UnityObject>();
                    var newRef = EditorGUI.ObjectField(rect2b, oldRef, typeof(UnityObject), true);
                    if (oldRef != newRef) vasset.SetRawValue(newRef);
                    return;
                }

                if (isPrimity)
                {
                    var soAsset = new SerializedObject(vasset);
                    
                    if (soAsset != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            var spValue = soAsset.FindProperty("_value");
                            EditorGUI.PropertyField(rect2b, spValue, GUIContent.none, false);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            soAsset.ApplyModifiedProperties();
                        }
                    }
                    
                    return;
                }

                return;
            }
            
            EditorGUI.PropertyField(rect, property, true);
        }
    }
}
#endif