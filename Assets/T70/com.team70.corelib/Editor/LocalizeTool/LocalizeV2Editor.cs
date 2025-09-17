using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Text.RegularExpressions;
using System.Globalization;
using System;
using UnityEngine.UI;

[CustomEditor(typeof(LocalizeV2))]
public class LocalizeV2Editor : Editor
{
	[NonSerialized] LocalizeV2 v2;
	[NonSerialized] List<LocalizeV2.Element> filtered;

	[NonSerialized] SearchUI2 searchUI;
	[NonSerialized] TreeUI2.GroupDrawer drawer;

	public bool drawDefault;

	public override void OnInspectorGUI()
	{
		v2 = (LocalizeV2)target;
		if (v2 == null) return;

		drawDefault = GUILayout.Toggle(drawDefault, "Default Inspector");
		if (drawDefault) DrawDefaultInspector();

		if (drawer == null)
		{
			drawer = new TreeUI2.GroupDrawer(DrawGroup, DrawItem);
			searchUI = new SearchUI2(OnChange);
			OnChange(null, false);
			LocalizeV2.EditModeInit(true);
		}

		drawer.elementHeight = v2.langs.Count * 18;

		searchUI.Draw();
		drawer.Draw();
		drawExportUI();
		
		GUILayout.BeginHorizontal();
		{
			//drawTranslateUI();
			drawCharacterUsageUI();
		}
		GUILayout.EndHorizontal();

		drawClearUI();


		Repaint();
	}

	static string fileName = "locallizev2.tsv";

	public void drawExportUI()
	{
		if (v2 == null)
		{
			Debug.LogWarning("Something wrong - v2 is null!");
			return;
		}

		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Clear All"))
			{
				v2.ClearAll();
				EditorUtility.SetDirty(v2);
			};
			
			if (GUILayout.Button("Export .TSV"))
			{
				File.WriteAllText(fileName, v2.ToTSV());
				EditorUtility.SetDirty(v2);
			};

			if (GUILayout.Button("Import .TSV"))
			{
				v2.ImportTSV(File.ReadAllLines(fileName));
				EditorUtility.SetDirty(v2);
				RefreshUI();
			}

			if (GUILayout.Button("Import Clipboard"))
			{
				v2.ImportTSV(EditorGUIUtility.systemCopyBuffer.Split('\n'));
				EditorUtility.SetDirty(v2);
				RefreshUI();

				Debug.LogWarning("Import finished!");
			}
		}
		GUILayout.EndHorizontal();
	}

	public void drawClearUI()
	{
		var c = GUI.backgroundColor;
		GUI.backgroundColor = new Color32(0xff, 0x88, 0x88, 0xff);
		GUILayout.BeginHorizontal();
		{
			for (var i = 0; i < v2.langs.Count; i++)
			{
				if (GUILayout.Button(v2.langs[i]))
				{
					v2.ClearTranslation(i);
					RefreshUI();
					break;
				};
			}
		}
		GUILayout.EndHorizontal();
		GUI.backgroundColor = c;
	}


	//	public void drawTranslateUI()
	//	{
	//		if (GUILayout.Button("Translate"))
	//		{
	//			var arr = v2.langs;
	//			var langCacheArr = new List<vietlabs.l2.L2_LangCache>();
	//			
	//			for (var i = 0;i < arr.Count; i++)
	//			{
	//				var lang = arr[i];
	//				
	//				if (lang == "vn") lang = "vi";
	//				if (lang == "cn") lang = "zh-cn";
	//				if (lang == "tw") lang = "zh-tw";
	//				
	//				var lc = vietlabs.l2.L2_Cache.Api.GetLanguage(lang);
	//				langCacheArr.Add(lc);
	//			}
	//			
	//			var srcIndex = 0;
	//			
	//			for (var i = 0;i < v2.data.Count;i ++)
	//			{
	//				var element = v2.data[i];
	//				var v0 = element.GetValue(srcIndex);
	//				
	//				if (string.IsNullOrEmpty(v0)) continue;
	//				
	//				for (var j = 0; j < arr.Count; j++)
	//				{
	//					if (j == srcIndex) continue;
	//					string result = element.GetValue(j);
	//					if (!string.IsNullOrEmpty(result)) continue;
	//					
	//					if (!langCacheArr[j].TranslateFromCache(v0, out result, arr[srcIndex])) continue;
	//					element.SetValue(result, j);
	//				}
	//			}
	//			
	//		};
	//	}

	public void drawCharacterUsageUI()
	{
		if (GUILayout.Button("Characters"))
		{
			var arr = v2.langs;
			var dicts = new HashSet<char>[arr.Count];
			for (var i = 0; i < dicts.Length; i++)
			{
				dicts[i] = new HashSet<char>();
			}

			for (var i = 0; i < v2.data.Count; i++)
			{
				var element = v2.data[i];

				for (var j = 0; j < element.locValues.Count; j++)
				{
					var text = element.locValues[j];

					for (var k = 0; k < text.Length; k++)
					{
						if (!dicts[j].Contains(text[k])) dicts[j].Add(text[k]);
					}
				}
			}

			for (var i = 0; i < dicts.Length; i++)
			{
				var list = dicts[i].ToList();
				list.Sort();
				Debug.Log(arr[i] + ":" + new string(list.ToArray(), 0, list.Count));
			}
		};
	}

	public void OnChange(string term, bool sensitive)
	{
		if (filtered == null) filtered = new List<LocalizeV2.Element>();
		filtered.Clear();

		if (string.IsNullOrEmpty(term))
		{
			filtered.AddRange(v2.data);
		}
		else
		{
			for (var i = 0; i < v2.data.Count; i++)
			{
				var d = v2.data[i];
				d.searchScore = SearchUI2.StringMatch
				(
					term, sensitive,
					d.locID, d.locValues[0]
				);

				if (d.searchScore > 0)
				{
					filtered.Add(d);
				}
			}

			filtered.Sort((s1, s2) => s2.searchScore.CompareTo(s1.searchScore));
		}

		RefreshUI();
	}

	void RefreshUI()
	{
		drawer = new TreeUI2.GroupDrawer(DrawGroup, DrawItem);
		drawer.elementHeight = v2.langs.Count * 20 + 20;
		drawer.Reset(filtered, (e) => e.locID, e => string.Empty);//e=>e.GetGroup()

		//var arr = drawer.tree.rootItem.children;
		//var dh = 0;

		//for (var i = 0;i < arr.Count;i++)
		//{
		//	var h = arr[i].height;
		//	arr[i].height = 18;
		//	dh += h-18;
		//}
	}

	void DrawLangs()
	{
		var so = new SerializedObject(v2);
		var prop = so.FindProperty("langs");
		EditorGUILayout.PropertyField(prop, true);
		EditorGUILayout.Space();
	}

	void DrawGroup(Rect r, string label, int childCount)
	{
		GUI.Label(r, label + " (" + childCount + ")", EditorStyles.boldLabel);
	}

	static Color bgColor = new Color(0.5f, 1f, 0.5f);
	static string editLocID;

	void DrawItem(Rect r, string locID)
	{
		r.x -= 12f;
		r.width += 32f;
		r.y += 2f;

		const int w = 30;

		var isRepaint = Event.current.type == EventType.Repaint;
		var isEditable = editLocID == locID;

		var e = v2.iGetElement(locID);
		if (e == null) return;

		if (editLocID != locID)
		{
			if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
			{
				editLocID = locID;

				//Event.current.Use();

				GUI.SetNextControlName("");
				GUI.FocusControl("");
				EditorGUIUtility.systemCopyBuffer = locID;
			}

			if (!isRepaint) return;

			GUI.Toggle(new Rect(r.x, r.y, 18f, 18f), e.enable, string.Empty);
			GUI.Label(new Rect(r.x + w, r.y, r.width - w, 18f), e.locID, EditorStyles.boldLabel);

		}
		else
		{
			e.enable = GUI.Toggle(new Rect(r.x, r.y, 18f, 18f), e.enable, string.Empty);
			EditorGUI.TextField(new Rect(r.x + w, r.y, r.width - w, 18f), e.locID, EditorStyles.boldLabel);
		}

		r.height = 18f;
		r.y += 18f;

		EditorGUI.BeginDisabledGroup(!e.enable);
		{
			var r1 = new Rect(r.x, r.y, w, 16f);
			var r2 = new Rect(r.x + w, r.y, r.width - 50f, 16f);

			var n = v2.langs.Count;
			var c = GUI.backgroundColor;

			for (var i = 0; i < n; i++)
			{
				if (i == 0)//v2.GetLangIndex()
				{
					GUI.backgroundColor = bgColor;
				}

				GUI.Label(r1, v2.langs[i]);

				var v = e.GetValue(i);

				if (isEditable)
				{
					var v1 = EditorGUI.TextArea(r2, v);
					if (v1 != v)
					{
						e.SetValue(v1, i);
						EditorUtility.SetDirty(v2);
					}
				}
				else
				{
					GUI.Label(r2, v, EditorStyles.textField);
				}

				GUI.backgroundColor = c;

				r1.y += 18f;
				r2.y += 18f;
			}
		}
		EditorGUI.EndDisabledGroup();
	}

	[MenuItem("KTools/LocalizeV2-Scan Prefabs")]
	public static void ScanPrefabs()
	{
		var allPaths = AssetDatabase.GetAllAssetPaths();
		var counter = 0;

		for (var i = 0; i < allPaths.Length; i++)
		{
			if (!allPaths[i].EndsWith(".prefab")) continue;

			var obj = AssetDatabase.LoadAssetAtPath<GameObject>(allPaths[i]);
			var inst = GameObject.Instantiate(obj);
			ScanGameObject(inst, "PREFAB_" + obj.name.Replace(" ", string.Empty).Replace("_", string.Empty).ToUpper());

			if (counter++ > 20)
			{
				break;
			}

			Resources.UnloadUnusedAssets();
			EditorUtility.UnloadUnusedAssetsImmediate();
		}
	}

	internal static bool ShouldLocalized(string text)
	{
		if (text == null) return false;
		text = text.Trim();
		if (text == string.Empty) return false;

		if (LocalizeV2.ignoreStrings.Contains(text)) return false;

		if (int.TryParse(text, out _)) return false; //skip int
		if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)) return false; //hex string

		return !float.TryParse(text, out _);
	}

	internal static void ScanGameObject(GameObject go, string path) //For both Prefabs & Scene Objects
	{
		var lb2 = go.GetComponent<LocalizeV2_Text>();

		if (lb2 == null)
		{
			var uilabel = go.GetComponent<Text>();
			if (uilabel != null && ShouldLocalized(uilabel.text) && !LocalizeV2.ignoreIDs.Contains(path))
			{
				lb2 = go.AddComponent<LocalizeV2_Text>();
				lb2.target = uilabel;
				lb2.originalText = uilabel.text;
				lb2.locID = path;

				uilabel.text = string.Empty;
				//LocallizeV2.Instance.Set(lb2.locID, lb2.originalText, 0);
				Debug.Log(lb2.locID + ":" + lb2.originalText);
			}
		}
		else if (LocalizeV2.ignoreIDs.Contains(path))
		{
			Debug.Log("Destroyed:: " + lb2.locID + ":" + lb2.originalText);
			//DestroyImmediate(lb2);
		}

		foreach (Transform t in go.transform)
		{
			if (t == go.transform) continue;
			ScanGameObject(t.gameObject, path + "." + t.gameObject.name.Replace(" ", string.Empty).Replace("_", string.Empty).ToUpper());
		}
	}
}
