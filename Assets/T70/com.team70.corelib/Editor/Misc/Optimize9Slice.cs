using System.IO;
using UnityEditor;
using UnityEngine;

public class Optimize9Slice : EditorWindow {
	static public int tollerant = 4;
	// public static bool copyFail = false;
	
	[MenuItem("Assets/T70/Optimize 9-slices")]
	static void DoOptimize9Slice()
	{
		var list = Selection.objects;
		for (int i = 0; i < list.Length; i++) 
		{
			if (!(list[i] is Texture2D)) continue;
			OptimizeTexture((Texture2D)list[i], false);
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

	[MenuItem("Assets/T70/Border 9-slices")]
	static void DoBorder9Slice()
	{
		var list = Selection.objects;
		for (int i = 0; i < list.Length; i++) {
			if (!(list[i] is Texture2D)) continue;
			OptimizeTexture((Texture2D)list[i], true);
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}




	static void OptimizeTexture(Texture2D tex, bool borderOnly) 
	{
		var path = AssetDatabase.GetAssetPath(tex);
		var importer = (TextureImporter)AssetImporter.GetAtPath(path);
		importer.isReadable = true;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.maxTextureSize = 4096;
		importer.alphaIsTransparency = true;
		importer.npotScale = TextureImporterNPOTScale.None;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

		if (tex.width <= 4 || tex.height <= 4)
		{
			Debug.LogWarning("Skipped too small texture <" + path +">");
			// if (copyFail)
			// {
            //     SaveTexture2File(tex, tex.name);//.Replace("belt_", "")
			// }
			return;
		}

		//Debug.Log("optimizing ... " + tex);

		var pixels = tex.GetPixels32();
		var w = tex.width;
		var h = tex.height;

		var miny = 0;
		var minx = 0;
		var maxx = w;
		var maxy = w;

		for (int y = 0; y <= h / 2; y++)
		{
			if (SameRowInfo(pixels, w, h, y, y + 1)) continue;
			miny = y;
		}

		for (int y = h - 1; y >= h / 2; y--)
		{
			if (SameRowInfo(pixels, w, h, y, y - 1)) continue;
			maxy = y;
		}

		for (int x = 0; x <= w / 2; x++)
		{
			if (SameColInfo(pixels, w, h, x, x + 1)) continue;
			minx = x;
		}

		for (int x = w - 1; x >= w / 2; x--)
		{
			if (SameColInfo(pixels, w, h, x, x - 1)) continue;
			maxx = x;
		}

		var l = minx;
		var r = w-maxx;
		var t = h-maxy;
		var b = miny;
		
		var dX = w - (l + r);
		var dY = h - (t + b);

		if (dX <= 0)
		{
			l = 0;
			r = 0;
		}

		if (dY == 0)
		{
			t = 0;
			b = 0;
		}
		
		importer.spriteBorder = new Vector4(l, b, r, t);
		if (borderOnly) return;

		if (dX > tollerant || dY > tollerant)
		{
			SaveTextureSlice(path, pixels, w, h, l, b, r, t, Mathf.Min(dX, tollerant), Mathf.Min(dY, tollerant));
			var stringX = dX > 0 ? $"left: {l}, right: {r}" : string.Empty;
			var stringY = dY > 0 ? $"top: {t}, bottom: {b}" : string.Empty;
			Debug.LogWarning($"Optimized: {path} ->> {w}x{h} \n {stringX}, {stringY}, dx = {dX}, dy = {dY}");
			importer.SaveAndReimport();
		}
		// else
		// {
		// 	Debug.Log($"Skipping: {path} ->>\n {l}, {r}, {t}, {b}, dx = {dX}, dy = {dY}");
		// }
	}

	static void SaveTextureSlice(string name, Color32[] source, int w, int h, int bx1, int by1, int bx2, int by2, int dx, int dy)
	{
        //if (by2 < 0) by2 = 0;

		var w2 = Mathf.Min(bx1 + bx2 + dx, w);
		var h2 = Mathf.Min(by1 + by2 + dy, h);
		var colors = new Color32[ w2 * h2 ];

		// Debug.LogWarning($"NEW SIZE: {w}x{h} --> {bx1 + bx2 + dx}x{by1 + by2 + dy} --> {bx1}, {bx2}, {by1}, {by2}, {dx}, {dy}" );
		
		for (var i = 0; i < colors.Length; i++) {
			var x = i % w2;
			var y = i / w2;

			if (x > bx1 + dx) x += w - w2;
			if (y > by1 + dy) y += h - h2;
			colors[i] = source[y * w + x];
		}
		
		var texture = new Texture2D(w2, h2, TextureFormat.ARGB32, false);
		texture.SetPixels32(colors);
		texture.name = name;
        SaveTexture2File(texture, name);
	}

	static void SaveTexture2File(Texture2D tex, string fullName)
	{
		File.WriteAllBytes(
			fullName,
			tex.EncodeToPNG()
		);
	}
	
	static bool SameRowInfo(Color32[] arr, int w, int h, int ln1, int ln2)
	{
		for (int i = 0; i < w; i++) {
			if (arr[i+ln1*w].Equals(arr[i+ln2*w])) continue;
			return false;
		}
		return true;
	}

	static bool SameColInfo(Color32[] arr, int w, int h, int col1, int col2)
	{
		for (int i = 0; i < h; i++) {
			if (arr[i*w + col1].Equals(arr[i*w + col2])) continue;
			return false;
		}
		return true;
	}
}
