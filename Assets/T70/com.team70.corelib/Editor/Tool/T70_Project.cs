using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.SceneManagement;

namespace com.team70
{

    public class T70_Project : EditorWindow
    {
        static private T70_Project _window;
        static Texture2D iconScene;
        static Texture2D iconPlay;
        static GUIStyle productNameStyle;
        static GUIStyle projectStyle;
        static string projectPath;

        const int MAX_SCENE = 5;
        

        [MenuItem("T70/Panel/Project")]
        private static void ShowWindow()
        {
            if (_window != null) return;

            _window = CreateInstance<T70_Project>();
            _window.titleContent = new GUIContent(EditorUserBuildSettings.activeBuildTarget.ToString());
            _window.Show();
        }
        
        void Init()
        {
            iconScene = AssetPreview.GetMiniTypeThumbnail(typeof(SceneAsset));
            iconPlay = EditorGUIUtility.FindTexture("PlayButton");

			productNameStyle = new GUIStyle(EditorStyles.largeLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 32
			};

			projectStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};

			projectPath = Application.dataPath;
            var arr = projectPath.Split('/').ToList();
            arr.RemoveAt(arr.Count - 1);

            if (arr.Count > 3)
            {
                arr.RemoveRange(0, arr.Count - 3);
            }

            projectPath = string.Join("/", arr);
        }

		[MenuItem("T70/Dev/Play 1st scene #1", false, 90)] static void Play1stScene() { PlayScene(0); }

		[MenuItem("T70/Dev/Play 2nd scene #2", false, 90)]
		static void Play2ndScene()
		{
			EditorSceneManager.OpenScene(EditorBuildSettings.scenes[5].path, OpenSceneMode.Single);
		}
		[MenuItem("T70/Dev/Test Resolution #;", false, 90)] static void NextResolution()
		{ 
			vIndex = (vIndex+1) % resolutions.Length;
			var v = resolutions[vIndex];
			T70U_GameView.currentGameViewSize = new Vector2(v.y, v.x);
		}

		static void PlayScene(int n)
		{
			var scenes = EditorBuildSettings.scenes;
			var counter = 0;

			for (int i = 0;i < scenes.Length; i++)
			{
				var scene = scenes[i];
				if (!scene.enabled) continue;

				if (counter == n)
				{
					var res = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
					if (res == true)
					{
						EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
						EditorApplication.isPlaying = true;
						return;
					}
				}

				counter++;
			}
		}



        void DrawProjectInfo()
        {
            GUILayout.Label(PlayerSettings.productName, productNameStyle);

            if (GUILayout.Button(projectPath, EditorStyles.toolbarButton))
            {
                EditorUtility.RevealInFinder(Application.dataPath);
            }
        }

        void DrawListScenes()
        {
            var listScenes = EditorBuildSettings.scenes;
            var n = Mathf.Min(listScenes.Length, listScenes.Length);

            GUILayout.BeginVertical();
            {
                for (int i = 0; i < n; i++)
                {
                    var scene = listScenes[i];
                    if (!scene.enabled) continue;

                    GUILayout.BeginHorizontal();
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);

                        if (GUILayout.Button(iconScene, EditorStyles.toolbarButton, GUILayout.Width(25f)))
                        {
	                        var res = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
	                        if (res == true)
	                        {
		                        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
	                        }
                        }

                        EditorGUILayout.ObjectField(asset, typeof(SceneAsset), false);
                        
                        if (GUILayout.Button(iconPlay, EditorStyles.toolbarButton, GUILayout.Width(25f)))
                        {
                            var res = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            if (res == true)
                            {
	                            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
	                            EditorApplication.isPlaying = true;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
        

        
        static int vIndex;
		static readonly Vector2[] resolutions = new Vector2[] 
		{
			new Vector2(2688, 1242),
			new Vector2(2436, 1125),
			new Vector2(2208, 1242),
			new Vector2(1334, 750 ),
			new Vector2(1136, 640 ),
			new Vector2(960,  640 ),
			new Vector2(2732, 2048),
			new Vector2(2388, 1668),
			new Vector2(2224, 1668),
			new Vector2(2048, 1536),
			new Vector2(1024, 768 ),
			new Vector2(1920, 1080),
			new Vector2(1920, 1200),
			new Vector2(2560, 1800)
		};

        public void OnGUI()
        {
            if (productNameStyle == null) Init();

            DrawProjectInfo();
            EditorGUILayout.Space();
            DrawListScenes();

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Resolution");
				
				var nIndex = EditorGUILayout.IntSlider(vIndex, 0, resolutions.Length-1);
				if (nIndex != vIndex)
				{
					vIndex = nIndex;
					var v = resolutions[vIndex];
					T70U_GameView.currentGameViewSize = new Vector2(v.y, v.x);
				}
			}
			EditorGUILayout.EndHorizontal();
        }
    }
}