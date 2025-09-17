using System.IO;
using UnityEditor;

namespace nano.va2
{
    public class VRefHelper
    {
        const string ASSET_SCRIPT = 
            "using System;\n" + 
            "using UnityEngine;\n"+
            "using nano.va2;\n\n"+
            
            "[CreateAssetMenu(fileName = \"ref.LOWERCLASSNAME.asset\", menuName = \"VAsset2/Ref/CLASSNAME\")]\n"+
            "public class VRef_CLASSNAME : VRef_ComponentT<CLASSNAME>\n" +
            "{\n\n"+
            "}\n\n"
            ;
        
        [MenuItem("Assets/VAsset2/Generate VRef")]
        public static void Generate()
        {
            var path = VGUI.GetSelectedFolder();
            if (string.IsNullOrEmpty(path)) return;

            var className = VGUI.GetFileNameFromClipboard();
            if (string.IsNullOrEmpty(className)) return;

            VGUI.GenerateScript(Path.Combine(path, "VRef_{0}.cs"), className, ASSET_SCRIPT);
            AssetDatabase.Refresh();
        }
    }
}