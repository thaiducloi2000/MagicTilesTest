using System.Net.Mime;
using UnityEngine;
using UnityEditor;

public class CameraPreview : EditorWindow
{
	const int WW = 540;
	const int HH = 960;

	static Camera camera;
	static RenderTexture renderTexture;
	static EditorWindow editorWindow;


	[MenuItem("T70/Panel/Camera Preview")]
	public static void Init()
	{
		editorWindow = GetWindow<CameraPreview>(typeof(CameraPreview));
		editorWindow.autoRepaintOnSceneChange = true;
		editorWindow.Show();
	}
	
	void Update()
	{
		if (camera == null) return;
		DoRender();
	}

	void OnSelectionChange()
	{
		if (editorWindow == null)
		{
			editorWindow = this;
		}
		
		var obj = Selection.activeGameObject;
		if (obj == null)
			return;

		var cam = obj.GetComponent<Camera>();
		if (cam == null) return;

		camera = cam;
		editorWindow.titleContent = new GUIContent(cam.name);
		
		DoRender();
		Repaint();
	}

	void DoRender()
	{
		EnsureRenderTexture();
		camera.renderingPath = RenderingPath.UsePlayerSettings;
		camera.targetTexture = renderTexture;
		
		var isEnabled = camera.enabled;
		camera.enabled = true;
		camera.Render();
		camera.targetTexture = null;
		camera.enabled = isEnabled;
	}

	void EnsureRenderTexture()
	{
		if (renderTexture == null)
		{
			renderTexture = new RenderTexture(WW, HH, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		}
	}

	void OnGUI()
	{
		if (camera == null)
		{
			if (GUILayout.Button("Find Camera"))
			{
				camera = GameObject.FindObjectOfType<Camera>();
			}
		}

		if (camera == null)
		{
			EditorGUILayout.HelpBox("Please select a Camera", MessageType.Info);
			return;
		}
		
		if (renderTexture != null)
		{
			// GUI.DrawTexture(new Rect(0.0f, 0.0f, WW, HH), renderTexture);
			GUI.DrawTexture(new Rect(0f, 0f, position.width, position.height), renderTexture, ScaleMode.ScaleToFit);
		}

		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Capture SceneView", EditorStyles.miniButton)) //new Rect(2f,2f, 100f, 24f), 
			{
				CaptureSceneView();
			}

			if (GUILayout.Button("Look through", EditorStyles.miniButton)) //new Rect(2f,2f, 100f, 24f), 
			{
				LookThrough();
			}
		}
		GUILayout.EndHorizontal();

		Repaint();
	}

	public void CaptureSceneView()
	{
		var sv = GetWindow<SceneView>();
		if (sv == null) return;
		if (camera == null) return;

		var svcam = sv.camera;
		Undo.RecordObjects(new Object[]{ camera.transform, camera }, "Capture scene view");
		camera.transform.position = svcam.transform.position;
		camera.transform.rotation = svcam.transform.rotation;
		camera.fieldOfView = svcam.fieldOfView;
	}

	public void LookThrough()
	{
		var sv = EditorWindow.GetWindow<SceneView>();
		if (sv == null) return;
		if (camera == null) return;

		var svcam = sv.camera;
		sv.pivot = camera.transform.position;
		sv.rotation = camera.transform.rotation;
		svcam.CopyFrom(camera);
	}
}