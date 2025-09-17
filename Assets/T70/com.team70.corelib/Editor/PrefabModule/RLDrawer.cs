using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.team70
{
	public class TypeAvailable
	{
		public Type type;
		public int count;
	}
	
	public class RLDrawer
	{
		public ReorderableList drawer;
		public ElementInfo info;
		private Type type;

		public int selectedIndex;
		public GUIContent[] contents;
		public List<TypeAvailable> availableTypes;

		PrefabModule module;
		
		public RLDrawer(ElementInfo info, PrefabModule module)
		{
			this.info = info;
			this.module = module;
			
			// Backward compatible
			if (info.componentType == "PrefabModule")
			{
				info.componentType = "com.team70.PrefabModule";
			}
			
			type = SerializableType.GetTypeByName(info.componentType) ?? typeof(Component);

			drawer = new ReorderableList(info.component, typeof(Component))
			{
				drawElementCallback = DrawElement,
				drawHeaderCallback = DrawTitle,
				onAddCallback = OnAdd,
				onRemoveCallback = OnRemove,
				onReorderCallback = OnReorder
			};

			RefreshAvailableTypes();
		}

		public void RefreshAvailableTypes()
		{
			var dict = new Dictionary<Type, TypeAvailable>();

			foreach (var c in info.component)
			{
				if (c == null) continue;
				var go = c.gameObject;
				var all = go.GetComponents<Component>();

				foreach (var cs in all)
				{
					if (cs == null) continue;

					Type cst = cs.GetType();
					if (!dict.TryGetValue(cst, out TypeAvailable ta))
					{
						ta = new TypeAvailable()
						{
							count = 1,
							type = cst
						};
						dict.Add(cst, ta);
						continue;
					}

					ta.count++;
				}
			}

			if (!dict.ContainsKey(type)) // missing type??
			{
				dict.Add(type, new TypeAvailable()
				{
					count = 0,
					type = type
				});
			}

			var cType = typeof(Component);
			if (!dict.ContainsKey(cType)) // missing type??
			{
				dict.Add(cType, new TypeAvailable()
				{
					count = 0,
					type = cType
				});
			}

			availableTypes = dict.Values.ToList();
			availableTypes.Sort((item1, item2) =>
			{
				var result = item1.count.CompareTo(item2.count);
				return result != 0 ? result : string.Compare(item1.type.Name, item2.type.Name, StringComparison.Ordinal);
			});

			selectedIndex = availableTypes.FindIndex(item => item.type == type);
			contents = availableTypes.Select(item => new GUIContent(item.type.Name)).ToArray();
		}

		public void DrawLayout()
		{
			if (info.component.Count <= 1)
			{
				var rect = GUILayoutUtility.GetRect(0, Screen.width, 20f, 20f);
				DrawSingle(rect);
			}
			else
			{
				drawer.DoLayoutList();
			}
		}

		void OnReorder(ReorderableList l)
		{
			EditorUtility.SetDirty(module);
		}

		void OnAdd(ReorderableList l)
		{
			l.list.Add(null);
			EditorUtility.SetDirty(module);
		}

		void OnRemove(ReorderableList l)
		{
			l.list.RemoveAt(l.index);
			EditorUtility.SetDirty(module);
		}

		void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			var c = (info.component.Count <= index) ? null : info.component[index];
			var isValid = (c != null) && type.IsInstanceOfType(c);

			var color = GUI.color;
			GUI.color = isValid ? color : new Color(.9f, 0f, 0f, 1f);
			{
				var cc = (Component)EditorGUI.ObjectField(rect, c, typeof(Component), true);

				// changed GameObject
				if (cc != null && (c == null || (c.gameObject != cc.gameObject)))
				{
					if (info.component.Count <= index)
					{
						info.component.Add(cc);
					}
					else
					{
						info.component[index] = cc;
					}

					var cType = SerializableType.GetTypeByName(info.componentType) ?? typeof(Component);
					
					Debug.Log(cType);
					
					if (!cType.IsInstanceOfType(cc)) // different type
					{
						var c1 = cc.gameObject.GetComponent(type);
						if (c1 != null)
						{
							info.component[index] = c1;
						}
					}

					RefreshAvailableTypes();
					EditorUtility.SetDirty(module);
				}
			}
			GUI.color = color;

			if (!isActive) return;

			color = GUI.color;
			GUI.color = new Color(0, 1f, 1f, 0.2f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture);
			GUI.color = color;
		}

		void DrawPolish(Rect rect)
		{
			if (!GUI.Button(rect, "Polish")) return;

			var data = (List<Component>)drawer.list;

			var h = new HashSet<Component>();
			for (var i = 0; i < data.Count; i++)
			{
				if (h.Contains(data[i]))
				{
					data[i] = null;
					continue;
				}

				h.Add(data[i]);
			}

			for (var i = data.Count - 1; i >= 0; i--)
			{
				if (data[i] == null) data.RemoveAt(i);
			}

			EditorUtility.SetDirty(module);
		}

		void DrawHeader(Rect rect)
		{
			var isValid = !string.IsNullOrEmpty(info.id);
			var lb = isValid ? info.id : "<invalid id>";

			var color = GUI.color;
			{
				if (!isValid) GUI.color = Color.red;
				rect.xMin += 8f;
				if (GUI.Button(rect, lb, EditorStyles.linkLabel) && isValid)
				{
					GUIUtility.systemCopyBuffer = info.id;
					Debug.Log(info.id);
				}
			}
			GUI.color = color;

			var list = Utils.DropZone(rect, type);
			if (list != null && list.Count > 0)
			{
				foreach (var item in list)
				{
					drawer.list.Add(item);
				}

				RefreshAvailableTypes();
				EditorUtility.SetDirty(module);
			}
		}

		void DrawComponentPopup(Rect rect)
		{
			var idx = EditorGUI.Popup(rect, selectedIndex, contents);
			if (idx != selectedIndex)
			{
				selectedIndex = idx;
				type = availableTypes[idx].type;
				info.componentType = type.FullName;

				for (int i = 0; i < info.component.Count; i++)
				{
					var c = info.component[i];
					if (c == null) continue;

					if (type.IsInstanceOfType(c)) continue;

					var c1 = c.gameObject.GetComponent(type);
					if (c1 != null)
					{
						info.component[i] = c1;
					}
				}

				EditorUtility.SetDirty(module);
			}
		}

		void DrawSingle(Rect rect)
		{
			var headerRect = rect;
			headerRect.xMax -= 300f;
			DrawHeader(headerRect);

			var popupRect = rect;
			popupRect.xMin = rect.xMax - 100f;
			popupRect.width = 100f;
			DrawComponentPopup(popupRect);

			var elementRect = new Rect(headerRect.xMax, rect.y, 200f, rect.height);
			DrawElement(elementRect, 0, false, false);
		}

		void DrawTitle(Rect rect)
		{
			var headerRect = rect;
			headerRect.xMin -= 5f;
			headerRect.xMax -= 195f;
			DrawHeader(headerRect);

			var popupRect = rect;
			popupRect.xMin = headerRect.xMax;
			popupRect.width = 120f;
			DrawComponentPopup(popupRect);

			var buttonRect = rect;
			buttonRect.xMin = popupRect.xMax;
			buttonRect.width = 80f;
			DrawPolish(buttonRect);
		}
	}
}
