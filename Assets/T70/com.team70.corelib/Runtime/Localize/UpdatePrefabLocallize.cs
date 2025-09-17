using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable] public class LocInfo
{
	public string guid;
	public string name;
	public List<InstInfo> infos = new List<InstInfo>();
}

[Serializable] public class InstInfo
{
	public string path;
	public string json;
}

public class UpdatePrefabLocallize : MonoBehaviour
{
	#if UNITY_EDITOR
	public TextAsset data;
	public List<LocInfo> infos;


	[ContextMenu("Do ABC")]
	public void DoABC()
	{
		var data1 = data.text.Split(new string[]{"--- "}, StringSplitOptions.RemoveEmptyEntries);

		infos = new List<LocInfo>();

		for (int i = 0; i< data1.Length; i++)
		{
			Debug.Log(data1[i]);

			var arr = data1[i].Split(new string[]{"|"}, StringSplitOptions.RemoveEmptyEntries);

			var loc = new LocInfo()
			{ 
				guid = arr[0].Trim(), name = arr[1].Trim()
			};
			infos.Add(loc);


			Debug.Log(arr[0]);
			Debug.Log(arr[1]);
			Debug.Log(arr[2]);
			
			var data2 = arr[2].Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < data2.Length; j++)
			{
				Debug.Log(data2[j]);

				var data3 = data2[j].Split(new string[]{"#####"}, StringSplitOptions.RemoveEmptyEntries);
				
				loc.infos.Add(new InstInfo()
				{
					path = data3[0],
					json = data3[1]
				});
			}
		}
	}
	
	static InstInfo info2;


	[ContextMenu("Apply Prefabs")]
	public void ApplyPrefabs()
	{
		

		for (int j =0;j < infos.Count;j ++)
		{
			var info = infos[j];

			var assetPath = AssetDatabase.GUIDToAssetPath(info.guid);
			if (string.IsNullOrEmpty(assetPath))
			{
				Debug.LogWarning("Missing prefab: " + info.guid + " | " + info.name);
				continue;
			}

			AssetDatabase.OpenAsset
			(
				AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object))
			);

			// find gameobject in scene
			var list = info.infos;
			for (var i = 0; i< list.Count; i++)
			{
				var item = list[i];

				var go = Find(item.path);
				if (go == null)
				{
					Debug.LogWarning("GameObject not found: " + item.path);	
					break;
				}

				var c = go.GetComponent<LocalizeV2_Text>();
				if (c == null) c = go.AddComponent<LocalizeV2_Text>();
				JsonUtility.FromJsonOverwrite(item.json, c);
				c.target = go.GetComponent<Text>();
				
				EditorUtility.SetDirty(go);
				EditorUtility.SetDirty(c);
				EditorUtility.SetDirty(prefabRoot);
				AssetDatabase.SaveAssets();
			}
		}
	}
	
	public GameObject prefabRoot 
	{
		get 
		{
			return UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
		}
	}
	
	GameObject Find(string path)
	{
		var root = prefabRoot;
		var arr = path.Split('/').ToList();
		arr.RemoveAt(0);
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
	#endif
}
