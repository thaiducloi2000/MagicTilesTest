using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace nano.va2
{
    public class VRefDrawer
    {
        public static void Draw(VAsset asset)
        {
            var rect = GUILayoutUtility.GetRect(0, Screen.width, 18f, 18f);
            rect.width = rect.width / 2f - 2f;
            EditorGUI.ObjectField(rect, asset, typeof(VAsset), false);

            rect.x += rect.width + 2f;

            var t1 = asset.GetValueT<UnityObject>();
            var t2 = EditorGUI.ObjectField(rect, t1, typeof(UnityObject), true);
            if (t2 != t1)
            {
                if (t2 is GameObject && asset is IVRefComponent)
                {
                    t2 = (t2 as GameObject).GetComponent((asset as IVRefComponent).targetTypeT);
                    if (t2 == null) return;
                }
                
                asset.SetRawValue(t2, false);
            }
        }
    }
}



