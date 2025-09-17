using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

#if ENABLE_CINEMACHINE
using Cinemachine;
#endif

namespace com.team70
{

    public class T70_ToolBox : EditorWindow
    {
        static private T70_ToolBox _window;


        [MenuItem("T70/Panel/Toolbox")]
        private static void ShowWindow()
        {
            if (_window == null) _window = CreateInstance<T70_ToolBox>();
            _window.name = "T70:ToolBox";
            _window.Show();
        }

        Camera targetCamera;
        Transform targetTransform;

        
        public void OnGUI()
        {
            GUILayout.Label("Camera <-> Scene");
            if (targetCamera == null) targetCamera = GetCamera();
            targetCamera = (Camera)EditorGUILayout.ObjectField(targetCamera, typeof(Camera), true);

            if (targetCamera != null)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("C"))
                    {
                        h2_Camera.CaptureSceneView(targetCamera);
                    }

                    if (GUILayout.Button("L"))
                    {
                        h2_Camera.LookThrough(targetCamera);
                    }
                }
                GUILayout.EndHorizontal();
            }


            GUILayout.Label("Transform -> Scene");
            targetTransform = (Transform)EditorGUILayout.ObjectField(targetTransform, typeof(Transform), true);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Transform"))
                {
                    var so = Selection.activeGameObject;
                    if (so == null) return;

                    var sc = h2_Camera.sceneCamera;
                    Undo.RecordObject(so.transform, "Scene -> Transform");
                    so.transform.position = sc.transform.position;
                    so.transform.rotation = sc.transform.rotation;
                }

                if (GUILayout.Button("All"))
                {
                    var so = Selection.activeGameObject;
                    if (so == null) return;

                    var sc = h2_Camera.sceneCamera;

                    Undo.RecordObject(so.transform, "Scene -> Transform");
                    so.transform.position = sc.transform.position;
                    so.transform.rotation = sc.transform.rotation;

#if ENABLE_CINEMACHINE
                var vc = so.GetComponent<CinemachineVirtualCamera>();
                if (vc != null)
                {
                    
                    var ls = vc.m_Lens;
                    ls = LensSettings.FromCamera(sc);
                    vc.m_Lens = ls;
                }
#endif
                }
            }
            GUILayout.EndHorizontal();
        }

        public static Camera GetCamera()
        {
            var go = Selection.activeGameObject;
            if (go == null) return Camera.main;

            var c = go.GetComponent<Camera>();
            if (c == null) c = Camera.main;
            return c;
        }

        public static int SecondsTS
        {
            get { return (int)(DateTime.Now - new DateTime(2010, 1, 1)).TotalSeconds; }
        }
    }



    internal class h2_Camera
    {
        private static SceneView _sceneView;
        //camera being looked through & its saved information
        private static Camera lt_camera;
        public static bool lt_orthor;
        public static Vector3 lt_mPosition;
        public static Quaternion lt_mRotation;

        public static SceneView sceneView
        {
            get
            {
                if (_sceneView != null) return _sceneView;
                //if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof (SceneView)) return _sceneView = (SceneView) EditorWindow.focusedWindow;
                return _sceneView = SceneView.lastActiveSceneView ?? (SceneView)SceneView.sceneViews[0];
            }
        }

        public static Camera sceneCamera
        {
            get { return sceneView.camera; }
        }

        private static Vector3 m_Position
        {
            get { return GetAnimT<Vector3>("m_Position"); }
            set { SetAnimT("m_Position", FixNaN(value)); }
        }

        private static Quaternion m_Rotation
        {
            get { return GetAnimT<Quaternion>("m_Rotation"); }
            set { SetAnimT("m_Rotation", value); }
        }

        private static float cameraDistance
        {
            get { return (float)h2_Reflection.GetProperty(sceneView, "cameraDistance"); }
        }

        private static bool orthographic
        {
            get
            {
                var sv = sceneView;
                if (sv == null) return false;

                var svc = sv.camera;
                if (svc == null) return false;

                return svc.orthographic;
            }
            set
            {
                SetAnimT("m_Ortho", value);
            }
        }

        public static void cmdLookThrough()
        {
            if (Selection.activeGameObject == null) return;
            var cam = Selection.activeGameObject.GetComponent<Camera>();
            if (cam == null) return;
            LookThrough(cam);
        }

        public static void cmdCaptureSV()
        {
            if (Selection.activeGameObject == null) return;
            var cam = Selection.activeGameObject.GetComponent<Camera>();
            if (cam == null) return;
            CaptureSceneView(cam);
        }

        private static void Refresh()
        {
            EditorApplication.RepaintProjectWindow();
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            if (view != null) view.Repaint();
            return;

            // //hacky way to force SceneView increase drawing frame
            // var t = Selection.activeTransform
            //         ?? (Camera.main != null ? Camera.main.transform : new GameObject("$t3mp$").transform);

            // var op = t.position;
            // t.position += new Vector3(1, 1, 1); //make some dirty
            // t.position = op;

            // if (t.name == "$t3mp$") UnityEngine.Object.DestroyImmediate(t.gameObject, true);
        }

        public static void CopyTo(Camera c)
        {
            c.CopyFrom(sceneCamera);
        }

        public static void CopyFrom(Camera cam)
        {
            sceneCamera.CopyFrom(cam);
            sceneCamera.fieldOfView = cam.fieldOfView;

            orthographic = cam.orthographic;
            m_Rotation = cam.transform.rotation;
            m_Position = cam.transform.position - cam.transform.rotation * new Vector3(0, 0, -cameraDistance);
            Refresh();
        }


        private static T GetAnimT<T>(string name)
        {
            if (sceneView == null) return default(T);

            var animT = h2_Reflection.GetField(sceneView, name);

            return (T)h2_Reflection.GetProperty(animT, "target");
        }

        private static void SetAnimT<T>(string name, T value)
        {
            if (sceneView == null) return;

            var animT = h2_Reflection.GetField(sceneView, name);
            h2_Reflection.SetProperty(animT, "target", value);
        }

        private static Vector3 FixNaN(Vector3 v)
        {
            if (float.IsNaN(v.x) || float.IsInfinity(v.x)) v.x = 0;
            if (float.IsNaN(v.y) || float.IsInfinity(v.y)) v.y = 0;
            if (float.IsNaN(v.z) || float.IsInfinity(v.z)) v.z = 0;
            return v;
        }

        /*private float m_Size {
                get { return GetAnimT<float>("m_Size"); }
                set { SetAnimT("m_Size", (float.IsInfinity(value) || (float.IsNaN(value)) || value == 0f) ? 100f : value); }
            }*/


        public static void CopyCamera(Camera dest, Camera src)
        {
            if ((dest == null) || (src == null)) return;

            dest.transform.position = src.transform.position;
            dest.transform.rotation = src.transform.rotation;
            dest.fieldOfView = src.fieldOfView;
            dest.orthographicSize = src.orthographicSize;

            //dest.isOrthoGraphic = sceneCamera.isOrthoGraphic;
            dest.orthographic = src.orthographic;
        }

        public static void CaptureSceneView(Camera cam)
        {
            if (cam == null) return;

            //Debug.Log(cam);

            lt_camera = null;
            Undo.RecordObject(cam, "Capture SceneView");
            Undo.RecordObject(cam.transform, "Capture SceneView");
            CopyCamera(cam, sceneCamera);
        }

        public static void LookThrough(Camera cam)
        {
            if (lt_camera == null)
            {
                //save current state of scene-camera
                lt_orthor = orthographic;
                lt_mPosition = m_Position;
                lt_mRotation = m_Rotation;
            }

            if (cam != lt_camera)
            {
                CopyFrom(cam);
                lt_camera = cam;

                if (Application.isPlaying)
                {
                    EditorApplication.update -= UpdateLookThrough;
                    EditorApplication.update += UpdateLookThrough;
                }
            }
            else if (lt_camera != null)
            {
                //cancel look through & restore old state of scene-camera
                orthographic = lt_orthor;
                m_Position = lt_mPosition;
                m_Rotation = lt_mRotation;
                lt_camera = null;
            }
        }

        private static void UpdateLookThrough()
        {
            if ((lt_camera == null) || !EditorApplication.isPlaying)
            {
                lt_camera = null;
                EditorApplication.update -= UpdateLookThrough;
                return;
            }

            if (EditorApplication.isPaused) return;

            Debug.Log("Update LookThrough: " + lt_camera);

            if ((lt_camera.transform.position != sceneCamera.transform.position) ||
                (lt_camera.transform.rotation != sceneCamera.transform.rotation) ||
                (lt_camera.orthographic != sceneCamera.orthographic))
            {
                CopyFrom(lt_camera);
            }
            else
            {
                lt_camera = null;
            }

        }
    }

    public class h2_Reflection
    {
        private const BindingFlags AllFlags = BindingFlags.Default //| BindingFlags.ExactBinding
                                              | BindingFlags.FlattenHierarchy //| BindingFlags.DeclaredOnly
                                                                              //| BindingFlags.CreateInstance
                                                                              //| BindingFlags.GetField
                                                                              //| BindingFlags.GetProperty
                                                                              //| BindingFlags.IgnoreCase
                                                                              //| BindingFlags.IgnoreReturn
                                                                              //| BindingFlags.SuppressChangeType
                                                                              //| BindingFlags.InvokeMethod
                                              | BindingFlags.NonPublic | BindingFlags.Public
                                              //| BindingFlags.OptionalParamBinding
                                              //| BindingFlags.PutDispProperty
                                              //| BindingFlags.PutRefDispProperty
                                              //| BindingFlags.SetField
                                              //| BindingFlags.SetProperty
                                              | BindingFlags.Instance | BindingFlags.Static;

        public static bool HasMethod(object obj, string methodName, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(methodName)) return false;
            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            return type.GetMethod(methodName, flags ?? AllFlags) != null;
        }

        public static object Invoke(object obj, string methodName, Type type = null, BindingFlags? flags = null,
            params object[] parameters)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var f = type.GetMethod(methodName, flags ?? AllFlags);

            if (f != null) return f.Invoke(obj, parameters);
            Debug.LogWarning(string.Format("Invoke Error : <{0}> is not a method of type <{1}>", methodName, type));
            return null;
        }

        public static object Invoke(object obj, string methodName, Type type, Type[] typeParams,
            BindingFlags? flags = null, params object[] parameters)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var f = type.GetMethod(methodName, flags ?? AllFlags, null, CallingConventions.Standard, typeParams, null);

            if (f != null) return f.Invoke(obj, parameters);
            Debug.LogWarning(string.Format("Invoke Error : <{0}> is not a method of type <{1}>", methodName, type));
            return null;
            /*
            ArgumentException: failed to convert parameters
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:192)
System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MethodBase.cs:115)
vietlabs.h2.h2_Reflection.Invoke (System.Object obj, System.String methodName, System.Type type, Nullable`1 flags, System.Object[] parameters) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Reflection.cs:41)
vietlabs.h2.h2_Camera.SetAnimT[Single] (System.String name, Single value) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:368)
vietlabs.h2.h2_Camera.set_orthographic (Boolean value) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:294)
vietlabs.h2.h2_Camera.CopyFrom (UnityEngine.Camera cam) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:338)
vietlabs.h2.h2_Camera.LookThrough (UnityEngine.Camera cam) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:416)
vietlabs.h2.h2_Common.RunCommand (System.String cmd) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/features/h2_Common.cs:56)
vietlabs.h2.h2_MatchData.Trigger (System.Collections.Generic.List`1 matchList) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:201)
vietlabs.h2.h2_MatchData.Check (UnityEngine.Event e, System.Collections.Generic.Dictionary`2 hndMap) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:169)
vietlabs.h2.h2_Shortcut.Check () (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/core/h2_Shortcut.cs:84)
vietlabs.h2.Hierarchy2.HierarchyItemCB (Int32 instID, Rect r) (at Assets/Plugins/Editor/Hierarchy2/Editor/Scripts/h2/Hierarchy2.cs:222)
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:222)
Rethrow as TargetInvocationException: Exception has been thrown by the target of an invocation.
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:232)
System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MethodBase.cs:115)
System.Delegate.DynamicInvokeImpl (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/Delegate.cs:443)
System.MulticastDelegate.DynamicInvokeImpl (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/MulticastDelegate.cs:71)
System.Delegate.DynamicInvoke (System.Object[] args) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System/Delegate.cs:415)
FavoritesTab+TreeViewTracker.HierarchyItemOnGuiCallback (Int32 item, Rect selectionRect) (at Assets/FlipbookGames/FavoritesTab/Editor/Scripts/FavoritesTab.cs:1754)
UnityEditor.SceneHierarchyWindow.OnGUIAssetCallback (Int32 instanceID, Rect rect) (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:230)
UnityEditor.TreeView.OnGUI (Rect rect, Int32 keyboardControlID) (at C:/buildslave/unity/build/Editor/Mono/GUI/TreeView/TreeView.cs:404)
UnityEditor.SceneHierarchyWindow.DoTreeView (Single searchPathHeight) (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:334)
UnityEditor.SceneHierarchyWindow.OnGUI () (at C:/buildslave/unity/build/Editor/Mono/SceneHierarchyWindow.cs:178)
System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Reflection/MonoMethod.cs:222)
            
            */
        }

        public static Type GetTypeByName(string typeName)
        {
            var asmList = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < asmList.Length; i++)
            {
                var result = asmList[i].GetType(typeName);
                if (result != null) return result;
            }
            return null;
        }

        public static T ChangeType<T>(object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static bool HasField(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return false;
            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            return type.GetField(name, flags ?? AllFlags) != null;
        }

        public static object GetField(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return false;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var field = type.GetField(name, flags ?? AllFlags);
            if (field == null)
            {
                Debug.LogWarning(
                    string.Format("GetField Error : <{0}> does not contains a field with name <{1}>", type, name));
                return null;
            }

            return field.GetValue(obj);
        }

        public static void SetField(object obj, string name, object value, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var field = type.GetField(name, flags ?? AllFlags);

            if (field == null)
            {
                Debug.LogWarning(
                    string.Format("SetField Error : <{0}> does not contains a field with name <{1}>", type, name));
                return;
            }

            field.SetValue(obj, value);
        }

        public static bool HasProperty(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return false;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            return type.GetProperty(name, flags ?? AllFlags) != null;
        }

        public static void SetProperty(object obj, string name, object value, Type type = null,
            BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var property = type.GetProperty(name, flags ?? AllFlags);

            if (property == null)
            {
                Debug.LogWarning(
                    string.Format("SetProperty Error : <{0}> does not contains a property with name <{1}>", obj, name));
                return;
            }

            property.SetValue(obj, value, null);
        }

        public static object GetProperty(object obj, string name, Type type = null, BindingFlags? flags = null)
        {
            if ((obj == null) || string.IsNullOrEmpty(name)) return null;

            if (type == null) type = obj is Type ? (Type)obj : obj.GetType();
            var property = type.GetProperty(name, flags ?? AllFlags);
            if (property != null) return property.GetValue(obj, null);

            Debug.LogWarning(
                string.Format("GetProperty Error : <{0}> does not contains a property with name <{1}>", type, name));
            return null;
        }
    }
}