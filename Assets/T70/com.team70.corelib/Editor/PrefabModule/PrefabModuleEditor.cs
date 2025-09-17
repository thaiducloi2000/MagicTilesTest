using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace com.team70
{
	[CustomEditor(typeof(PrefabModule))]
	public class PrefabModuleEditor : Editor
	{
		private static bool drawDefault;
		[NonSerialized] public PrefabModule pm;
		[NonSerialized] public List<RLDrawer> drawers = new List<RLDrawer>();

		public override void OnInspectorGUI()
		{
			drawDefault = GUILayout.Toggle(drawDefault, " Dev");
			if (drawDefault) DrawDefaultInspector();

			if (pm != target) // change target?
			{
				pmScript = null;
				pmSearched = false;
			}

			pm = (PrefabModule)target;
			if (pm == null) return;

			// 
			// TODO: 
			//	- collapsible (persistent)
			//	- validation (duplicate / wrong component types / remove nulls)
			//  - drag / drop support
			//

			DrawScript();

			var arr = pm.lstElement;

			var config = pm.config;
			if (config != null)
			{
				var h = config.Count * 20f;
				var rect = GUILayoutUtility.GetRect(0, Screen.width, h, h);
				rect.height = 18f; //2px : space
				DrawData(config, rect);
			}

			if (arr == null || arr.Count == 0) return;

			for (var i = 0; i < arr.Count; i++)
			{
				if (drawers.Count <= i) drawers.Add(null);
				var d = drawers[i];

				// check for invalid drawer --> create new
				if (d == null || d.drawer == null || (d.drawer.list != arr[i].component))
				{
					d = new RLDrawer(arr[i], pm);
					drawers[i] = d;
				}

				d.DrawLayout();
			}
			
			GUILayout.BeginVertical();
			{
				if (GUILayout.Button("Collect Type"))
				{
					var collectType = pm.gameObject.GetComponent<CollectTypePrefabModule>();
					if (collectType == null) collectType = pm.gameObject.AddComponent<CollectTypePrefabModule>();
					collectType.DebugList();
				}
				
				if (GUILayout.Button("Get List Element Name"))
				{
					pm.DebugGetAllElement();
				}
			}
			GUILayout.EndVertical();
		}

		void DrawData(List<PrefabModule.ConfigKV> config, Rect rect)
		{
			for (var i = 0; i < config.Count; i++)
			{
				config[i].DrawInspector(pm, rect);
				rect.y += 20f;
			}
		}

		[NonSerialized] public MonoScript pmScript;
		public bool pmSearched;
		void DrawScript()
		{
			if (pm.script == null) return;

			var fullTypeName = pm.script.classType.fullTypeName;
			if (string.IsNullOrEmpty(fullTypeName)) return;
			
			// Automatically convert PrefabModule type 
			if (fullTypeName == "PrefabModule")
			{
				fullTypeName = "com.team70.PrefabModule";
				pm.script.classType.fullTypeName = fullTypeName;
			}
			
			if (pmSearched == false)
			{
				pmScript = null;
				pmSearched = true;
				pm.script.classType.RefreshCacheType();

				Type cacheType = pm.script.classType.cacheType;
				if (cacheType == null) return;

				var cacheTypeName = cacheType.Name.ToLower();
				var list = AssetDatabase.FindAssets($"{cacheTypeName} t:monoscript");
				if (drawDefault)
				{
					Debug.Log($"FindScript : {cacheTypeName} t:monoscript --> {list.Length}");	
				}
				
				if (list.Length <= 0) return;

				for (var i = 0; i < list.Length; i++)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(list[i]);
					pmScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
					// if (drawDefault) Debug.Log("Checking: " + pmScript.name.ToLower() + " : " + cacheTypeName);
					if (pmScript.name.ToLower() == cacheTypeName) break;
					pmScript = null;
				}
			}

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.ObjectField("Logic", pmScript, typeof(MonoScript), false);
				if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60f)))
				{
					pmScript = null;
					pmSearched = false;
					Repaint();
				}
			}
			GUILayout.EndHorizontal();

		}
	}
}
