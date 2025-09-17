#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace vietlabs.vc2t
{
	public class DiffuseColor2Texture
	{
		[MenuItem("Tools/Scale Diffuse Color UV")]
		public static void ScaleUV()
		{
			var s = Utils.GetSelection<MeshFilter>();
			Debug.Log("S:: => " + s.Count);

			foreach (var mf in s)
			{
				var m = mf.sharedMesh;
				var uvs = m.uv;

				for (int i = 0;i < uvs.Length; i++)
				{
					uvs[i] = new Vector2(0.5f, 1 / 255f);
				}
				
				Debug.Log(m.name + " --> " + uvs.Length);

				var m2 = UnityEngine.Object.Instantiate(m);
				m2.uv = uvs;
				mf.sharedMesh = m2;
			}
		}

		
		[MenuItem("Tools/Generate Texture")]
		public static void GenerateTexture()
		{
			var s = Selection.activeObject;

			if (!(s is Material)) 
			{
				Debug.LogWarning("Must select material");
				return;
			}

			var m = s as Material;
			var c = m.color;

			var tex = (Texture2D) m.mainTexture;
			if (tex != null)
			{
				Debug.LogWarning("Already have a texture!");
				return;
			}

			int w = 4;
			int h = 256;

			tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
			for (int i =0 ;i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					tex.SetPixel(i, j, c);
				}
			}
			tex.Apply();

			var texPath = AssetDatabase.GetAssetPath(m).Replace(".mat", ".png");
			File.WriteAllBytes(texPath, tex.EncodeToPNG());

			AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);
			
			var tex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
			m.color = Color.white;
			m.mainTexture = tex2;
		}
	}
}

#endif