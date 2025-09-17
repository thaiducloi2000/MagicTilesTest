using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractAtlasMesh : MonoBehaviour
{
	public MeshFilter filter;
	public MeshRenderer render;
	public string filePath;
	public string fileName;

	public Texture2D tex2;
	public Vector2[] uvs;
	public Vector2[] uvs2;


	[ContextMenu("Extract")]
	public void Extract()
	{
		var m 		= filter.sharedMesh;
		var mat 	= render.sharedMaterial;

		uvs 		= m.uv;
		var minUV	= uvs[0];
		var maxUV	= uvs[0];

		for (int i = 1;i < uvs.Length;i ++)
		{
			var v = uvs[i];

			if (v.x < minUV.x) minUV.x = v.x;
			if (v.x > maxUV.x) maxUV.x = v.x;
			if (v.y < minUV.y) minUV.y = v.y;
			if (v.y > maxUV.y) maxUV.y = v.y;
		}

		var tex = (Texture2D)mat.mainTexture;
		var texColors = tex.GetPixels32();
		var tw = tex.width;
		var th = tex.height;

		var x1 = Mathf.FloorToInt(minUV.x * tw);
		var y1 = Mathf.FloorToInt(minUV.y * th);

		var x2 = Mathf.CeilToInt(maxUV.x * tw);
		var y2 = Mathf.CeilToInt(maxUV.y * th);

		var w = x2-x1;
		var h = y2-y1; 
		
		tex2 = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
		var texColors2 = new Color32[w * h];
		
		for (int c = x1; c < x2; c++)
		{
			for (int r = y1; r < y2; r++)
			{
				var idx1 = c + r * tw;
				var idx2 = c-x1 + (r-y1) * w;
				texColors2[idx2] = texColors[idx1];
			}
		}
		tex2.SetPixels32(texColors2);
		tex2.Apply();
		File.WriteAllBytes("Assets/1.png", tex2.EncodeToPNG());
		
		uvs2 = new Vector2[uvs.Length];
		for (int i = 0;i < uvs.Length;i ++)
		{
			var xx = uvs[i].x - x1;
			var yy = uvs[i].y - y1;
			uvs2[i] = new Vector2(xx / w, yy / h);
		}

		var m2 = Mesh.Instantiate(m);
		m2.uv = uvs2;
		
		var go = Instantiate(filter.gameObject);
		go.GetComponent<MeshFilter>().sharedMesh = m2;
	}
}
