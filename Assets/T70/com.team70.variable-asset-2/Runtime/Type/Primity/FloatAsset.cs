using UnityEngine;

namespace nano.va2
{
    [CreateAssetMenu(fileName = "new.float.asset", menuName = "VAsset2/Primity/Float", order = 110)]
    public class FloatAsset : VAssetT<float>, IFloat, IInt, IPrimityT, ISupportJson
    {
        public float floatValue { get { return _value; } }
        public int intValue { get { return (int)_value; } }

        override protected bool IsEqualT(float v1, float v2)
        {
            return v1 == v2;
        }

        public static FloatAsset operator ++(FloatAsset asset)
        {
            asset.Value++;
            return asset;
        }

        public static FloatAsset operator --(FloatAsset asset)
        {
            asset.Value--;
            return asset;
        }

        public string ToJson() { return _value.ToString(); }
        public bool FromJson(string json)
        {
            if (!float.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _value)) return false;
            FinishEditValueDirectly();
            Editor_MarkAsDirty();
            return true;
        }
    }
}