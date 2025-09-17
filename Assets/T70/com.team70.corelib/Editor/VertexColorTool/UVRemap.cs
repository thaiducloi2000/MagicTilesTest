using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVRemap : MonoBehaviour
{
	public Texture2D tex1;
	public Texture2D tex2;
	public int nCols1;
	public int nCols2;
	public Color32[] colors1;
	public Color32[] colors2;
	public int[] indexRemap;
	
	public Color32[] ReadColors(Texture2D tex, int nCols)
	{
		var result = new Color32[nCols];
		var px = tex.width / nCols;

		for (int i = 0;i < nCols; i++)
		{
			result[i] = tex.GetPixel(i * px + px/2, px/2);
		}

		return result;
	}

	[ContextMenu("Read Colors")]
	public void ReadColors()
	{
		colors1 = ReadColors(tex1, nCols1);
		colors2 = ReadColors(tex2, nCols2);
	}

	public int GetNearestColorIndex(Color32[] arr, Color32 c)
	{
		var minDelta = 100000;
		var minIndex = 0;

		for (int i = 0; i< arr.Length; i++)
		{
			var color1 = arr[i];
			var color2 = c;
			var delta = Mathf.Abs(color1.r - color2.r) + Mathf.Abs(color1.g - color2.g) + Mathf.Abs(color1.b - color2.b) + Mathf.Abs(color1.a - color2.a);
			if (delta == 0) return i;
			
			if (delta < minDelta)
			{
				minDelta = delta;
				minIndex = i;
			}
		}
		return minIndex;
	}


	[ContextMenu("Map Index")]
	public void MapIndex()
	{
		indexRemap = new int[colors1.Length];
		for (int i = 0;i < colors1.Length; i++)
		{
			indexRemap[i] = GetNearestColorIndex(colors2, colors1[i]);
		}
	}

	public Mesh mesh;

	[ContextMenu("Map UV")]
	public void MapUV()
	{	
		var dict = new Dictionary<Color, Vector2>();
		for (var i = 0;i < colors1.Length; i++)
		{
			var idx = indexRemap[i];
			
			if (dict.ContainsKey(colors1[i]))
			{
				Debug.LogWarning("Duplicated key: " + colors1[i]);
				continue;
			}

			dict.Add(colors1[i], new Vector2
			(
				(idx+0.5f) / nCols2,
				2f / 255
			));
		}
		

		var oUV = mesh.uv;
		var nVertices = mesh.vertexCount;

		var uvs = new Vector2[nVertices];
		for (var i = 0;i < nVertices; i++)
		{
			var uv = oUV[i];
			var c1 = tex1.GetPixel(Mathf.RoundToInt(uv.x * tex1.width), Mathf.RoundToInt((1f - uv.y) * tex1.height));
			uvs[i] = dict[c1];
		}
		
		var mesh2 = UnityEngine.Object.Instantiate(mesh);
		mesh2.uv = uvs;
		mesh2.name = mesh.name;
		mesh2.UploadMeshData(false);

		var go = new GameObject() { name = mesh.name };
		var mf = go.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh2;
		go.AddComponent<MeshRenderer>();
	}
}
