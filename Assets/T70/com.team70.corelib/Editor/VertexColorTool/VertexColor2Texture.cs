using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;

namespace vietlabs.vc2t
{
	[InitializeOnLoad] public class SelectionHelper 
	{
		[InitializeOnLoadMethod]
		static void Init()
		{
			Selection.selectionChanged -= OnSelectionChange;
			Selection.selectionChanged += OnSelectionChange;
		}
		
		static HashSet<GameObject> selectionSet = new HashSet<GameObject>();
        static HashSet<GameObject> newSet = new HashSet<GameObject>();
        static HashSet<GameObject> deleteSet = new HashSet<GameObject>();
        public static List<GameObject> selectionOrdered = new List<GameObject>();
 
        private static void OnSelectionChange()
        {
            newSet.Clear();
            newSet.UnionWith(Selection.gameObjects);
 
            deleteSet.Clear();
            deleteSet.UnionWith(selectionSet);
            deleteSet.ExceptWith(newSet);
 
            foreach (var g in deleteSet)
            {
                selectionSet.Remove(g);
                selectionOrdered.Remove(g);
            }
 
            newSet.ExceptWith(selectionSet);
            foreach (var g in newSet)
            {
                selectionSet.Add(g);
                selectionOrdered.Add(g);
            }
        }
	}


	public class Utils
	{
		public static string ToHex(Color32 c)
		{
			return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
		}

		public static int CompareVertexColor(VertexColorInfo v1, VertexColorInfo v2)
		{
			var color1 = v1.colors[0];
			var color2 = v2.colors[1];

			if (color1.r != color2.r) return color1.r.CompareTo(color2.r);
			if (color1.g != color2.g) return color1.g.CompareTo(color2.g);
			if (color1.b != color2.b) return color1.b.CompareTo(color2.b);
			return color1.a.CompareTo(color2.a);
		}

		public static bool CompareMeshVertices(Mesh m1, Mesh m2, bool deep)
		{
			if (m1.vertexCount != m2.vertexCount) return false;
			if (!deep) return true; // compare number of vertices only

			var n = m1.vertexCount;
			var v1 = m1.vertices;
			var v2 = m2.vertices;

			for (int i =0; i < n; i++)
			{
				var p1 = v1[i];
				var p2 = v2[i];
				if (p1 != p2) return false;
			}

			return true;
		}

		public static List<T> GetSelection<T>() where T : Component
		{
			var s = SelectionHelper.selectionOrdered;
			var result = new List<T>();

			foreach (var go in s)
			{
				var c = go.GetComponent<T>();
				if (c == null) continue;

				result.Add(c);
			}

			return result;
		}
	}
	

	[Serializable] public class VertexColorInfo
	{
		public int index;
		public List<Color32> colors;
		public List<int> indices = new List<int>();
	}


	[Serializable]
	public class MeshInfo
	{
		public string meshName;
		public Mesh[] meshes; // mesh color animate
		[SerializeField] public List<VertexColorInfo> list;
		[NonSerialized] public Dictionary<string, VertexColorInfo> dict;
		
		public List<Transform> sourceGO = new List<Transform>();
		
		string GetKey(Color32 c1, Color32 c2)
		{
			return string.Format("{0}_{1}", Utils.ToHex(c1), Utils.ToHex(c2));
		}

		VertexColorInfo GetVCI(Color32 c1, Color32 c2, bool autoNew = true)
		{
			var id = GetKey(c1, c2);
			
			if (dict.TryGetValue(id, out VertexColorInfo vci)) return vci;
			if (!autoNew) return null;

			vci = new VertexColorInfo()
			{
				colors = new List<Color32> { c1, c2 },
				indices = new List<int>()
			};
			
			list.Add(vci);
			dict.Add(id, vci);
			return vci;
		}
		
		public void Read()
		{
			list = new List<VertexColorInfo>();
			dict = new Dictionary<string, VertexColorInfo>();
			
			var colorA = meshes[0].colors32;
			var colorB = meshes[1].colors32;
			var n = colorA.Length;
			
			// Create color pairs & distribute vertice indices
			for (int i = 0; i < n; i++)
			{
				var vci = GetVCI(colorA[i], colorB[i], true);
				vci.indices.Add(i);
			}
		}
		
		public void Sort()
		{
			list.Sort(Utils.CompareVertexColor);
		}

		public void GenerateMesh(string exportPath, int height, int nColors)
		{
			var mesh2 = UnityEngine.Object.Instantiate(meshes[0]);

			var colorA = meshes[0].colors32;
			var colorB = meshes[1].colors32;

			var uvs = new Vector2[colorA.Length];
			for (int i = 0; i < uvs.Length; i++)
			{
				var cA = colorA[i];
				var cB = colorB[i];
				var vci = GetVCI(cA, cB, false);

				if (vci == null) 
				{
					Debug.LogWarning("Something wrong! vci not found: " + cA + ":" + cB + " --> " + GetKey(cA, cB));
					return;
				}
				
				var idx = vci.index;
				uvs[i] = new Vector2((idx + 0.5f) / nColors, 0.5f / (height - 1f));
			}

			mesh2.name = meshName;
			mesh2.uv = uvs;
			mesh2.UploadMeshData(false);

			var go = new GameObject() { name = meshName, hideFlags = HideFlags.HideAndDontSave };
			var mf = go.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh2;
			ExportModel($"{exportPath}{meshName}.fbx", new UnityObject[] { go });

			// Clean up!
			UnityObject.DestroyImmediate(go, false);
		}
		
		void ExportModel(string fbxPath, UnityObject[] go)
		{
			Debug.LogWarning("Required: UnityEditor.Formats.Fbx.Exporter package!");
			//UnityEditor.Formats.Fbx.Exporter.ExportObjects($"{exportPath}{meshName}.fbx", new UnityEngine.Object[] { go });
		}
	}
	
	

	public class VertexColor2Texture : MonoBehaviour
	{
		[Range(1, 1024)] public int gradientResolution = 256;
		[Range(1, 16)] public int colorPixelSize = 4;

		public string exportPath = "Assets/";
		public string atlasName = "atlas";
		public Transform output;

		public int nColors;
		public List<MeshInfo> infos = new List<MeshInfo>();


		[ContextMenu("Add Meshes")]
		public void AddMeshes()
		{
			var list = SelectionHelper.selectionOrdered;
			var dict = new Dictionary<Mesh, MeshInfo>();
			
			for (var i = 0; i < list.Count; i++)
			{
				var item = list[i];
				var rd = item.GetComponent<MeshFilter>();
				if (rd == null) continue;

				var m = rd.sharedMesh;

				if (!dict.TryGetValue(m, out MeshInfo mi))
				{
					mi = new MeshInfo()
					{
						meshName = item.name,
						meshes = new Mesh[] { m, m},
						sourceGO = new List<Transform>()
					};

					dict.Add(m, mi);
				}

				mi.sourceGO.Add(item.transform);
			}

			infos.AddRange(dict.Values.ToList());
			infos.Sort((item1, item2) => { return item1.meshName.CompareTo(item2.meshName); });
		}

		
		[ContextMenu("Read")]
		void Read()
		{
			nColors = 0;
			var list = new List<VertexColorInfo>();

			for (int i = 0; i < infos.Count; i++)
			{
				infos[i].Read();
				list.AddRange(infos[i].list);
			}

			nColors = list.Count;
			// set the color index (for UV lookup)
			for (int i = 0; i < nColors; i++)
			{
				list[i].index = i;
			}
		}

		[ContextMenu("Sort")]
		void Sort()
		{
			infos.Sort((item1, item2) => 
			{
				return item1.meshName.CompareTo(item2.meshName);
			});

			for (int i = 0;i < infos.Count; i++)
			{
				infos[i].Sort();
			}
		}
		
		[ContextMenu("Generate Mesh & Atlas")]
		public void Generate()
		{
			Read();
			GeneratePNG();
			GenerateMeshes();
		}

		void GenerateMeshes()
		{
			for (int i = 0; i < infos.Count; i++)
			{
				infos[i].GenerateMesh(exportPath, gradientResolution, nColors);
			}
		}

		Color32 Cerp(List<Color32> colors, float pct)
		{
			var ppt = pct * (colors.Count - 1);
			var idx = Mathf.FloorToInt(ppt);

			if (idx >= colors.Count - 1) return colors[colors.Count - 1];
			if (idx < 0) return colors[0];

			var p = ppt - idx;
			return Color32.Lerp(colors[idx], colors[idx + 1], p);
		}

		void GeneratePNG()
		{
			var w = nColors;
			var h = gradientResolution;
			var c = 0;

			var p = colorPixelSize;
			var wp = w * p;
			var n = wp * h;
			
			var arr = new Color32[n];

			for (int i = 0; i < infos.Count; i++)
			{
				var m = infos[i];

				for (var k = 0; k < m.list.Count; k++)
				{
					var info = m.list[k];

					for (int r = 0; r < h; r++)
					{
						var color = h > 1 ? Cerp(info.colors, (r + 0.5f) / (h - 1f)) : info.colors[0];

						for (var c1 = 0; c1 < p; c1++)
						{
							var idx = (c * p + c1) + r * wp;
							arr[idx] = color;
						}
					}

					c = (c + 1) % wp;
				}
			}

			var tex = new Texture2D(wp, h, TextureFormat.ARGB32, false, false);
			tex.SetPixels32(arr);
			tex.Apply();

			Directory.CreateDirectory(exportPath);
			File.WriteAllBytes($"{exportPath}{atlasName}.png", tex.EncodeToPNG());
		}


		[ContextMenu("Create GameObjects")]
		public void GenerateGOs()
		{
			var basePath = $"{exportPath}{atlasName}";
			var mat = AssetDatabase.LoadAssetAtPath<Material>(basePath + ".mat");
			if (mat == null)
			{
				var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath + ".png");
				mat = new Material(Shader.Find("Mobile/Diffuse"));
				mat.mainTexture = tex;
				AssetDatabase.CreateAsset(mat, basePath + ".mat");
			}

			for (int i = 0; i < infos.Count; i++)
			{
				var info = infos[i];
				var source = info.sourceGO;
				var path = $"{exportPath}{info.meshName}.fbx";
				var mesh = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<MeshFilter>().sharedMesh;

				if (mesh == null)
				{
					Debug.LogWarning("Mesh not found: " + path);
					return;
				}

				for (var k = 0; k < source.Count; k++)
				{
					var src = source[k];
					var clone = Instantiate(src);

					var mf = clone.GetComponent<MeshFilter>();
					mf.sharedMesh = mesh;

					var mr = clone.GetComponent<MeshRenderer>();
					mr.sharedMaterial = mat;
					src.gameObject.SetActive(false);

					clone.SetParent(src.parent, false);
					clone.SetParent(output, true);
					clone.name = src.name;
				}
			}
		}

		
		public void AddPair(MeshFilter mf0, MeshFilter mf1)
		{
			var m1 = mf0.sharedMesh;
			var m2 = mf1.sharedMesh;

			if (false == Utils.CompareMeshVertices(m1, m2, true))
			{
				Debug.LogWarning("Not originated from the same mesh: " + m1 + " : "+ m2);
				return;
			}

			infos.Add(new MeshInfo()
			{
				meshName = mf0.name,
				meshes = new Mesh[]{ m1, m2},
				sourceGO = new List<Transform> { mf0.transform } // create only 1???
			});
		}

		[ContextMenu("Add Pairs")]
		public void AddManyPair()
		{
			var listMesh = Utils.GetSelection<MeshFilter>();
			for (int i = 0; i< listMesh.Count; i+=2)
			{
				AddPair(listMesh[i+0], listMesh[i+1]);
			}
		}
	}
}




#endif