using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace nano.va2
{

    public class VClass<TClass>
    {
        public static BindingFlags FLAG = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        [NonSerialized] public Dictionary<string, FieldInfo> fieldMap;
        public List<string> fieldIds;
        
        public void Scan()
        {
            fieldMap = new Dictionary<string, FieldInfo>();
            fieldIds = new List<string>();
            
            var fields = typeof(TClass).GetFields(FLAG);

            foreach (FieldInfo f in fields)
            {
                if (!VUtils.TypeAssetTMap.ContainsKey(f.FieldType))
                {
                    // Unsupport type
                    continue;
                }

                fieldMap.Add(f.Name, f);
                fieldIds.Add(f.Name);
            }
        }

        public void SetFieldValue(string id, object classValue, object fieldValue)
        {
            if (fieldMap == null) Scan();

            FieldInfo field;
            if (fieldMap.TryGetValue(id, out field))
            {
                field.SetValue(classValue, fieldValue);
            }
        }

        public object GetFieldValue(string id, object classValue)
        {
            if (fieldMap == null) Scan();

            FieldInfo field;
            if (fieldMap.TryGetValue(id, out field))
            {
                return field.GetValue(classValue);
            }

            return null;
        }
    }
    
    
    public class VClassAssetT<TClass> : VAssetT<TClass>, IParentAsset, IClassAssetT where TClass : class
    {
        public VClass<TClass> classInfo;
        public VContainer container = new VContainer();

        public void Init()
        {
            classInfo = new VClass<TClass>();
            classInfo.Scan();
        }
        
        public VContainer GetContainer()
        {
            return container;
        }
        
        protected override bool IsEqualT(TClass v1, TClass v2)
        {
            return v1 == v2;
        }
        
        internal void CopyClassValue2Asset()
        {
            if (classInfo == null) Init();

            var v = Value;
            if (v == null) return;
            
            foreach (var fi in classInfo.fieldMap)
            {
                var child = container.GetChild(fi.Key);
                if (child == null) continue;
                child.SetRawValue(fi.Value.GetValue(v), true, false, true);
            }
        }
        
        internal void CopyAssetValue2Class()
        {  
            if (classInfo == null) Init();

            var v = Value;
            if (v == null) return;
            
            foreach (var child in container.children)
            {
                var id = child.idFromParent;
                classInfo.SetFieldValue(id, v, child.rawValue);
            }
        }
        
        
        public bool FromJson(string json)
        { 
            TClass temp;
            
            try
            {
                temp = JsonUtility.FromJson<TClass>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid json for: " + this.variableName + "\n" + e + "\n\n" + json);
                return false;
            }

            SetValue(temp, false, false);
            Editor_MarkAsDirty();
            return true;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(_value, true);
        }

        public virtual void OnChildAssetChange(VAsset child)
        {
            if (classInfo == null) Init();

            var v = Value;
            if (v == null) return;
            
            classInfo.SetFieldValue(child.idFromParent, v, child.rawValue);
            TriggerChange(true);
        }

        public virtual void CopyChangesToChildren()
        {
            CopyClassValue2Asset();
        }
        
        
        public bool DestroyChild(VAsset child)
        {
            // do nothing
            Debug.Log(child.name + " : being destroyed!");
            return true;
        }
        
        public float labelWPercent = 0.5f;
    }
}