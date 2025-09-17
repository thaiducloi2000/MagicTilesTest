using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace nano.va2
{
    public static class VUtils
    {
        public static Dictionary<Type, Type> TypeAssetTMap = new Dictionary<Type, Type>()
        {
            {typeof(bool), typeof(BoolAsset)},
            {typeof(float), typeof(FloatAsset)},
			{typeof(double), typeof(DoubleAsset)},
            {typeof(int), typeof(IntAsset)},
            {typeof(string), typeof(StringAsset)},

            {typeof(Animator), typeof(VRef_Animator)},
            {typeof(AudioSource), typeof(VRef_AudioSource)},
            {typeof(Collider), typeof(VRef_Light)},
            {typeof(MonoBehaviour), typeof(VRef_MonoBehaviour)},
            {typeof(Renderer), typeof(VRef_Renderer)},
            {typeof(Rigidbody), typeof(VRef_Rigidbody)},
            {typeof(Transform), typeof(VRef_Transform)},


        };

        #if UNITY_EDITOR

        public static ET CreateChild<ET>(VAsset parent, string eName) where ET : ScriptableObject
        {
            var element = ScriptableObject.CreateInstance<ET>();
            element.name = eName;
            AssetDatabase.AddObjectToAsset(element, parent);
            return element;
        }

        public static VAsset CreateChild(VAsset parent, string eName, Type eType)
        {
            var child = (VAsset)ScriptableObject.CreateInstance(eType);
            child.name = eName;
            AssetDatabase.AddObjectToAsset(child, parent);
            return child;
        }
        #endif


        private const char EOL = '\n';

        public static Dictionary<string, string> ReadKJson(string kjson, char delimiter = '\t')
        {
            /*
                key1 : json_1
                key2 : json_2
                key3 : json_3
                key4 : json_4
            */

            var length = kjson.Length;
            var result = new Dictionary<string, string>();
            
            StringBuilder k = new StringBuilder();
            StringBuilder v = new StringBuilder();
            
            int i = -1;
            bool isKey = true;

            while (++i < length)
            {
                var c = kjson[i];

                if (i == length-1)
                {
                    v.Append(c);
                    c = EOL;
                }
                
                if (c == EOL)
                {
                    var key = k.ToString().Trim();

                    if (!result.ContainsKey(key))
                    {
                        var value = v.ToString().Trim();
                        result.Add(key, value);
                    }
                    
                    isKey = true;
                    k.Clear();
                    v.Clear();
                    continue;
                }
                
                if (c == delimiter)
                {
                    isKey = false;
                    continue;
                }

                if (isKey)
                {
                    k.Append(c);
                }
                else
                {
                    v.Append(c);
                }
            }

            return result;
        }

        static public void SafeTrigger(this VEvent evt)
        {
            if (evt == null) return;
            #if UNITY_EDITOR
            {
                evt.Trigger();  // do not try-catch in editor mode
            }
            #else
            try 
            {
                evt.Trigger();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("Exception: " + e);
            }
            #endif
        }
    }
}