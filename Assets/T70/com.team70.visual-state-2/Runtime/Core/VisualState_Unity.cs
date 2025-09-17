

#if UNITY_5_3_OR_NEWER
    #define UNITY_4_3_OR_NEWER
    #define UNITY_4_4_OR_NEWER
    #define UNITY_4_5_OR_NEWER
    #define UNITY_4_6_OR_NEWER
    #define UNITY_4_7_OR_NEWER
    #define UNITY_5_0_OR_NEWER
    #define UNITY_5_1_OR_NEWER
    #define UNITY_5_2_OR_NEWER
#else
    #if UNITY_5
    #define UNITY_4_3_OR_NEWER
    #define UNITY_4_4_OR_NEWER
    #define UNITY_4_5_OR_NEWER
    #define UNITY_4_6_OR_NEWER
    #define UNITY_4_7_OR_NEWER

    #if UNITY_5_0
    #define UNITY_5_0_OR_NEWER
    #elif UNITY_5_1
#define UNITY_5_0_OR_NEWER
#define UNITY_5_1_OR_NEWER
#elif UNITY_5_2
#define UNITY_5_0_OR_NEWER
#define UNITY_5_1_OR_NEWER
#define UNITY_5_2_OR_NEWER
#endif
#else
#if UNITY_4_3
#define UNITY_4_3_OR_NEWER
#elif UNITY_4_4
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#elif UNITY_4_5
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#define UNITY_4_5_OR_NEWER
#elif UNITY_4_6
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#define UNITY_4_5_OR_NEWER
#define UNITY_4_6_OR_NEWER
#elif UNITY_4_7
#define UNITY_4_3_OR_NEWER
#define UNITY_4_4_OR_NEWER
#define UNITY_4_5_OR_NEWER
#define UNITY_4_6_OR_NEWER
#define UNITY_4_7_OR_NEWER
#endif
#endif
#endif

using System;
using System.Reflection;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


using Object = UnityEngine.Object;

namespace nano.vs2
{
    public class VisualState_Unity
    {
        public static HashSet<string> PROPERTIES_IGNORES = new HashSet<string>
        {
            "m_LocalEulerAnglesHint.z",
            "m_LocalEulerAnglesHint.y",
            "m_LocalEulerAnglesHint.x"
        };

        public static Dictionary<string, string> PROPERTIES_CHANGED_NAME = new Dictionary<string, string>
        {
            {"m_BackGroundColor.r","backgroundColor.r"},
            {"m_BackGroundColor.g","backgroundColor.g"},
            {"m_BackGroundColor.b","backgroundColor.b"},
            {"m_BackGroundColor.a","backgroundColor.a"}


            ,{"m_CullingMask.m_Bits","cullingMask"}
            ,{"field of view","fieldOfView"}
            ,{"near clip plane","nearClipPlane"}
            ,{"far clip plane","farClipPlane"}
            ,{"m_OcclusionCulling","useOcclusionCulling"}
            ,{"m_HDR","allowHDR"}
            ,{"orthographic size","orthographicSize"}

            // ,{"m_projectionMatrixMode","usePhysicalProperties"}

            ,{"m_NormalizedViewPortRect.width","rect.width"}
            ,{"m_NormalizedViewPortRect.height","rect.height"}
            ,{"m_NormalizedViewPortRect.x","rect.x"}
            ,{"m_NormalizedViewPortRect.y","rect.y"}
            // ,{"m_NormalizedViewPortRect.width","backgroundColor.a"}


            //Light
            ,{"m_Shadows.m_Type","shadows"}
        };

        public static Dictionary<Type, Dictionary<string, string>> COM_PROPERTIES_CHANGED_NAME = new Dictionary<Type, Dictionary<string, string>>
        {
            {typeof (Light), new Dictionary<string, string>
                {
                    { "m_Type", "type"}

                }
            },



            {
            typeof (ParticleSystem), new Dictionary<string, string>
                {
                    { "InitialModule.startSpeed.scalar", "startSpeed"}//InitialModule.startSpeed.scalar  0,
                    ,{"lengthInSec","lengthInSec"}
                ,   {"InitialModule.startColor.maxColor.r", "startColor.r" }
                ,   {"InitialModule.startColor.maxColor.g", "startColor.g" }
                ,   {"InitialModule.startColor.maxColor.b", "startColor.b" }
                ,   {"InitialModule.startColor.maxColor.a", "startColor.a" }

                }
            }
        };



        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public |
                                   BindingFlags.NonPublic;

#if UNITY_EDITOR
        /// <summary>
        /// index 0 is value from
        /// </summary>
        public static PropertyModification[] GetValueModification(UndoPropertyModification undoProperty)
        {
            PropertyModification[] arr = new PropertyModification[2];
#if UNITY_5_1_OR_NEWER
        arr[0] = undoProperty.previousValue;
        arr[1] = undoProperty.currentValue;
#else
            arr[0] = undoProperty.propertyModification;
            arr[1] = GetCurModificationTo(undoProperty.propertyModification);
#endif
            return arr;
        }




        public static bool TryGetPropertyValue(SerializedProperty prop, out object obj)
        {
            var t = prop.propertyType;

            switch (t)
            {
                case SerializedPropertyType.AnimationCurve: obj = prop.animationCurveValue; return true;
                case SerializedPropertyType.ArraySize: obj = prop.arraySize; return true;
                case SerializedPropertyType.Boolean: obj = prop.boolValue ? 1: 0; return true;
                case SerializedPropertyType.Bounds: obj = prop.boundsValue; return true;
                case SerializedPropertyType.BoundsInt: obj = prop.boundsIntValue; return true;
                case SerializedPropertyType.Character: obj = prop.stringValue; return true;
                case SerializedPropertyType.Color: obj = prop.colorValue; return true;
                case SerializedPropertyType.Enum:
                    obj = prop.enumValueIndex; return true;
                case SerializedPropertyType.ExposedReference:
                    obj = prop.exposedReferenceValue; return true;
                case SerializedPropertyType.FixedBufferSize:
                    obj = prop.fixedBufferSize; return true;
                case SerializedPropertyType.Float:
                    obj = prop.floatValue; return true;
                case SerializedPropertyType.Integer:
                    obj = prop.intValue; return true;
                case SerializedPropertyType.LayerMask:
                    obj = prop.intValue; return true;

                case SerializedPropertyType.ObjectReference:
                    obj = prop.objectReferenceValue; return true;

                case SerializedPropertyType.Quaternion:
                    obj = prop.objectReferenceValue; return true;
                case SerializedPropertyType.Rect:
                    obj = prop.rectValue; return true;
                case SerializedPropertyType.RectInt:
                    obj = prop.rectIntValue; return true;
                case SerializedPropertyType.String:
                    obj = prop.stringValue; return true;
                case SerializedPropertyType.Vector2:
                    obj = prop.vector2Value; return true;
                case SerializedPropertyType.Vector2Int:
                    obj = prop.vector2IntValue; return true;
                case SerializedPropertyType.Vector3:
                    obj = prop.vector3Value; return true;

                case SerializedPropertyType.Vector3Int:
                    obj = prop.vector3IntValue; return true;
                case SerializedPropertyType.Vector4:
                    obj = prop.vector4Value; return true;



            }
            Debug.LogWarning("value not support " + prop.propertyType);
            obj = null;
            return false;
        }

#endif
        /// <summary>
        /// check and set dirty to tell unity update target
        /// </summary>
        public static void CheckUpdateDirtyState(UnityEngine.Object target)
        {
			if (target == null) return;
			
#if UNITY_4_6_OR_NEWER
        if (target is UnityEngine.UI.Graphic)
        {
            ((UnityEngine.UI.Graphic)target).SetAllDirty();
        }
        else if (target is UnityEngine.UI.MaskableGraphic)
        {
            ((UnityEngine.UI.MaskableGraphic)target).SetAllDirty();

        }
        else if(target is VisualState)
            {
                var vs = (target as VisualState);
                if (vs == null) return;
                vs.RefreshState();
            }

#endif
        }
        
        

    private static object GetValue<T>(object val)
    {
        var valueT = val.GetType();
        var targetT = typeof(T);
        if(valueT == targetT)
        {
            return (T)val;
        }
        else if(valueT == typeof(string))
        {
            var s = val.ToString();
            if(targetT == typeof(int))
            {
                int result;
                int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if(targetT == typeof(float))
            {
                float result;
                float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if(targetT == typeof(double))
            {
                double result;
                double.TryParse(s, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
        }
        // Debug.LogWarning("Type not support " + targetT + "  " + valueT);
        return val;
    }

        public static bool GetPercentValue(object val, float amount, out object result)
        {
            if (val == null)
            {
                result = null;
                Debug.LogWarning("----> should never be here! ");
                return false;
            }
            
            var type = val.GetType();
            result = null;
            if (type == typeof(int) || type.IsEnum)
            {
                //result = (int)((int)val * amount);
                result = Mathf.RoundToInt((int)val * amount);
                //result = Mathf.CeilToInt((int)val * amount);
                //result = Mathf.FloorToInt((int)val * amount);
            }
            else if (type == typeof(float))
            {
                result = (float)val * amount;
            }
            else if (type == typeof(double))
            {
                result = (double)val * amount;
            }

            else if (type == typeof(Vector2))
            {
                result = (Vector2)val * amount;
            }
            else if (type == typeof(Vector3))
            {
                result = (Vector3)val * amount;
            }
            else if (type == typeof(Vector4))
            {
                result = (Vector4)val * amount;
            }
            else if (type == typeof(Color))
            {
                result = (Color)val * amount;
            }
            else if (type == typeof(Color32))
            {
                result = (Color32)((Color)val * amount);
            }
            else if(type == typeof(bool))
            {
                result = amount >= 0.5f ? true : false;
            }
            else
            {
                result = val;
                 Debug.LogWarning("Type: " + type + " is NOT supported, will return " + val);
                return false;
            }
            return true;
        }
        public static bool Add2Value(object val1, object val2, ref object result, float amount, Type t = null)
        {
            //Debug.Log("Add2Value: " + val1 + " : " + val2);
            var type = t;
            
            if (t == null)
            {
                try
                {
                    type = val1.GetType();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to Add2Value, val1 == null & t == null {e.ToString()}");
                    return false;
                }
            }
            
            if (type == typeof(int))
            {
                result = (int)val1 + (int)val2; return true;
            }
            if (type == typeof(float))
            {
                result = (float)val1 + (float)val2; return true;
            }
            if (type == typeof(double))
            {
                result = (double)val1 + (double)val2; return true;
            }

            if (type == typeof(Vector2))
            {
                result = (Vector2)val1 + (Vector2)val2; return true;
            }
            if (type == typeof(Vector3))
            {
                result = (Vector3)val1 + (Vector3)val2; return true;
            }
            if (type == typeof(Vector4))
            {
                result = (Vector4)val1 + (Vector4)val2; return true;
            }



            if (type == typeof(Color))
            {
                result = (Color)val1 + (Color)val2;
                return true;
            }
            if (type == typeof(Color32))
            {
                result = (Color32)((Color)val1 + (Color)val2);
                return true;
            }

            if (type == typeof(string))
            {
                result = (string)val1 + (string)val2;
                return true;
            }
            result = amount >= 1 ? val2 : val1;

            // Debug.LogWarning("Type: " + type + " is NOT supported, will return " + val2);
            return false;
        }

        public static void GetallField(Type type)
        {
            Debug.Log("===Property===");
            foreach (var item in type.GetProperties(VSGetSet.FLAGS))
            {
                Debug.Log(item.Name);
            }

            Debug.Log("===Field===");
            var fields = type.GetFields(VSGetSet.FLAGS);
            foreach (var item in fields)
            {
                Debug.Log(item.Name);
            }
        }

        public static string GetFullPath(Object obj)
        {
            if (obj == null) return string.Empty;
            Transform transform = null;
            if (obj is Component) transform = (obj as Component).transform;
            else if (obj is GameObject) transform = (obj as GameObject).transform;
            else
            {
                return obj.name;
            }

            var s = obj.name;
            while(transform.parent != null)
            {
                s = transform.parent.name +"/"+s;
                transform = transform.parent;
            }
            return s;


        }


    }

    
}