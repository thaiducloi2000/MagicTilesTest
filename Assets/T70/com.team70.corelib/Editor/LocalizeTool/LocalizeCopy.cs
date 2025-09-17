using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

public class LocalizeCopy
{
    [MenuItem("KTools/LocalizeV2-Restore text")]
	public static void RestoreText()
	{
		var text = File.ReadAllText("Assets/localize_project.json");
		var projectInfo = JsonUtility.FromJson<ProjectInfo>(text);

		foreach (var prefabInfo in projectInfo.prefabs)
		{
			var path = prefabInfo.path;
			var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			
			// Open in Edit mode
			AssetDatabase.OpenAsset(obj);
			var root = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

			foreach (var textInfo in prefabInfo.texts)
			{
				var c = FindGO(root, textInfo.fullPath);
				if (c == null)
				{
					Debug.LogWarning("Not found! , FullPath: " + textInfo.fullPath);
					continue;
				}

				var lt = c.GetComponent<LocalizeV2_Text>();
				if (lt == null) lt = c.AddComponent<LocalizeV2_Text>();
				
				lt.target = lt.GetComponent<Text>();
				lt.locID = textInfo.locID;
				lt.defaultText = textInfo.defaultText;
				lt.toUpper = textInfo.toUpper;
				
				EditorUtility.SetDirty(lt);
				EditorUtility.SetDirty(c);
				EditorUtility.SetDirty(root);
				AssetDatabase.SaveAssets();
			}
		}
	}
	
	public static IEnumerable<GameObject> GetAllChild(GameObject target, bool returnMe = false)
	{
		if (returnMe)
		{
			yield return target;
		}

		if (target.transform.childCount > 0)
		{
			for (var i = 0; i < target.transform.childCount; i++)
			{
				yield return target.transform.GetChild(i).gameObject;
				foreach (GameObject item in GetAllChild(target.transform.GetChild(i).gameObject, false))
				{
					yield return item;
				}
			}
		}
	}
	
	static GameObject FindGO(GameObject prefabRoot,string path)
	{
		var root = prefabRoot;
		var arr = path.Split('/').ToList();
		arr.RemoveAt(0);

		var childName = string.Join("/", arr.ToArray()).Trim();
		var child = root.transform.Find(childName);
		if (child == null)
		{
			Debug.LogWarning("Not found: " + path + " --> " + childName);
			return null;
		}

		return child.gameObject;
	}
	
	[MenuItem("KTools/LocalizeV2-Scan Prefabs 2")]
	public static void ScanPrefabs2()
	{
		var allPaths = AssetDatabase.GetAllAssetPaths();
		var counter = 0;

		var projectInfo = new ProjectInfo();

		for (var i = 0; i < allPaths.Length; i++)
		{
			var path = allPaths[i];
			if (!path.EndsWith(".prefab")) continue;

			var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			
			var prefabInfo = new PrefabInfo()
			{
				path = path,
				guid = AssetDatabase.AssetPathToGUID(path)
			};
			
			// Open in Edit mode
			AssetDatabase.OpenAsset(obj);
			var root = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
			var success = false;
			
			foreach (var child in GetAllChild(root))
			{
				var  c = child.GetComponent<LocalizeV2_Text>();
				if (c == null) continue;
				prefabInfo.texts.Add
				(
					TextInfo.From(c)
				);

				success = true;
			}

			if (success)
			{
				projectInfo.prefabs.Add(prefabInfo);
				// if (projectInfo.prefabs.Count > 5) break;
			}

			if (counter++ % 20 != 0) continue;
			
			Resources.UnloadUnusedAssets();
			EditorUtility.UnloadUnusedAssetsImmediate();
		}

		File.WriteAllText("Assets/localize_project.json", JsonUtility.ToJson(projectInfo, false));
	}

	public static string GetHierarchyPath(Transform t, string suffix)
	{
		if (t.parent == null) return suffix;
		var name2 = string.IsNullOrEmpty(suffix) ? t.name : (t.name + "/" + suffix);
		return GetHierarchyPath(t.parent, name2);
	}
	
	
	[Serializable] public class TextInfo
	{
		public string fullPath;
		
		public string locID;
		public string defaultText;
		public bool toUpper;

		public static TextInfo From(LocalizeV2_Text text)
		{
			var fullPath = GetHierarchyPath(text.transform, string.Empty);
			return new TextInfo()
			{
				fullPath = fullPath,
				locID = text.locID,
				defaultText = text.defaultText,
				toUpper = text.toUpper
			};
		}
	}
	
	[Serializable] public class PrefabInfo
	{
		public string guid;
		public string path;
		
		public List<TextInfo> texts = new List<TextInfo>();
	}

	[Serializable] public class ProjectInfo
	{
		public List<PrefabInfo> prefabs = new List<PrefabInfo>();
	}
}
