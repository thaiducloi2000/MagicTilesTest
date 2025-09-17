using UnityEngine;

namespace nano.va2
{
    [CreateAssetMenu(fileName = "new.double.asset", menuName = "VAsset2/Primity/Double", order = 110)]
    public class DoubleAsset : VAssetT<double>, IFloat, IInt, IPrimityT, ISupportJson
    {
        public float floatValue { get { return (float)_value; } }
        public int intValue { get { return (int)_value; } }

        override protected bool IsEqualT(double v1, double v2)
        {
            return v1 == v2;
        }
		
        public static DoubleAsset operator ++(DoubleAsset asset)
        {
            asset.Value++;
            return asset;
        }

        public static DoubleAsset operator --(DoubleAsset asset)
        {
            asset.Value--;
            return asset;
        }

        public string ToJson() { return _value.ToString(); }
        public bool FromJson(string json)
        {
            if (!double.TryParse(json, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out _value)) return false;
            FinishEditValueDirectly();
            Editor_MarkAsDirty();
            return true;
        }
    }
}