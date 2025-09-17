using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class T70U_GameView
{
    [Serializable] public class GVInfo 
    {
        public string name;
        public int width;
        public int height;
    }

    [Serializable] public class GVProjectInfo 
    {
        public List<GVInfo> builtins;
        public List<GVInfo> customs;
    }


    
    static object gameViewSizesInstance;
    static MethodInfo getGroup;
    static PropertyInfo getCurrentGroup;
    static PropertyInfo selectedSizeIndexProp;
    static MethodInfo addCustomSize;
    static ConstructorInfo newViewSize;

    static Type GameViewType;

    static MethodInfo getSizeOfMainGameView;
	static MethodInfo snapZoom;
	static MethodInfo EnforceZoomAreaConstraints;
	static PropertyInfo getMinscaleOfMainGameView;
    
    static T70U_GameView()
    {
        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        
        var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        var gvst = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");

        GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        getSizeOfMainGameView = GameViewType.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
        selectedSizeIndexProp = GameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		getMinscaleOfMainGameView = GameViewType.GetProperty("minScale", BindingFlags.Instance | BindingFlags.NonPublic);
		snapZoom = GameViewType.GetMethod("SnapZoom", BindingFlags.Instance | BindingFlags.NonPublic);
		EnforceZoomAreaConstraints = GameViewType.GetMethod("EnforceZoomAreaConstraints", BindingFlags.Instance | BindingFlags.NonPublic);

        newViewSize = gvsType.GetConstructor(new Type[] {gvst, typeof(int), typeof(int), typeof(string) });
        gameViewSizesInstance = instanceProp.GetValue(null, null);

        getGroup = sizesType.GetMethod("GetGroup");
        getCurrentGroup = gameViewSizesInstance.GetType().GetProperty("currentGroup");
        addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");

        EditorApplication.update -= Load;
        EditorApplication.update += Load;
    }
    
    static void Load()
    {
        EditorApplication.update -= Load;
        if (!File.Exists("Library/T70/GVPInfo.json")) return;
        var pInfo = JsonUtility.FromJson<GVProjectInfo>(File.ReadAllText("Library/T70/GVPInfo.json"));
        Set(pInfo.builtins, pInfo.customs);
    }

    static void Save(GVProjectInfo pInfo)
    {
        if (!Directory.Exists("Library/T70/")) Directory.CreateDirectory("Library/T70/");
        File.WriteAllText("Library/T70/GVPInfo.json", JsonUtility.ToJson(pInfo));
    }

    static void Changed()
    {
        gameViewSizesInstance.GetType().GetMethod("Changed").Invoke(gameViewSizesInstance, new object[]{});
    }

    internal static object GetCurrentGroup()
    {
        return getCurrentGroup.GetValue(gameViewSizesInstance);
    }
    
    static public void Set(List<GVInfo> builtins, List<GVInfo> customs)
    {
        var group = GetCurrentGroup();
        var listBuiltIn = (IList)(group.GetType().GetField("m_Builtin", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(group));
        var listCustom = (IList)(group.GetType().GetField("m_Custom", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(group));

        listBuiltIn.Clear();
        listCustom.Clear();

        // Add free aspect
        listBuiltIn.Add(newViewSize.Invoke(new object[] { 0, 0, 0, "Free Aspect"}));
        
        for (int i =0; i < builtins.Count; i ++)
        {
            var item = builtins[i];
            listBuiltIn.Add(newViewSize.Invoke(new object[] { 1, item.width, item.height, item.name}));
        }
        
        for (int i =0; i < customs.Count; i ++)
        {
            var item = customs[i];
            listCustom.Add(newViewSize.Invoke(new object[] { 1, item.width, item.height, item.name}));
        }
        
        EditorUtility.SetDirty((UnityEngine.Object)gameViewSizesInstance);
        
        Save(new GVProjectInfo()
        {
            builtins = builtins,
            customs = customs
        });

        Changed();
    }
    
    static void AddCustomSize(int w, int h, string name)
    {
        var group = getCurrentGroup.GetValue(gameViewSizesInstance);
        var newSize = newViewSize.Invoke(new object[] { 1, w, h, name});
        addCustomSize.Invoke(group, new object[] { newSize });
    }

    static void AddBuiltinSize(int w, int h, string name)
    {
        var group = getCurrentGroup.GetValue(gameViewSizesInstance);
        var newSize = newViewSize.Invoke(new object[] { 1, w, h, name});
        addCustomSize.Invoke(group, new object[] { newSize });
    }

    static void ReplaceSizeAt(int index, int w, int h, string name)
    {
        var group = GetCurrentGroup();
        var listBuiltIn = (IList)(group.GetType().GetField("m_Builtin", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(group));
        var listCustom = (IList)(group.GetType().GetField("m_Custom", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(group));

        if (index < listBuiltIn.Count)
        {
            listBuiltIn[index] = newViewSize.Invoke(new object[] { 1, w, h, name});
            Changed();
            return;
        }
        
        index -= listBuiltIn.Count;
        if (index < listCustom.Count) 
        {
            listCustom[index] = newViewSize.Invoke(new object[] { 1, w, h, name});
            Changed();
            return;
        }
    }
    
    public static int IndexOfName(string text)
    {
        var group = getCurrentGroup.GetValue(gameViewSizesInstance);
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
        for(int i = 0; i < displayTexts.Length; i++)
        {
            string display = displayTexts[i];
            if (display.Contains(text)) return i;
        }
        return -1;
    }

	public static float minScale
	{
		get
		{
			return (float)getMinscaleOfMainGameView.GetValue(EditorWindow.GetWindow(GameViewType));
		}
	}

    public static Vector2 currentGameViewSize
    {
        get
        {
            System.Object Res = getSizeOfMainGameView.Invoke(null,null);
            return (Vector2)Res;
        }

        set
        {
            var idx = IndexOfName("Dynamic");
            if (idx == -1)
            {
                AddCustomSize((int)value.x, (int)value.y, "Dynamic");
                idx = IndexOfName("Dynamic");
                if (idx == -1)
                {
                    Debug.LogWarning("Add Dynamic failed!");
                    return;
                }
            }
            else
            {
                ReplaceSizeAt(idx, (int) value.x, (int)value.y, "Dynamic");
            }

			var window = EditorWindow.GetWindow(GameViewType);
            selectedSizeIndexProp.SetValue(window, idx, null);

			snapZoom.Invoke(window, new object[]{ lastScale });
			EnforceZoomAreaConstraints.Invoke(window, new object[]{});
			
			refreshCounter = 5; //half a sec?
			EditorApplication.update -= RefreshMinScale;
			EditorApplication.update += RefreshMinScale;
        }
    }

	static int refreshCounter;
	static float lastScale = 1f;
	
	static void RefreshMinScale()
	{
		if (refreshCounter--<=0) EditorApplication.update -= RefreshMinScale;

		var window = EditorWindow.GetWindow(GameViewType);
		snapZoom.Invoke(window, new object[]{ minScale });
		lastScale = minScale;
	}
}