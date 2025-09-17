using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nano.va2
{
    [CreateAssetMenu(fileName = "new.int.asset", menuName = "VAsset2/Primity/Int", order = 110)]
    public class IntAsset : VAssetT<int>, IFloat, IInt, IPrimityT, ISupportJson
    {
        public float floatValue { get { return (float)_value; } }
        public int intValue { get { return _value; } }

        protected override bool IsEqualT(int v1, int v2)
        {
            return v1 == v2;
        }

        public static IntAsset operator ++(IntAsset asset)
        {
            asset.Value++;
            return asset;
        }

        public static IntAsset operator --(IntAsset asset)
        {
            asset.Value--;
            return asset;
        }

        public string ToJson() { return _value.ToString(); }
        public bool FromJson(string json)
        {
            if (!int.TryParse(json, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _value)) return false;
            FinishEditValueDirectly();
            Editor_MarkAsDirty();
            return true;
        }
    }
}