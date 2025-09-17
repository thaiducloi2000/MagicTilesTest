using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityObject = UnityEngine.Object;

namespace Team70
{
	[CreateAssetMenu(menuName = "T70/AutoBuild2")]
	public class AutoBuild2 : ScriptableObject
	{
		[Serializable] public class BuildRequestTS
		{
			public int processId;
			public string machineId;
			public string projectPath;
			public bool willRunSHPostBuild = true;
			
			public bool isDifferentSession
			{
				get
				{
					if (machineId != SystemInfo.deviceUniqueIdentifier)
					{
						//Debug.LogWarning("Different device");
						return true;
					}

					Process currentProcess = Process.GetCurrentProcess();
					if (processId != currentProcess.Id)
					{
						//Debug.LogWarning("Different process ID");
						return true; // Changed process id
					}

					if (projectPath != Application.dataPath)
					{
						//Debug.LogWarning("Different project path");
						return true; // Changed project
					}

					return false;
				}
			}

			public static BuildRequestTS Generate()
			{
				Process currentProcess = Process.GetCurrentProcess();

				return new BuildRequestTS()
				{
					machineId = SystemInfo.deviceUniqueIdentifier,
					processId = currentProcess.Id,
					projectPath = Application.dataPath
				};
			}
		}
		[Serializable] public class BuildInfo
		{
			public string projectId;
			public string productName;
			public string packageName;

			public Texture2D buildIcon;
			public BuildTarget target;

			public bool isAAB;
			public bool splitAPK = true;
			public bool overwrite = true;
			public string buildVersion;
			public int buildNumber;

			// Android options
			public bool enable;

			public bool isAndroid { get { return target == BuildTarget.Android; } }
			public bool isIOS { get { return target == BuildTarget.iOS; } }

			public void PreprocessBuild(string buildFolder)
			{
				AssetDatabase.SaveAssets();
				GC.Collect();
				Resources.UnloadUnusedAssets();
				EditorUtility.UnloadUnusedAssetsImmediate();

				PlayerSettings.bundleVersion = buildVersion;

				if (isAndroid)
				{
					PlayerSettings.Android.bundleVersionCode = buildNumber;
					PlayerSettings.Android.buildApkPerCpuArchitecture = splitAPK;
					AndroidArchitecture aac = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
					PlayerSettings.Android.targetArchitectures = aac;

					EditorUserBuildSettings.buildAppBundle = isAAB;
					EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
					EditorUserBuildSettings.androidCreateSymbolsZip = true;

					var listAPKNames = GetAPKNames(buildFolder);

					for (var i = 0; i < listAPKNames.Length; i++)
					{
						var apkName = listAPKNames[i];
						var tempName = apkName.Replace(buildName, Application.productName);

						if (File.Exists(tempName)) File.Delete(tempName); // always delete temp file no matter what
						if (overwrite && File.Exists(apkName)) File.Delete(apkName);
					}

					return;
				}

				if (isIOS)
				{
					PlayerSettings.iOS.buildNumber = buildNumber.ToString();
					if (!overwrite) return;

					var path = fullBuildPath(buildFolder);
					if (Directory.Exists(path)) Directory.Delete(path, true);
					return;
				}

				Debug.LogWarning("Unsupported platform: " + target);
			}

			public string[] GetAPKNames(string buildFolder)
			{
				var buildDir = new DirectoryInfo(buildFolder);
				var fullName = buildDir.FullName;

				if (isAAB)
					return new[]
					{
						Path.Combine(fullName, buildName + ".aab")
					};

				if (splitAPK)
					return new[]
					{
						Path.Combine(fullName, buildName + ".arm64-v8a.apk"), Path.Combine(fullName, buildName + ".armeabi-v7a.apk"),
					};

				return new[]
				{
					Path.Combine(fullName, buildName + ".apk")
				};
			}

			public void PostProcessBuild(string buildFolder)
			{
				if (target != BuildTarget.Android) return;
				if (isAAB) return;

				// var productName = Application.productName;
				var listAPKNames = GetAPKNames(buildFolder);

				for (int i = 0; i < listAPKNames.Length; i++)
				{
					var apkName = listAPKNames[i];
					var tempName = apkName.Replace(buildName, productName);

					if (!File.Exists(tempName)) continue;
					File.Move(tempName, apkName);
				}
			}

			public string buildName
			{
				get
				{
					return string.Format("{0}_v{1}_b{2}", projectId, buildVersion, buildNumber);
				}
			}

			public string fullBuildPath(string buildFolder)
			{
				if (buildFolder.EndsWith("/"))
				{
					// remove last slash
					buildFolder = buildFolder.Substring(0, buildFolder.Length - 1);
				}

				var buildDir = new DirectoryInfo(buildFolder);

				if (isAndroid)
				{
					if (isAAB) return Path.Combine(buildDir.FullName, buildName + ".aab");
					if (splitAPK) return buildDir.FullName;
					return Path.Combine(buildDir.FullName, buildName + ".apk");
				}

				return Path.Combine(buildDir.FullName, "XCode_" + buildName);
			}

			public BuildInfo Read()
			{
				buildVersion = PlayerSettings.bundleVersion;
				productName = PlayerSettings.productName;

				if (isAndroid)
				{
					buildNumber = PlayerSettings.Android.bundleVersionCode;
					packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
				}

				if (isIOS)
				{
					int.TryParse(PlayerSettings.iOS.buildNumber, out buildNumber);
					packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
				}

				return this;
			}

			public void Write()
			{
				PlayerSettings.bundleVersion = buildVersion;
				PlayerSettings.productName = productName;

				if (isAndroid)
				{
					PlayerSettings.Android.bundleVersionCode = buildNumber;
					PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, packageName);
				}

				if (isIOS)
				{
					PlayerSettings.iOS.buildNumber = buildNumber.ToString();
					PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, packageName);
				}

				EditorApplication.RepaintProjectWindow();
			}

			public void IncreaseVersion()
			{
				buildNumber++;
				if (string.IsNullOrEmpty(buildVersion)) buildVersion = "1.0.0";

				var arr = buildVersion.Split('.');
				var last = int.Parse(arr[2]);
				last++;

				arr[2] = last.ToString();
				buildVersion = string.Join(".", arr);
			}
		}

		[Serializable] public class KeystoreInfo
		{
			public string path = "../Info/team70.keystore";
			public string alias = "team70";
			public string keystorePassword = "123456789";
			public string aliasPassword = "123456789";

			public void Write()
			{
				// Must use absolute path or else gradle build fail
				var keystoreFI = new FileInfo(path);
				PlayerSettings.Android.keystoreName = keystoreFI.FullName;
				PlayerSettings.Android.keystorePass = keystorePassword;
				PlayerSettings.Android.keyaliasPass = aliasPassword;
				PlayerSettings.Android.keyaliasName = alias;
			}
		}

		const string DEFAULT_BUILD_FOLDER = "../../_Build";

		public KeystoreInfo keystore;
		public string buildFolder = DEFAULT_BUILD_FOLDER;
		public Texture2D icon;

		public List<BuildInfo> listBuild = new List<BuildInfo>();
		
		public List<UnityEngine.Object> listUnused;
		public BuildRequestTS buildRQ;
		public UnityEvent beforeBuild;
		public TextAsset shTemplate;
		public bool runGitPostBuild = true;
		
		[NonSerialized] private BuildInfo _buildInfo;

		[NonSerialized] private int _lockCount;
		public void Lock() { _lockCount++; }
		public void Unlock()
		{
			_lockCount--;
			if (_lockCount == 0 && _buildInfo != null) ExportBuild(_buildInfo);
		}

		public int nBuildPending
		{
			get
			{
				if (buildRQ != null && !buildRQ.isDifferentSession)
				{
					return listBuild.Count(t => t.enable);
				}

				StopBuild();
				return 0;
			}
		}

		public void StopBuild()
		{
			for (var i = 0; i < listBuild.Count; i++)
			{
				listBuild[i].enable = false;
			}
		}

		public void ProcessBuild()
		{
			var cTarget = EditorUserBuildSettings.activeBuildTarget;

			for (var i = 0; i < listBuild.Count; i++)
			{
				var item = listBuild[i];
				if (!item.enable) continue;

				if (item.target != cTarget) continue;

				item.enable = false; // already build
				Build(item);
				return; // only do one build at a time
			}
			
			// No build for current target --> will need to switch platform first!
			
			ExternalProcessUtils.GitResetRepo();
			
			for (var i = 0; i < listBuild.Count; i++)
			{
				var item = listBuild[i];
				if (!item.enable) continue;
				
				switch (item.target)
				{
					case BuildTarget.Android:
						EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
						return; // only do one build at a time

					case BuildTarget.iOS:
						EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
						return; // only do one build at a time
				}

			}
		}

		void RemoveUnusedAssets()
		{
			AssetDatabase.StartAssetEditing();
			for (var i = 0; i < listUnused.Count; i++)
			{
				if (listUnused[i] == null) continue;
				var path = AssetDatabase.GetAssetPath(listUnused[i]);
				AssetDatabase.DeleteAsset(path);
			}
			AssetDatabase.StopAssetEditing();
		}

		public void Build(BuildInfo info)
		{
			DryRun(info);

			if (_lockCount == 0)
			{
				ExportBuild(info);
				return;
			}

			_buildInfo = info;
		}

		public void EnableBuildForTarget(BuildTarget target, bool updateBuildNumber = true)
		{
			for (var i = 0; i < listBuild.Count; i++)
			{
				var b = listBuild[i];
				b.enable = (b.target == target);
				
				if (updateBuildNumber && b.target == target)
				{
					b.buildNumber++;
				}
			}
			
			EditorUtility.SetDirty(this);
		}

		public void DryRun(BuildInfo info)
		{
			if (string.IsNullOrEmpty(buildFolder))
			{
				buildFolder = DEFAULT_BUILD_FOLDER;
			}

			var buildDir = new DirectoryInfo(buildFolder);
			if (!buildDir.Exists) Directory.CreateDirectory(buildDir.FullName);

			// Check for existed build with the same name
			if (info.overwrite == false)
			{
				if (info.target == BuildTarget.Android)
				{
					var apkNames = info.GetAPKNames(buildFolder);
					for (var i = 0; i < apkNames.Length; i++)
					{
						var item = apkNames[i];
						if (!File.Exists(item)) continue;
						Debug.LogWarning("Exporting file existed! " + item);
						// return;
					}
				}
				else
				{
					var fullBuildPath = info.fullBuildPath(buildFolder);
					if (Directory.Exists(fullBuildPath))
					{
						Debug.LogWarning("Exporting target existed! " + fullBuildPath);
						EditorUtility.RevealInFinder(fullBuildPath);
						// return;
					}
				}
			}

			RemoveUnusedAssets();
			if (info.target == BuildTarget.Android)
			{
				keystore.Write();
			}

			info.Write();
			info.PreprocessBuild(buildFolder);

			// write customized icon, if any
			if (icon != null && info.buildIcon != null && info.buildIcon != icon)
			{
				var p1 = AssetDatabase.GetAssetPath(info.buildIcon);
				var p2 = AssetDatabase.GetAssetPath(icon);
				var bytes = File.ReadAllBytes(p1);
				File.WriteAllBytes(p2, bytes);

				// force refresh
				AssetDatabase.ImportAsset(p2, ImportAssetOptions.ForceSynchronousImport);
				RepaintAll();
			}

			_lockCount = 0;
			if (beforeBuild != null) beforeBuild.Invoke();
		}

		static void RepaintAll()
		{
			var windows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
			if (windows == null || windows.Length <= 0) return;

			for (var i = 0; i < windows.Length; i++)
			{
				var w = windows[i] as EditorWindow;
				if (w != null) w.Repaint();
			}
		}

		[ContextMenu("Run Postbuild")]
		private void RunPostBuild()
		{
			listBuild[0].buildNumber++;
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			
			RunPostBuildSH(listBuild[0]);
		}
		
		private void RunPostBuildSH(BuildInfo info)
		{
			if (shTemplate == null) return;

			var shPath = ExternalProcessUtils.FileFromTemplate(
				"git-post-build.sh", shTemplate.text,
				"{{TEMPLATE_PROJECT_PATH}}", Application.dataPath.Replace("/Assets", string.Empty),
				"{{TEMPLATE_TAG}}", $"{info.projectId}/v{info.buildVersion}_b{info.buildNumber}",
				"{{TEMPLATE_BRANCH}}", $"build/v{info.buildVersion}_b{info.buildNumber}",
				"{{TEMPLATE_BUILD_ASSET}}", $"{AssetDatabase.GetAssetPath(this)}"
			);
			
			// Run & Lock Unity until git finish
			ExternalProcessUtils.Run(shPath, true);
		}
		
		private void ExportBuild(BuildInfo info)
		{
			var fullBuildPath = info.fullBuildPath(buildFolder);
			BuildPipeline.BuildPlayer(GetScenes(), fullBuildPath, info.target, BuildOptions.None);
			info.PostProcessBuild(buildFolder);
			
			if (runGitPostBuild == true && buildRQ.willRunSHPostBuild) 
			{
				// Run once per build request
				buildRQ.willRunSHPostBuild = false;
				RunPostBuildSH(info);
			}
			
			if (nBuildPending > 0)
			{
				ProcessBuild();
				return;
			}
			
			EditorUtility.RevealInFinder(fullBuildPath);
		}

		internal static string[] GetScenes()
		{
			return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
		}

		internal static int SecondsTS
		{
			get { return (int)(DateTime.Now - new DateTime(2010, 1, 1)).TotalSeconds; }
		}

		internal static void DrawRect(Rect rect, Color c)
		{
			var bgColor = GUI.color;
			GUI.color = c;
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false);
			GUI.color = bgColor;
		}
	}


	[InitializeOnLoad]
	internal class AutoBuild2Helper
	{
		static AutoBuild2 build;
		static AutoBuild2Helper()
		{
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}

		static void Update()
		{
			if (EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				return;
			}

			if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				if (build != null) build.StopBuild();
				EditorApplication.update -= Update;
				return;
			}

			if (build == null)
			{
				string[] guids = AssetDatabase.FindAssets("t:" + nameof(AutoBuild2)); //FindAssets uses tags check documentation for more info
				if (guids.Length == 0)
				{
					EditorApplication.update -= Update;
					return;
				}

				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				build = AssetDatabase.LoadAssetAtPath<AutoBuild2>(path);
				if (build == null)
				{
					EditorApplication.update -= Update;
					return;
				}
			}

			EditorApplication.update -= Update;

			//check if timeStamp is valid
			if (build.nBuildPending == 0)
			{
				return;
			}

			build.ProcessBuild();
		}


		[MenuItem("T70/Build/Select Build Asset &#b")]
		public static void SelectBuild()
		{
			var arr = AssetDatabase.FindAssets("t:AutoBuild2");

			if (arr.Length == 0)
			{
				Debug.LogWarning("There are no Build asset in the project!");
				return;
			}

			// if (arr.Length > 1)
			// {
			//     Debug.LogWarning("There are [" + arr.Length + "] Build assets in the project!");
			// }

			var path = AssetDatabase.GUIDToAssetPath(arr[0]);
			var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
			Selection.activeObject = obj;
			EditorGUIUtility.PingObject(obj);
		}

		static AutoBuild2 FindAutoBuildAssets(string name = null, bool allowFallback = false)
		{
			var arr = AssetDatabase.FindAssets("t:AutoBuild2");
			if (arr.Length == 0)
			{
				Debug.LogWarning("There are no Build asset in the project!");
				return null;
			}

			for (var i = 0; i < arr.Length; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(arr[i]).Replace("\\", "/");
				if (string.IsNullOrEmpty(name))
				{
					return AssetDatabase.LoadAssetAtPath<AutoBuild2>(path);
				}

				var fileName = Path.GetFileName(path);
				if (fileName.ToLower().Contains(name.ToLower()))
				{
					return AssetDatabase.LoadAssetAtPath<AutoBuild2>(path);
				}
			}

			if (!allowFallback) return null;

			// Fall back to the first one
			var path0 = AssetDatabase.GUIDToAssetPath(arr[0]).Replace("\\", "/");
			return AssetDatabase.LoadAssetAtPath<AutoBuild2>(path0);
		}

		[MenuItem("T70/Build/Android")]
		public static void BuildAndroid()
		{
			var buildAsset = FindAutoBuildAssets("android", true);
			if (buildAsset == null) return;
			buildAsset.EnableBuildForTarget(BuildTarget.Android);
			
			buildAsset.buildRQ = AutoBuild2.BuildRequestTS.Generate();
			buildAsset.ProcessBuild();
		}

		[MenuItem("T70/Build/iOS")]
		public static void BuildIOS()
		{
			var buildAsset = FindAutoBuildAssets("ios", true);
			if (buildAsset == null) return;
			buildAsset.EnableBuildForTarget(BuildTarget.iOS);

			buildAsset.buildRQ = AutoBuild2.BuildRequestTS.Generate();
			buildAsset.ProcessBuild();
		}
	}

	[CustomEditor(typeof(AutoBuild2))]
	public class AutoBuild2Editor : Editor
	{
		public int bIndex;
		public string path;
		public bool drawDefault;

		public GUIStyle productNameStyle;
		public GUIStyle projectStyle;

		void Init()
		{
			productNameStyle = new GUIStyle(EditorStyles.largeLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 32
			};

			projectStyle = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};

			path = Application.dataPath;
			var arr = path.Split('/').ToList();
			arr.RemoveAt(arr.Count - 1);

			if (arr.Count > 3)
			{
				arr.RemoveRange(0, arr.Count - 3);
			}

			path = string.Join("/", arr);
		}

		string helpInfo;
		public override void OnInspectorGUI()
		{
			var ab = (AutoBuild2)target;
			if (ab == null) return;

			if (productNameStyle == null) Init();

			GUILayout.Label(PlayerSettings.productName, productNameStyle);

			var r0 = GUILayoutUtility.GetRect(0, Screen.width, 16f, 16f);
			r0.xMin = 0;
			r0.xMax += 16f;
			AutoBuild2.DrawRect(r0, new Color32(20, 20, 20, 255));

			if (Event.current.type == EventType.MouseUp && r0.Contains(Event.current.mousePosition))
			{
				var fullPath = new DirectoryInfo(ab.buildFolder);
				EditorUtility.RevealInFinder(fullPath.FullName);
				Event.current.Use();
			}

			GUI.Label(r0, path, projectStyle);
			EditorGUILayout.Space();

			if (string.IsNullOrEmpty(helpInfo))
			{
				helpInfo = $"\nPackageName:\t{PlayerSettings.applicationIdentifier}\nTargetPlatform:\t{EditorUserBuildSettings.activeBuildTarget}\n";
			}
			EditorGUILayout.HelpBox(helpInfo, MessageType.Info);
			var rect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				helpInfo = null;
				Repaint();
			}

			drawDefault = GUILayout.Toggle(drawDefault, "Draw default");
			if (drawDefault)
			{
				DrawDefaultInspector();
			}

			EditorGUILayout.Space();

			var max = ab.listBuild.Count;
			if (max == 0)
			{
				EditorGUILayout.HelpBox("No BuildInfo!", MessageType.Warning);

				if (GUILayout.Button("Create Default"))
				{
					new AutoBuild2.BuildInfo(){ }.Read();
					
					ab.listBuild = new List<AutoBuild2.BuildInfo>()
					{
						new AutoBuild2.BuildInfo()
						{
							target = BuildTarget.Android, isAAB = false, splitAPK = true
						}.Read(),
						new AutoBuild2.BuildInfo()
						{
							target = BuildTarget.Android, isAAB = true, splitAPK = true
						}.Read(),
						new AutoBuild2.BuildInfo()
						{
							target = BuildTarget.iOS, isAAB = true, splitAPK = true
						}.Read()
					};
				}
				return;
			}

			serializedObject.Update();
			var prop = serializedObject.FindProperty($"listBuild.Array.data[{bIndex}]");
			prop.isExpanded = true;

			var info = ab.listBuild[bIndex];

			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(prop, true);
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}

			var r = GUILayoutUtility.GetLastRect();
			r.x = 0;
			r.width = Screen.width;
			r.height = 16f;

			AutoBuild2.DrawRect(r, new Color32(20, 20, 20, 255));

			r.xMin += 12f;
			GUI.Label(r, info.buildName);

			if (ab.icon != null && ab.icon == info.buildIcon)
			{
				EditorGUILayout.HelpBox("Build Icon must be different with the template icon (which will be overwritten)", MessageType.Error);
			}

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Read"))
				{
					info.Read();
				}

				if (GUILayout.Button("Write"))
				{
					info.Write();
				}

				if (GUILayout.Button("+"))
				{
					info.IncreaseVersion();
				}
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Dry Run"))
			{
				ab.DryRun(info);
				helpInfo = null;
				Repaint();
			}

			bIndex = EditorGUILayout.IntSlider(bIndex, 0, max - 1);

			var nBuilds = 0;
			for (var i = 0; i < ab.listBuild.Count; i++)
			{
				var item = ab.listBuild[i];

				if (!item.enable) continue;
				nBuilds++;

				var hint = item.target == BuildTarget.Android
					? $"{string.Join("\n", item.GetAPKNames(ab.buildFolder))}"
					: item.fullBuildPath(ab.buildFolder);

				EditorGUILayout.HelpBox(hint, MessageType.Info);
			}

			if (nBuilds == 0) return;

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Build"))
				{
					ab.buildRQ = AutoBuild2.BuildRequestTS.Generate();
					ab.ProcessBuild();
				}

				if (GUILayout.Button("Stop"))
				{
					ab.StopBuild();
				}
			}
			GUILayout.EndHorizontal();
		}
	}


	public class ExternalProcessUtils
	{
		public static string FileFromTemplate(string fileName, string source, params string[] tokens)
		{
			for (var i =0 ;i < tokens.Length; i+=2)
			{
				source = source.Replace(tokens[i], tokens[i+1]);
			}

			var filePath = Application.dataPath.Replace("/Assets", "/" + fileName);
			// Debug.LogWarning("FilePath: " + filePath);
			File.WriteAllText(filePath, source);
			return filePath;
		}
		
		public static void GitResetRepo()
		{
			RunCMD("git reset --hard | git clean -df");
		}
		
		public static void RunCMD(string cmd)
		{
			try
			{
				var startInfo = new ProcessStartInfo()
				{
					FileName = "/bin/bash",
					Arguments = $"-c \" {cmd} \" ",
					CreateNoWindow = true
				};
				
				var proc = new Process()
				{
					StartInfo = startInfo
				};
				
				proc.Start();
				proc.WaitForExit();
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}
		}

		public static void ChmodX(string filePath)
		{
			RunCMD($"chmod +x  {filePath}");
		}
		
		public static void Run(string filePath, bool waitExit = false)
		{
			ChmodX(filePath);
			
			try
			{
				var process = new Process
				{
					EnableRaisingEvents = false,
					StartInfo =
					{
						FileName = filePath,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardInput = true,
						RedirectStandardError = true
					}
				};

				process.Start();
				if (waitExit) process.WaitForExit();
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}
		}
	}
}
