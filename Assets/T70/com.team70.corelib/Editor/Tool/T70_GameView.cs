using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using GVInfo = T70U_GameView.GVInfo;

public class T70_GameView : EditorWindow
{
    [Serializable] public class GVInfo2 : GVInfo
    {
        public string info;
        public GVGroup group;
    }

    [Flags] public enum GVGroup 
    {
        iPhone = 1,
        iPad = 2,
        AndroidPhone = 4,
        AndroidTablet = 8,
        Others = 16
    }

    public bool isPortrait = true;
    public GVInfo2 standard;
    public GVGroup group = GVGroup.iPhone | GVGroup.iPad | GVGroup.AndroidPhone | GVGroup.AndroidTablet;
    public List<GVInfo2> listInfos;

    void SetDefault()
    {
        standard = new GVInfo2(){ width = 1920, height = 1080, name = "Standard" };

        listInfos = new List<GVInfo2>()
        {
            new GVInfo2(){ width = 2688, height = 1242, name = "iPhone XS Max | XR", group = GVGroup.iPhone, info = "6.5 inch (iPhone XS Max, iPhone XR)" }
            , new GVInfo2(){ width = 2436, height = 1125, name = "iPhone X | XS", group = GVGroup.iPhone, info = "5.8 inch (iPhone X, iPhone XS)" }
            , new GVInfo2(){ width = 2208, height = 1242, name = "iPhone 8+", group = GVGroup.iPhone, info = "5.5 inch (iPhone 6s Plus, iPhone 7 Plus, iPhone 8 Plus)" }
            , new GVInfo2(){ width = 1334, height = 750 , name = "iPhone 8", group = GVGroup.iPhone, info = "4.7 inch (iPhone 6, iPhone 6s, iPhone 7, iPhone 8)" }
            , new GVInfo2(){ width = 1136, height = 640 , name = "iPhone SE", group = GVGroup.iPhone, info = "4 inch (iPhone SE)" }
            , new GVInfo2(){ width = 960,  height = 640 , name = "iPhone 4S", group = GVGroup.iPhone, info = "3.5 inch (iPhone 4s)" }

            , new GVInfo2(){ width = 2732, height = 2048, name = "iPad Pro 2 | 3", group = GVGroup.iPad, info = "12.9 inch (iPad Pro (2nd & 3rd generation))" }
            , new GVInfo2(){ width = 2388, height = 1668, name = "iPad Pro", group = GVGroup.iPad, info = "11 inch (iPad Pro)" }
            , new GVInfo2(){ width = 2224, height = 1668, name = "iPad Air", group = GVGroup.iPad, info = "10.5 inch (iPad Pro, iPad Air)" }
            , new GVInfo2(){ width = 2048, height = 1536, name = "iPad ", group = GVGroup.iPad, info = "9.7 inch (iPad, iPad mini)" }
            , new GVInfo2(){ width = 1024, height = 768 , name = "iPad Mini", group = GVGroup.iPad, info = "9.7 inch (iPad, iPad mini)" }

            , new GVInfo2(){ width = 1920, height = 1080, name = "Android Phone", group = GVGroup.AndroidPhone, info = "Android Phone" }
            , new GVInfo2(){ width = 1920, height = 1200, name = "Tablet 7\"", group = GVGroup.AndroidTablet, info = "Android Tablet 7\"" }
            , new GVInfo2(){ width = 2560, height = 1800, name = "Tablet 10\"", group = GVGroup.AndroidTablet, info = "Android Tablet 10\"" }
        };  

        EditorUtility.SetDirty(this);
    }
    
    public void OnGUI()
    {
        var so = new SerializedObject(this);
        so.Update();

        if (listInfos == null || listInfos.Count == 0)
        {
            SetDefault();
        }

        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.PropertyField(so.FindProperty("isPortrait"));
            EditorGUILayout.PropertyField(so.FindProperty("standard"));
            EditorGUILayout.PropertyField(so.FindProperty("group"));
            EditorGUILayout.PropertyField(so.FindProperty("listInfos"));
        }

        if (EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
        }
        
        if (GUILayout.Button("Apply"))
        {
            var list = new List<GVInfo>();
            for (int i = 0;i < listInfos.Count; i++)
            {
                var item = listInfos[i];
                var isEnable = (item.group & group) > 0;
                if (!isEnable) continue;

                list.Add(new GVInfo()
                {
                    name = item.name,
                    width = isPortrait ? item.height : item.width,
                    height = isPortrait ? item.width: item.height
                });
            }
            
            T70U_GameView.Set
            (
                new List<GVInfo>(){ new GVInfo()
                {
                    name = "Standard",
                    width = isPortrait ? standard.height : standard.width,
                    height = isPortrait ? standard.width: standard.height
                }} , list
            );
        }
    }
    
    static T70_GameView _window;

    [MenuItem("T70/Panel/GameView")]
    private static void ShowWindow()
    {
        if (_window == null) _window = CreateInstance<T70_GameView>();
        _window.titleContent = new GUIContent("T70-GameView");
        _window.Show();
    }
}