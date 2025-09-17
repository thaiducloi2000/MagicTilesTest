using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace nano.vs2
{
    public static class VSStyle
    {
        public static GUIContent lockIconContent = EditorGUIUtility.TrIconContent("LockIcon", "Unlock");
        public static GUIContent lockOnIconContent = EditorGUIUtility.TrIconContent("LockIcon-On", "Lock");

        public static GUIContent playContent = EditorGUIUtility.TrIconContent("Animation.Play", "Play Review.");
        public static GUIContent recordContent = EditorGUIUtility.TrIconContent("Animation.Record", "Record state change.");

        public static GUIContent propertySettingContent = EditorGUIUtility.TrIconContent("_Popup");

        public static Color TREE_LINE_HEIGHT = new Color(0f, 0f, 0f, 1);
        public static Color TREE_HEADER_BG = new Color(0.5f, 0.5f, 0.5f, 1);
        public static Color TREE_ELEMENT_BG = new Color(0f, 0f, 0f, .1f);
        public static Color TREE_ELEMENT_BG1 = new Color(0f, 0f, 0f, .05f);
        public static Color PARITI_HEADER_PROGRESS_COLOR = Color.blue;
        public static Color EVEN_HEADER_PROGRESS_COLOR = Color.green;

        public static Color SELECTED_STATE_COLUMN_COLOR = new Color(1f, 0.75f, 0f, 1);


        public const float ADD_STATE_BUTTON_WIDTH = 30;
        public const float PROPERTY_ELEMENT_SETTING_SIZE = 30;
        public const float PROPERTY_CURRENT_VALUE_WIDTH = 50;

        public const float PROPERTY_MIN_SIZE = 30;
        public const float PROPERTY_SIZE = 300;
        public const float PROPERTY_MAX_SIZE = 1000;
        public const float PROPERTY_ONE_LINE_OBJECT_WIDTH = 100;

        public const float INDENT_SPACE = 10;
        //PrefabOverlayAdded Icon     winbtn_graph_max_h   d_CollabCreate Icon
        public static GUIContent addStateContent = EditorGUIUtility.TrIconContent("d_CollabCreate Icon", "Add event.");

        public static GUIContent duplicateTargetButtonContent = EditorGUIUtility.TrIconContent("FilterSelectedOnly", "Select");

        public static GUIContent AnimateContent = new GUIContent("Animate");

    }
    public class Setting
    {
        public static GroupType groupType = GroupType.FullStruct;
        public const string PLAY_BUTTON_TOOLTIP = "Anim time must be set";
        public const float TREE_ROW_MARGIN = 4;

        public static Color DuplicatedPropertyColor = new Color(1f, .5f, 0, 1);

        internal static float GetPropertyWidth(float r)
        {
            return r / 3;
        }
    }

}



//  public static GUIContent playContent = EditorGUIUtility.TrIconContent("Animation.Play", "Play the animation clip.");
//         public static GUIContent recordContent = EditorGUIUtility.TrIconContent("Animation.Record", "Enable/disable keyframe recording mode.");
//         public static GUIContent previewContent = EditorGUIUtility.TrTextContent("Preview", "Enable/disable scene preview mode.");
//         public static GUIContent prevKeyContent = EditorGUIUtility.TrIconContent("Animation.PrevKey", "Go to previous keyframe.");
//         public static GUIContent nextKeyContent = EditorGUIUtility.TrIconContent("Animation.NextKey", "Go to next keyframe.");
//         public static GUIContent firstKeyContent = EditorGUIUtility.TrIconContent("Animation.FirstKey", "Go to the beginning of the animation clip.");
//         public static GUIContent lastKeyContent = EditorGUIUtility.TrIconContent("Animation.LastKey", "Go to the end of the animation clip.");
//         public static GUIContent addKeyframeContent = EditorGUIUtility.TrIconContent("Animation.AddKeyframe", "Add keyframe.");
//         public static GUIContent addEventContent = EditorGUIUtility.TrIconContent("Animation.AddEvent", "Add event.");
//         public static GUIContent sequencerLinkContent = EditorGUIUtility.TrIconContent("Animation.SequencerLink", "Animation Window is linked to Sequence Editor.  Press to Unlink.");

//         public static GUIContent noAnimatableObjectSelectedText = EditorGUIUtility.TrTextContent("No animatable object selected.");
//         public static GUIContent formatIsMissing = EditorGUIUtility.TrTextContent("To begin animating {0}, create {1}.");
//         public static GUIContent animatorAndAnimationClip = EditorGUIUtility.TrTextContent("an Animator and an Animation Clip");
//         public static GUIContent animationClip = EditorGUIUtility.TrTextContent("an Animation Clip");
//         public static GUIContent create = EditorGUIUtility.TrTextContent("Create");
//         public static GUIContent dopesheet = EditorGUIUtility.TrTextContent("Dopesheet");
//         public static GUIContent curves = EditorGUIUtility.TrTextContent("Curves");
//         public static GUIContent samples = EditorGUIUtility.TrTextContent("Samples");
//         public static GUIContent createNewClip = EditorGUIUtility.TrTextContent("Create New Clip...");

//         public static GUIContent animatorOptimizedText = EditorGUIUtility.TrTextContent("Editing and playback of animations on optimized game object hierarchy is not supported.\nPlease select a game object that does not have 'Optimize Game Objects' applied.");

//         public static GUIStyle playHead = "AnimationPlayHead";

//         public static GUIStyle curveEditorBackground = "CurveEditorBackground";
//         public static GUIStyle curveEditorLabelTickmarks = "CurveEditorLabelTickmarks";
//         public static GUIStyle eventBackground = "AnimationEventBackground";
//         public static GUIStyle eventTooltip = "AnimationEventTooltip";
//         public static GUIStyle eventTooltipArrow = "AnimationEventTooltipArrow";
//         public static GUIStyle keyframeBackground = "AnimationKeyframeBackground";
//         public static GUIStyle timelineTick = "AnimationTimelineTick";
//         public static GUIStyle dopeSheetKeyframe = "Dopesheetkeyframe";
//         public static GUIStyle dopeSheetBackground = "DopesheetBackground";
//         public static GUIStyle popupCurveDropdown = "PopupCurveDropdown";
//         public static GUIStyle popupCurveEditorBackground = "PopupCurveEditorBackground";
//         public static GUIStyle popupCurveEditorSwatch = "PopupCurveEditorSwatch";
//         public static GUIStyle popupCurveSwatchBackground = "PopupCurveSwatchBackground";

//         public static GUIStyle miniToolbar = EditorStyles.toolbar;
//         public static GUIStyle miniToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
//         public static GUIStyle toolbarLabel = new GUIStyle(EditorStyles.toolbarPopup);
