using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using T70 = com.team70.T70;


public class T70_Screenshot : EditorWindow
{
    [Serializable] public class ResInfo
    {
        public string info;
        public bool enabled;

        public int w;
        public int h;

        public void Draw(bool isPortrait)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(16f);
                enabled = GUILayout.Toggle(enabled, GUIContent.none, GUILayout.Width(20f));
                if (isPortrait)
                {
                    h = EditorGUILayout.DelayedIntField(h, GUILayout.Width(60f));
                    w = EditorGUILayout.DelayedIntField(w, GUILayout.Width(60f));
                } else {
                    w = EditorGUILayout.DelayedIntField(w, GUILayout.Width(60f));
                    h = EditorGUILayout.DelayedIntField(h, GUILayout.Width(60f));
                }
                
                info = EditorGUILayout.TextField(info);
            }
            GUILayout.EndHorizontal();
        }
    }

    [Serializable] public class SSInfo
    {
        public string directory;
        public string fileName;
        public string fullPath;
        public int w;
        public int h;
        public int superSize;
    }

    [Serializable] public class SSSetting
    {
        public string prefixName;
        public bool resSuffix = true;
        public bool timeStampSuffix = true;
        public bool scaleSuffix = true;
        public bool isPortrait = true;
        
        public List<ResInfo> infos;
        public int scale;
        
        public int enableCount;

        public void ResetInfos()
        {
            infos = new List<ResInfo>()
            {
                new ResInfo(){ w = 2688, h = 1242, enabled = true, info = "6.5 inch (iPhone XS Max, iPhone XR)" }
                , new ResInfo(){ w = 2436, h = 1125, enabled = true, info = "5.8 inch (iPhone X, iPhone XS)" }
                , new ResInfo(){ w = 2208, h = 1242, enabled = true, info = "5.5 inch (iPhone 6s Plus, iPhone 7 Plus, iPhone 8 Plus)" }
                , new ResInfo(){ w = 1334, h = 750, enabled = true, info = "4.7 inch (iPhone 6, iPhone 6s, iPhone 7, iPhone 8)" }
                , new ResInfo(){ w = 1136, h = 640, enabled = true, info = "4 inch (iPhone SE)" }
                , new ResInfo(){ w = 960, h = 640, enabled = true, info = "3.5 inch (iPhone 4s)" }

                , new ResInfo(){ w = 2732, h = 2048, enabled = true, info = "12.9 inch (iPad Pro (2nd & 3rd generation))" }
                , new ResInfo(){ w = 2388, h = 1668, enabled = true, info = "11 inch (iPad Pro)" }
                , new ResInfo(){ w = 2224, h = 1668, enabled = true, info = "10.5 inch (iPad Pro, iPad Air)" }
                , new ResInfo(){ w = 2048, h = 1536, enabled = true, info = "9.7 inch (iPad, iPad mini)" }
                , new ResInfo(){ w = 1024, h = 768, enabled = true, info = "9.7 inch (iPad, iPad mini)" }

                , new ResInfo(){ w = 1920, h = 1080, enabled = true, info = "Android Phone" }
                , new ResInfo(){ w = 1920, h = 1200, enabled = true, info = "Android Tablet 7\"" }
                , new ResInfo(){ w = 1800, h = 2560, enabled = true, info = "Android Tablet 10\"" }
                , new ResInfo(){ w = 1024, h = 1024, enabled = false, info = "(Custom 1)" }
                , new ResInfo(){ w = 512, h = 512, enabled = false, info = "(Custom 2)" }
                , new ResInfo(){ w = 256, h = 256, enabled = false, info = "(Custom 3)" }
                , new ResInfo(){ w = 128, h = 128, enabled = false, info = "(Custom 4)" }
            };
        }

        public string GetFileName(int w, int h, int timeStamp)
        {
            return string.Format("{0}{1}{2}{3}.png", 
                string.IsNullOrEmpty(prefixName) ? string.Empty : (prefixName + "_"),
                timeStampSuffix ? timeStamp + "_" : string.Empty,
                resSuffix ? w +"x"+h : string.Empty,
                (scaleSuffix && scale > 1) ? "@" + scale : string.Empty
            );
        }

        public void OnDraw()
        {
            prefixName = EditorGUILayout.DelayedTextField("File Name", prefixName);
            EditorGUI.indentLevel++;
            {
                timeStampSuffix = EditorGUILayout.ToggleLeft("Time Stamp", timeStampSuffix);
                scaleSuffix = EditorGUILayout.ToggleLeft("Scale", scaleSuffix);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.HelpBox(GetFileName(1920, 1080, 329752509), MessageType.Info);
            isPortrait = EditorGUILayout.Toggle("Is Portrait", isPortrait);
            scale = EditorGUILayout.IntSlider("Up Scale", scale, 1, 8);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("None")) { Set((i, b) => { return false; }); }
                if (GUILayout.Button("Invert")) { Set((i, b) => { return !b; }); }
                if (GUILayout.Button("Odd")) { Set((i, b) => { return i%2 == 1 ? b : !b; }); }
                if (GUILayout.Button("Even")) { Set((i, b) => { return i%2 == 0 ? b : !b; }); }
            }
            GUILayout.EndHorizontal();
            
            enableCount = 0;
            for (int i =0 ;i < infos.Count; i++)
            {
                infos[i].Draw(isPortrait);
                if (infos[i].enabled) enableCount++;
            }
            
            if (enableCount > 0)
            {
                EditorGUILayout.HelpBox(string.Format("{0} Resolutions selected", enableCount), MessageType.Info);
            } else {
                EditorGUILayout.HelpBox("No Resolution selected", MessageType.Warning);
            }
        }

        void Set(Func<int, bool, bool> cb)
        {
            for (int i =0 ;i < infos.Count; i++)
            {
                infos[i].enabled = cb(i, infos[i].enabled);
            }
        }
    }
    
    public SSSetting settings;
    
    public void OnGUI()
    {
        OnDraw();
        Repaint();
    }
    
    public void OnDraw()
    {
        if (settings == null) 
        {
            settings = new SSSetting();
            settings.ResetInfos();
        }

        settings.OnDraw();

        EditorGUILayout.Space();

        if (queue.Count > 0)
        {
            var rect = GUILayoutUtility.GetRect(16f, Screen.width, 16f, 16f);
            EditorGUI.ProgressBar(rect, progress, "Progressing ... ");
            Repaint();
        }
        else
        {
            EditorGUI.BeginDisabledGroup(settings.enableCount == 0);
            {
                if (GUILayout.Button("Screenshot"))
                {
                    var ts = T70.SecondsTS;
                    var list = new List<SSInfo>();
                    var infos = settings.infos;

                    for (int i = 0; i < infos.Count; i++)
                    {
                        var item = infos[i];
                        if (!item.enabled) continue;

                        var ww = settings.isPortrait ? item.h : item.w;
                        var hh = settings.isPortrait ? item.w : item.h;

                        list.Add(new SSInfo()
                        {
                            directory = "../Screenshots/",
                            fileName = settings.GetFileName(ww, hh, ts),
                            w = ww,
                            h = hh,
                            superSize = settings.scale
                        });
                    }

                    SS_Start(list, false);
                }
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Open Folder", GUILayout.Width(100f)))
            {
                var path = new DirectoryInfo("../Screenshots");
                if (!path.Exists)
                {
                    Directory.CreateDirectory(path.FullName);
                }
                
                EditorUtility.RevealInFinder(path.FullName);
            }
        }
    }

    

    


    // STATIC UTILS: DO SCREENSHOT
    static internal List<SSInfo> queue = new List<SSInfo>();
    static internal int nDone;
    static internal int nFrame;
    static float progress { get { return nDone / (float)(nDone + queue.Count); } }

    public static void SS_Start(List<SSInfo> list, bool clearQueue)
    {
        if (clearQueue)
        {
            queue.Clear();
            nDone = 0;
        }

        queue.AddRange(list);
        EditorApplication.update -= SS_Process;
        EditorApplication.update += SS_Process;
    }

    
    private static Action delayAction;
    private static int delayCounter;
    
    private static void SS_DelayShot()
    {
        if (T70_Screenshot.delayCounter-- > 0)
        return;
        
        EditorApplication.update -= SS_DelayShot; 
        T70_Screenshot.delayAction();
    }

    public static void SS_Process()
    {
        if (nFrame++ % 5 != 0) return; // check once every 1/20 sec (5 frames)
        if (queue.Count == 0)
        {
            EditorApplication.update -= SS_Process;
            return;
        }

        EditorApplication.ExecuteMenuItem("Window/General/Game");
        var last = queue[queue.Count - 1];

        if (string.IsNullOrEmpty(last.fullPath)) // TODO : Capture now
        {
            if (!Directory.Exists(last.directory)) Directory.CreateDirectory(last.directory);
            last.fullPath = Path.Combine(last.directory, last.fileName);
            if (File.Exists(last.fullPath)) // existed: append secondTS
            {
                last.fullPath = Path.Combine(last.directory, last.fileName + "_" + T70.SecondsTS + ".png");
            }
            
            T70U_GameView.currentGameViewSize = new Vector2(last.w, last.h);
            T70_Screenshot.delayAction = (Action) (() => ScreenCapture.CaptureScreenshot(last.fullPath, last.superSize));
            T70_Screenshot.delayCounter = 2;

            EditorApplication.update -= SS_DelayShot;
            EditorApplication.update += SS_DelayShot;
            return;
        }

        if (!File.Exists(last.fullPath)) return; // screenshot not yet finished
        nDone++;
        queue.RemoveAt(queue.Count - 1);
    }

    public static void AddTestSize()
    {
        AddCustomSize(GameViewSizeGroupType.Standalone, 123, 456, "Test size");
    }

    public static void AddCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height, string text)
    {
        // goal:
        // var group = ScriptableSingleton<GameViewSizes>.instance.GetGroup(sizeGroupType);
        // group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);

        var asm = typeof(Editor).Assembly;
        var sizesType = asm.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        var getGroup = sizesType.GetMethod("GetGroup");
        var instance = instanceProp.GetValue(null, null);
        var group = getGroup.Invoke(instance, new object[] { (int)sizeGroupType });
        var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
        var gvsType = asm.GetType("UnityEditor.GameViewSize");
        var ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
        var newSize = ctor.Invoke(new object[] { 1, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
    }


    // Windows base
    static T70_Screenshot _window;

    [MenuItem("T70/Panel/Screenshot")]
    private static void ShowWindow()
    {
        if (_window == null) _window = CreateInstance<T70_Screenshot>();
        _window.titleContent = new GUIContent("T70-ScreenShot");
        _window.Show();
    }
}




