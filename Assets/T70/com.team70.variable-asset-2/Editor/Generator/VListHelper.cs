
using System.IO;
using UnityEditor;

namespace nano.va2
{
    public class VListHelper
    {
        const string ASSET_SCRIPT = 
            "using System;\n" + 
            "using UnityEngine;\n"+
            "using nano.va2;\n\n"+

            "[Serializable] public class CLASSNAME\n"+
            "{\n\n"+
            "}\n\n"+
            
            "[CreateAssetMenu(fileName = \"LOWERCLASSNAME.asset\", menuName = \"VAsset2/Custom/CLASSNAME\")]\n"+
            "public class ListCLASSNAMEAsset : VListAssetT<CLASSNAME>\n" +
            "{\n\n"+
            "}\n\n"+
            
            "#if UNITY_EDITOR\n" +
            "[UnityEditor.CustomEditor(typeof(ListCLASSNAMEAsset))]\n" +
            "public class ListCLASSNAMEAssetEditor : VAssetEditor<ListCLASSNAMEAsset> {}\n" +
            "#endif";
            
            
        [MenuItem("Assets/VAsset2/Generate List")]
        public static void Generate()
        {
            var path = VGUI.GetSelectedFolder();
            if (string.IsNullOrEmpty(path)) return;

            var className = VGUI.GetFileNameFromClipboard();
            if (string.IsNullOrEmpty(className)) return;

            VGUI.GenerateScript(Path.Combine(path, "List{0}Asset.cs"), className, ASSET_SCRIPT);
            // VGUI.SaveScript(Path.Combine(path, "List{0}AssetEditor.cs"), className, EDITOR_SCRIPT);

            AssetDatabase.Refresh();
        }
    }
}

