using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nano.va2
{
    [CreateAssetMenu(fileName = "new.config.asset", menuName = "VAsset2/Game/Config", order = 110)]
    public class ConfigAsset : VListAssetT<VAsset>
    {
        [NonSerialized] private Dictionary<string, VAsset> _map;
        protected Dictionary<string, VAsset> assetMap
        {
            get
            {
                if (_map != null) return _map;

                _map = new Dictionary<string, VAsset>();
                for (int i = 0; i < _value.Count; i++)
                {
                    var asset = _value[i];
                    if (asset == null) continue;
                    var assetName = asset.variableName;
                    if (_map.ContainsKey(assetName))
                    {
#if UNITY_EDITOR
                    Debug.LogWarningFormat("[Editor] Duplicated asset with name {0}", assetName);
#endif
                        continue;
                    }

					if (asset is IVRef)
					{
#if UNITY_EDITOR
                    Debug.LogWarningFormat("[Editor] ConfigAsset can not hold IVRef asset: {0}", assetName);
#endif
					}

					if (!(asset is ISupportJson))
					{
#if UNITY_EDITOR
                    Debug.LogWarningFormat("[Editor] ConfigAsset element must implement ISupportJson: {0}", assetName);
#endif
					}

                    _map.Add(assetName, asset);
                }
                return _map;
            }
        }

        public bool FromDictionary(Dictionary<string, string> data)
        {
            var map = assetMap;
            var counter = 0;

            // Debug.LogWarning(this + " Config Asset --> FromDictionary: " + data.Count + " --> " + map.Count);
                
            foreach (var kvp in data)
            {
                VAsset asset;
                if (!map.TryGetValue(kvp.Key, out asset))
                {
#if UNITY_EDITOR
                    Debug.LogWarningFormat("[Editor] Asset with name {0} not found", kvp.Key);
#endif
                    continue;
                }
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    Debug.LogWarning("Empty value for key: " + kvp.Key);
                    continue;
                }
                //Debug.Log(asset.name + " :: " + kvp.Key + " --> " + kvp.Value);
                
                var config = asset as ISupportJson;
                if (config.FromJson(kvp.Value))
                {
                    counter++;
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(asset);
#endif
                }
            }
            return counter > 0;
        }

        public bool FromKJson(string kjson)
        {
            // try - catch ?
            FromDictionary(VUtils.ReadKJson(kjson));
            return true;
        }

		

        public string ToKJson()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < _value.Count; i++)
            {
                var item = _value[i];
                var config = item as ISupportJson;
                sb.AppendFormat("{0}\t{1}\n", item.variableName, config != null ? config.ToJson() : "null");
            }
            return sb.ToString();
        }
        
#if UNITY_EDITOR

        [ContextMenu("Copy KJson")]
        public void CopyKJson()
        {
            var kjson = ToKJson();
            UnityEditor.EditorGUIUtility.systemCopyBuffer = kjson;
            Debug.Log(kjson);
        }
        
        [ContextMenu("Polish")]
        public void PolishAssets()
        {   
            _map = null;
            _value = assetMap.Values.ToList();
            _value.Sort((item1, item2) => String.Compare(item1.variableName, item2.variableName, StringComparison.Ordinal));

            // force notify event changes
            var result = this.ToKJson();
            Debug.Log(this.name + " (copied to clipboard) \n" + result);
            
            UnityEditor.EditorGUIUtility.systemCopyBuffer = result;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

