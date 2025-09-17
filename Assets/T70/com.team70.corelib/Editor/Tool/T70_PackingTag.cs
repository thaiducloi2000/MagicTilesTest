using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class T70_PackingTag : EditorWindow
{
    public class PackingTagInfo
    {
        public string tag;
        public List<TextureInfo> infos = new List<TextureInfo>();

        public bool isExpanded;
        public Vector2 contentSize = Vector2.zero;

		public void DoSelection()
		{
			var list = new List<UnityEngine.Object>();
			for (int i = 0; i < infos.Count; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(infos[i].path);
				list.Add(asset);
			}

			Selection.objects = list.ToArray();
		}

        public void DrawGUI(Rect rect)
        {
            var h = 18f;
            var maxW = 0f;
            GUILayout.BeginVertical(GUILayout.Height(infos.Count * h));
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        var r = new Rect(rect.x + 20f, rect.y + h * (i + 1), h, h);
                        var info = infos[i];
                        if (info.icon == null)
                        {
                            info.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(info.path);
                        }

                        GUI.DrawTexture(r, info.icon, ScaleMode.ScaleToFit);

                        var w = EditorStyles.label.CalcSize(new GUIContent(infos[i].path)).x;
                        r.x += h + 4f;
                        r.width += w;
                        GUI.Label(r, infos[i].path);

                        maxW = Mathf.Max(w, maxW);

                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            var touchRect = new Rect(rect.x + 20f, r.y, r.width, r.height);
                            if (touchRect.Contains(Event.current.mousePosition))
                            {
                                EditorGUIUtility.PingObject(info.icon);	
                                Event.current.Use();
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            contentSize.x = maxW;
            contentSize.y = infos.Count * h;
        }
    }

    public class TextureInfo
    {
        public string guid;
        public string path;
        public Texture2D icon;
    }
    static T70_PackingTag _window;


    [MenuItem("T70/Panel/PackingTag")]
    private static void ShowWindow()
    {
        if (_window == null) _window = CreateInstance<T70_PackingTag>();
        _window.titleContent = new GUIContent("T70-PackingTag");
        _window.Show();
    }
    
    // [MenuItem("Window/VietLabs/PackingTag Scan")]
    // static void Init()
    // {
    //     // Get existing open window or if none, make a new one:
    //     T70_PackingTag window = (T70_PackingTag)EditorWindow.GetWindow(typeof(T70_PackingTag));
    //     window.Show();
    // }
    
    [NonSerialized] private static List<PackingTagInfo> cache = new List<PackingTagInfo>();

    static void Scan()
    {
        var allPaths = AssetDatabase.FindAssets("t:texture2d");
        var dict = new Dictionary<string, PackingTagInfo>();
        cache.Clear();

        for (int i = 0; i < allPaths.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(allPaths[i]);
            if (!path.StartsWith("Assets/")) continue;
            if (path.Contains("/Editor/")) continue;
            
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            var tag = ti.spritePackingTag;

            PackingTagInfo tagInfo;

            if (false == dict.TryGetValue(tag, out tagInfo))
            {
                tagInfo = new PackingTagInfo()
                {
                    tag = tag
                };
                dict.Add(tag, tagInfo);
            }

            tagInfo.infos.Add(new TextureInfo()
            {
                guid = allPaths[i],
                path = path
            });
        }

        cache.AddRange(dict.Values);
		cache.Sort
		(
			(item1, item2) => { return item1.tag.CompareTo(item2.tag); }
		);
    }

	Vector2 scrollPosition;

    void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            Scan();
            Debug.Log("Refresh: " + cache.Count);
        }
        if (cache.Count == 0) return;

        var h = 18f;
        var totalH = cache.Count * h;
        for (int ii = 0; ii < cache.Count; ii++)
        {
            if (cache[ii].isExpanded)
            {
                totalH += cache[ii].contentSize.y;
            }
        }

        var rect = new Rect(0, 0, position.width - 15f, totalH);
        var viewRect = new Rect(0, 30f, position.width, position.height - 30f);
        scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, rect);
        {
            for (int i = 0; i < cache.Count; i++)
            {
                var item = cache[i];

                var addH = 0f;
                for (int ii = 0; ii < i; ii++)
                {
                    if (cache[ii].isExpanded)
                    {
                        addH += cache[ii].contentSize.y;
                    }
                }

				var oldExpand = item.isExpanded;
                var text = string.IsNullOrEmpty(item.tag) ? "(no tag)" : item.tag;
                var w = EditorStyles.label.CalcSize(new GUIContent(text)).x;

                var newExpand = EditorGUI.Foldout(new Rect(4f, i * h + addH, w, h), oldExpand, text);

                if (oldExpand != newExpand)
                {
					item.isExpanded = newExpand;
					if (newExpand) // DO SELECT
					{
						item.DoSelection();
					}
				}

				if (newExpand)
				{
                    item.DrawGUI(new Rect(0, i * h + addH, 0, 0));
                }
                
            }
        }
        GUI.EndScrollView();
    }
}
