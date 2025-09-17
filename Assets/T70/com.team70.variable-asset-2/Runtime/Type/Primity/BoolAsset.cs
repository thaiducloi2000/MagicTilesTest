using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nano.va2 
{
    [CreateAssetMenu(fileName = "new.bool.asset", menuName = "VAsset2/Primity/Bool", order = 110)]
    public class BoolAsset : VAssetT<bool>, IPrimityT, ISupportJson
    {
        override protected bool IsEqualT(bool v1, bool v2)
        {
            return v1 == v2;
        }

        public string ToJson() { return _value ? "true" : "false"; }
        public bool FromJson(string json)
        {
            var v = json.ToLowerInvariant();
            SetValue((v == "true") || (v == "1"), false, true); // force notify changes
            Editor_MarkAsDirty();
            return true;
        }
    }
}