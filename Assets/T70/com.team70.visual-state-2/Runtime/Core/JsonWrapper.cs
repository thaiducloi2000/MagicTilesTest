using System;
using UnityEngine;
using UnityEditor;
namespace nano.vs2
{
    public class JsonWrapper
    {

        public static object FromJson(string json, Type t, bool SingleValue)
        {
            if (SingleValue) return getJsonSingleValue(json, t);
            try
            {
                return JsonUtility.FromJson(json, t);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Parse error json: " + json + " type: " + t);
                Debug.LogWarning("mess: " + ex);

                return null;
            }

        }
        public static string ToJson(object obj, bool SingleVale)
        {
            if (SingleVale) return toJsonSingleValue(obj);
            return JsonUtility.ToJson(obj);
        }
        /*static string partent = "{0}\"val\":{1}{2}";*/
        private static object getJsonSingleValue(string json, Type t)
        {
            if (t == typeof(string)) return json;
            return tryParseSingle(json, t);
            /*object obj = tryParseSingle(json, t);
            if (obj != null)
            {
                return obj;
            }

            //string newJson = "{\"val\":" + json + "}";
            // Debug.Log("new json: " + newJson);

            var type = typeof(Wrapper<>).MakeGenericType(t);
            object a_Context = Activator.CreateInstance(type);
            return (JsonUtility.FromJson(string.Format(partent, '{', json, '}'), a_Context.GetType()) as IWrapper).value;*/
        }
        private static object tryParseSingle(string json, Type t)
        {
            if (t.IsEnum)
            {
                return Enum.Parse(t, json);
            }
            
            if (t.IsClass) return null;
            if (t == typeof(int))
            {
                if (!int.TryParse(json, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int intValue))
                {
                    Debug.LogWarning("Invalid <int> value: " + json);
                } 
                return intValue;
            }

            if (t == typeof(float))
            {
                if (!float.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
                {
                    Debug.LogWarning("Invalid <float> value: " + json);
                } 
                return floatValue;
            }

            if (t == typeof(double))
            {
                if (!double.TryParse(json, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                {
                    Debug.LogWarning("Invalid <double> value: " + json);
                } 
                
                return doubleValue;
            }

            if (t == typeof(long))
            {
                if (!long.TryParse(json, System.Globalization.NumberStyles.Integer | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite, System.Globalization.CultureInfo.InvariantCulture, out long longValue))
                {
                    Debug.LogWarning("Invalid <long> value: " + json);
                } 
                
                return longValue;
            }
            
            if (t == typeof(bool))
            {
                if (json.Length == 1) return json == "1";
                return bool.Parse(json);
            }
            
            Debug.LogWarning("Unsupported type: " + t);
            return null;
        }
        
        
        private static string toJsonSingleValue(object obj)
        {
            return obj.ToString();
        }


#if UNITY_EDITOR
        public static string GetFullJson(SerializedProperty prop)
        {
            string path = prop.propertyPath;

            string result = "{";
            while (prop.NextVisible(true))
            {
                var childPath = prop.propertyPath.Replace(path + ".", string.Empty);
                var arr = childPath.Split('.');
                if (arr.Length == 1)
                {
                    object obj = null;
                    if (VisualState_Unity.TryGetPropertyValue(prop, out obj))
                        result += string.Format("\"{0}\":{1},", childPath, obj == null ? "null" : obj.ToString());
                }
                else
                {
                    result += string.Format("\"{0}\":{1},", childPath, GetFullJson(prop.Copy()));
                }
            }
            return result.TrimEnd(',') + "}";

        }
#endif
        public interface IWrapper
        {
            object value { get; }
        }
        [Serializable]
        private class Wrapper<T> : IWrapper
        {
            public T val;
            public object value
            {
                get
                {
                    return val;
                }
            }
        }
    }
}