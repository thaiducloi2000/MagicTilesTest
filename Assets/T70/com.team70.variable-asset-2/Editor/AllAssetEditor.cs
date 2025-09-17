using UnityEditor;

namespace nano.va2
{
    [CustomEditor(typeof(BoolAsset))] public class BoolAssetEditor : VAssetEditor<BoolAsset> { }
    [CustomEditor(typeof(IntAsset))] public class IntAssetEditor : VAssetEditor<IntAsset> { }
    [CustomEditor(typeof(FloatAsset))] public class FloatAssetEditor : VAssetEditor<FloatAsset> {}
	[CustomEditor(typeof(DoubleAsset))] public class DoubleAssetEditor : VAssetEditor<DoubleAsset> {}
    [CustomEditor(typeof(StringAsset))] public class StringAssetEditor : VAssetEditor<StringAsset> {}


    // STRUCT
    [CustomEditor(typeof(ColorAsset))] public class ColorAssetEditor : VAssetEditor<ColorAsset> {}

    
	// GAME
	[CustomEditor(typeof(ConfigAsset))] public class ConfigAssetEditor : VAssetEditor<ConfigAsset> { }
}