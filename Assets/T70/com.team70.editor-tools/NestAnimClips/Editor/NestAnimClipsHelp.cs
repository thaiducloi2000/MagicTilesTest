using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class NestAnimClipsHelp : MonoBehaviour
{
    /*[MenuItem("Assets/T70/Nest", true )]
    static bool NestAnimClipsValidateNestAnimClipsValidate()
    {
        return Selection.activeObject is AnimatorController;
    }*/

    [MenuItem("Assets/T70/Nest AnimationClips")]
    static void NestAnimClips()
    {
        AnimatorController animController = (AnimatorController)Selection.activeObject;
        if (animController == null) return;

        // Get all objects currently in Controller asset, we'll destroy them later
        UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animController));

        AssetDatabase.SaveAssets();

        // Add animations from all animation layers, without duplicating them
        var oldToNew = new Dictionary<AnimationClip, AnimationClip>();
        foreach (AnimatorControllerLayer layer in animController.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                var old = state.state.motion as AnimationClip;
                if (old == null) continue;

                if (!oldToNew.ContainsKey(old)) // New animation in list - create new instance
                {
                    var newClip = UnityEngine.Object.Instantiate(old) as AnimationClip;
                    newClip.name = old.name;
                    AssetDatabase.AddObjectToAsset(newClip, animController);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip));
                    oldToNew[old] = newClip;
                    Debug.Log("Nested animation clip: " + newClip.name);
                }

                state.state.motion = oldToNew[old];
            }
        }

        // Destroy all old AnimationClips in asset
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] is AnimationClip)
            {
                UnityEngine.Object.DestroyImmediate(objects[i], true);
            }
        }
        AssetDatabase.SaveAssets();
    }
}
