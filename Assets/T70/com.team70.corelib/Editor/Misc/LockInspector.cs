#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

static class LockInspector
{
    [MenuItem("T70/Tools/Toggle Inspector Lock (shortcut) %`")]
    static void ToggleWindowLock() // Inspector must be inspecting something to be locked
    {
        EditorWindow
            windowToBeLocked = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead

        if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "InspectorWindow")
        {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            PropertyInfo propertyInfo = type.GetProperty("isLocked");
            bool value = propertyInfo != null && (bool) propertyInfo.GetValue(windowToBeLocked, null);
            if (propertyInfo != null) propertyInfo.SetValue(windowToBeLocked, !value, null);
            windowToBeLocked.Repaint();
        }
        else if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "ProjectBrowser")
        {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.ProjectBrowser");
            PropertyInfo propertyInfo = type.GetProperty("isLocked",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            bool value = propertyInfo != null && (bool) propertyInfo.GetValue(windowToBeLocked, null);
            if (propertyInfo != null) propertyInfo.SetValue(windowToBeLocked, !value, null);
            windowToBeLocked.Repaint();
        }
        else if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "SceneHierarchyWindow")
        {
            Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.SceneHierarchyWindow");

            FieldInfo fieldInfo = type.GetField("m_SceneHierarchy",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                PropertyInfo propertyInfo = fieldInfo.FieldType.GetProperty("isLocked",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                object value = fieldInfo.GetValue(windowToBeLocked);
                bool value2 = propertyInfo != null && (bool) propertyInfo.GetValue(value);
                if (propertyInfo != null) propertyInfo.SetValue(value, !value2, null);
            }

            windowToBeLocked.Repaint();
        }
    }
}
#endif