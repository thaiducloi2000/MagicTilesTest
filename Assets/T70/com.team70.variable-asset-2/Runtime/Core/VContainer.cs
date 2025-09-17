using System;
using UnityEngine;
using nano.va2;
using System.Collections.Generic;

namespace nano.va2
{
    [Serializable] public class VContainer
    {
        public List<VAsset> children = new List<VAsset>();
        [NonSerialized] protected Dictionary<string, VAsset> _cache;
        
        protected void RefreshCache() // _cache
        {
            _cache = new Dictionary<string, VAsset>();

            for (int i = 0; i < children.Count; i++)
            {
                var m = children[i];
                if (m == null)
                {
                    Debug.LogWarning("Null child found at " + i);
                    continue;
                }

                if (_cache.ContainsKey(m.idFromParent))
                {
                    Debug.LogWarning("Duplicated idFromParent: " + m.idFromParent);
                    continue;
                }
                
                _cache.Add(m.idFromParent, m);
            }
        }
        
        public VAsset GetChild(string id)
        {
            if (_cache == null) RefreshCache();
            VAsset info;
            if (_cache.TryGetValue(id, out info)) return info;
            return null;
        }
        
        #if UNITY_EDITOR
        public bool AttachChild(VAsset parent, string id, Type dataType)
        {
            // Debug.LogWarning("Attach: " + id);

            if (_cache == null) RefreshCache();
            if (_cache.ContainsKey(id)) return false;
            
            Type soType;
            if (!VUtils.TypeAssetTMap.TryGetValue(dataType, out soType))
            {
                Debug.LogWarning("Unsupported data type: " + dataType);
                return false;
            }

            var child = VUtils.CreateChild(parent, id, soType);
            child.idFromParent = id;
            child.parent = parent;

            #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(child);
                UnityEditor.EditorUtility.SetDirty(parent);
            #endif

            _cache.Add(id, child);
            children.Add(child);
            return true;
        }
        
        public bool DetachChild(VAsset parent, string id)
        {
            // Debug.LogWarning("Detach: " + id);

            if (_cache == null) RefreshCache();

            VAsset child;

            if (_cache.TryGetValue(id, out child)) 
            {   
                _cache.Remove(id);
                // Debug.Log("Removed from _cache: " + id);

                for (int i = children.Count-1; i>=0; i --)
                {
                    child = children[i];
                    
                    if (child != null && child.idFromParent != id) continue;
                    
                    children.RemoveAt(i);

                    if (child != null)
                    {
                        child.idFromParent = null;
                        child.parent = null;
                        UnityEngine.Object.DestroyImmediate(child, true);
                    }
                    
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(parent);
                    #endif
                    return true;
                }
            }
            
            return false;
        }

        #endif
    }
}


