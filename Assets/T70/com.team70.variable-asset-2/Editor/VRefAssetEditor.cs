#define VREF_USE_VASSET

#if VREF_USE_VASSET
// COMPONENTS - 3D
using nano.va2;
using UnityEditor;


[CustomEditor(typeof(VRef_Transform))] public class VRef_TransformEditor : VAssetEditor<VRef_Transform> {}
[CustomEditor(typeof(VRef_Animator))] public class VRef_AnimatorEditor : VAssetEditor<VRef_Animator> {}
[CustomEditor(typeof(VRef_AudioSource))] public class VRef_AudioSourceEditor : VAssetEditor<VRef_AudioSource> {}
[CustomEditor(typeof(VRef_Camera))] public class VRef_CameraEditor : VAssetEditor<VRef_Camera> {}
[CustomEditor(typeof(VRef_Collider))] public class VRef_ColliderEditor : VAssetEditor<VRef_Collider> {}
[CustomEditor(typeof(VRef_Light))] public class VRef_LightEditor : VAssetEditor<VRef_Light> {}
[CustomEditor(typeof(VRef_MonoBehaviour))] public class VRef_MonoBehaviourEditor : VAssetEditor<VRef_MonoBehaviour> {}
[CustomEditor(typeof(VRef_Rigidbody))] public class VRef_RigidbodyEditor : VAssetEditor<VRef_Rigidbody> {}


// COMPONENTS - UGUI (2D)
[CustomEditor(typeof(VRef_Image))] public class VRef_ImageEditor : VAssetEditor<VRef_Image> {}
[CustomEditor(typeof(VRef_RectTransform))] public class VRef_RectTransformEditor : VAssetEditor<VRef_RectTransform> {}
[CustomEditor(typeof(VRef_Text))] public class VRef_TextEditor : VAssetEditor<VRef_Text> {}

    
// ASSETS
[CustomEditor(typeof(VRef_GameObject))] public class VRef_GameObjectEditor : VAssetEditor<VRef_GameObject> {}
[CustomEditor(typeof(VRef_AnimationClip))] public class VRef_AnimationClipEditor : VAssetEditor<VRef_AnimationClip> {}
[CustomEditor(typeof(VRef_AudioClip))] public class VRef_AudioClipEditor : VAssetEditor<VRef_AudioClip> {}
[CustomEditor(typeof(VRef_AnimatorController))] public class VRef_AnimatorControllerEditor : VAssetEditor<VRef_AnimatorController> {}
[CustomEditor(typeof(VRef_TextAsset))] public class VRef_TextAssetEditor : VAssetEditor<VRef_TextAsset> {}
#endif