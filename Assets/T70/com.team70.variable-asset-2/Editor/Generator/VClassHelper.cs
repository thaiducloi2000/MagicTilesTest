using UnityEngine;
using UnityEditor;
using System.IO;

namespace nano.va2
{
    public class VClassHelper
    {
        const string ASSET_SCRIPT = 
            "using System;\n" + 
            "using UnityEngine;\n"+
            "using nano.va2;\n\n"+

            "[Serializable] public class CLASSNAME\n"+
            "{\n\n"+
            "}\n\n"+
            
            "[CreateAssetMenu(fileName = \"LOWERCLASSNAME.asset\", menuName = \"VAsset2/Custom/CLASSNAME\")]\n"+
            "public class CLASSNAMEAsset : VClassAssetT<CLASSNAME>\n" +
            "{\n\n"+
            "}\n\n";

        const string EDITOR_SCRIPT = 
            "using nano.va2;\n"+
            "using UnityEditor;\n\n"+

            "[CustomEditor(typeof(CLASSNAMEAsset))]\n" +
            "public class CLASSNAMEAssetEditor : VClassAssetEditor<CLASSNAME, CLASSNAMEAsset>\n" +
            "{\n\n"+
            "}\n\n";

        


        [MenuItem("Assets/VAsset2/Generate Class")]
        public static void Generate()
        {
            var path = VGUI.GetSelectedFolder();
            if (string.IsNullOrEmpty(path)) return;

            var className = VGUI.GetFileNameFromClipboard();
            if (string.IsNullOrEmpty(className)) return;

            VGUI.GenerateScript(Path.Combine(path, "Runtime", "{0}Asset.cs"), className, ASSET_SCRIPT);
            VGUI.GenerateScript(Path.Combine(path, "Editor", "{0}AssetEditor.cs"), className, EDITOR_SCRIPT);
            
            AssetDatabase.Refresh();
        }
    }
}