using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace nano.vs2
{
	public interface IColumnAdapter
	{
		int stateClicked { get; }
		int State { get; set; }
		int nStates { get; }
		List<AmountData> BlemData { get; }
		void RemoveState(int state);
		void AddState(int state);
		void SetScrollX(float x);
		bool lockTarget { get; set; }
		VisualState vs { get; }
	}

	public struct GUIColor : IDisposable
	{
		Color cache;
		public GUIColor(Color c)
		{
			cache = GUI.color;
			GUI.color = c;
		}

		public void Dispose()
		{
			GUI.color = cache;
		}
		public static GUIColor Get(Color c)
		{
			return new GUIColor(c);
		}
	}
	public struct GUIBGColor : IDisposable
	{
		Color cache;
		public GUIBGColor(Color c)
		{
			cache = GUI.backgroundColor;
			GUI.backgroundColor = c;
		}

		public void Dispose()
		{
			GUI.backgroundColor = cache;
		}
		public static GUIBGColor Get(Color c)
		{
			return new GUIBGColor(c);
		}
	}
	public class VSColumnHeader : MultiColumnHeader
	{
		private IColumnAdapter columnAdapter;
		public VSColumnHeader(MultiColumnHeaderState state, IColumnAdapter columnAdapter) : base(state)
		{
			this.columnAdapter = columnAdapter;

		}
		protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
		{
			if (columnAdapter.stateClicked >= 0)
			{
				menu.AddItem(new GUIContent("Remove"), false, OnRemoveState);
				menu.AddItem(new GUIContent("Duplicate"), false, OnDuplicate_Click);
			}
			// base.AddColumnHeaderContextMenuItems(menu);
		}

		private void OnDuplicate_Click()
		{
			columnAdapter.AddState(columnAdapter.stateClicked);
			// col
		}

		private void OnRemoveState()
		{
			columnAdapter.RemoveState(columnAdapter.stateClicked);
		}

		protected override void ToggleVisibility(int columnIndex)
		{
			Debug.Log("ToggleVisibility " + columnIndex);
			base.ToggleVisibility(columnIndex);
		}
		protected override void ColumnHeaderClicked(MultiColumnHeaderState.Column column, int columnIndex)
		{
			Debug.Log("click " + columnIndex);
			//base.ColumnHeaderClicked(column, columnIndex);
		}
		protected override void OnVisibleColumnsChanged()
		{
			base.OnVisibleColumnsChanged();
			Debug.Log("OnVisibleColumnsChanged");
		}
		protected override void OnSortingChanged()
		{
			base.OnSortingChanged();
			Debug.Log("OnSortingChanged");
		}
		public override void OnGUI(Rect rect, float xScroll)
		{
			columnAdapter.SetScrollX(xScroll);
			base.OnGUI(rect, xScroll);
		}
		protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
		{
			// if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
			// {
			//     ColumnHeaderClicked(column, columnIndex);
			// }

			if ((columnAdapter.State + 1) == columnIndex)
			{
				EditorGUI.DrawRect(headerRect, VSStyle.SELECTED_STATE_COLUMN_COLOR);
			}
			var s = EditorStyles.boldLabel;
			s.alignment = TextAnchor.UpperCenter;
			EditorGUI.LabelField(headerRect, column.headerContent, s);
			if (columnIndex == 0)
			{
				Rect rect = new Rect(headerRect.x, headerRect.y, headerRect.height, headerRect.height);

				using (GUIColor.Get(Color.black))
				{
					if (GUI.Button(rect, columnAdapter.lockTarget ? VSStyle.lockOnIconContent : VSStyle.lockIconContent, EditorStyles.centeredGreyMiniLabel))
					{
						columnAdapter.lockTarget = !columnAdapter.lockTarget;
					}
				}
				var objectRect = headerRect;
				objectRect.x = rect.x + rect.width + 5;
				objectRect.width -= rect.width + 5;
				objectRect.height = EditorGUIUtility.singleLineHeight;
				objectRect.y += (headerRect.height - objectRect.height) / 2;
				using (GUIBGColor.Get(Color.cyan))
				{
					EditorGUI.ObjectField(objectRect, columnAdapter.vs, typeof(Object), true);
				}

				return;
			}
			DrawBlenData(headerRect, columnIndex);


			//DrawSettingButton(headerRect, columnIndex);


		}
		private void DrawSettingButton(Rect headerRect, int columnIndex)
		{
			if (columnIndex > columnAdapter.nStates) return;

			var r2 = headerRect;
			var width = 15;
			Rect settingRect = new Rect(r2.x + r2.width - width, r2.y, width, width);
			if (GUI.Button(settingRect, "-"))
			{
				columnAdapter.RemoveState(columnIndex - 1);
			}
		}
		private void DrawBlenData(Rect headerRect, int columnIndex)
		{
			if (columnAdapter.BlemData == null) return;
			if (columnAdapter.BlemData.Count <= (columnIndex - 1)) return;
			var amount = columnAdapter.BlemData[columnIndex - 1].amount;
			var r = headerRect;

			var propressHeight = 8;
			int padding = 3;

			r.height = propressHeight;
			r.x += padding;
			r.width -= padding * 2;
			r.y = (headerRect.y + headerRect.height) - (propressHeight + padding);



			if (amount > 0)
			{

				EditorGUI.ProgressBar(r, amount, "");
			}
		}

	}
	public class TreeItemData : TreeViewItem
	{
		public bool isDuplicated = false;
		public object obj;
		public bool isProperty;
		public TreeViewItem toTreeViewItem(int depth)
		{
			return new TreeViewItem
			{
				displayName = displayName,
				id = id,
				depth = depth
			};
			//this.depth = depth;
			//return this;
		}
	}
	public enum GroupType
	{
		FullStruct,
		Component,
		GameObject
	}
	public class VSTreeViewMultipleColumn : VSTreeview
	{
		const float PROPERTY_MARGIN_LEFT = 20;

		const float ROW_HEIGHT = 20;
		const float kToggleWidth = 18f;
		private VSEditor vsEditor;
		public VSTreeViewMultipleColumn(TreeViewState state, VSTreeModel model) : base(state, model)
		{
		}

		public VSTreeViewMultipleColumn(TreeViewState state, MultiColumnHeader multiColumnHeader, VSTreeModel model, VSEditor vsEditor) : base(state, multiColumnHeader, model)
		{
			this.vsEditor = vsEditor;
			rowHeight = ROW_HEIGHT;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			//customFoldoutYOffset = (ROW_HEIGHT - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			//extraSpaceBeforeIconAndLabel = kToggleWidth;
			//multicolumnHeader.sortingChanged += OnSortingChanged;

			Reload();
		}
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			if (m_TreeModel.root == null)
			{
				Debug.LogError("tree model root is null. did you call SetData()?");
			}
			//Debug.Log("build rows");
			m_Rows.Clear();
			GetGroupRows(Setting.groupType, m_Rows);
			
			// We still need to setup the child parent information for the rows since this 
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths(root, m_Rows);

			return m_Rows;
		}

		private void GetGroupRows(GroupType type, List<TreeViewItem> rows)
		{
			var allItem = m_TreeModel.m_Data;
			if (allItem == null || allItem.Count == 0) return;
			var dict = getGrouping(type, allItem);
			var maxId = allItem.Count;
			foreach (var item in dict)
			{
				var lst = item.Value;
				//if(lst.Count == 1)
				//{
				//    var prop = m_TreeModel.GetData(lst[0]);
				//    rows.Add(prop.toTreeViewItem(0));
				//    continue;
				//}
				var parrent = new TreeItemData
				{
					id = ++maxId,
					isProperty = false,
					depth = 0

				};
				for (int i = 0; i < lst.Count; i++)
				{
					var prop = m_TreeModel.GetData(lst[i]);
					if (i == 0)
					{
						var propBase = (prop.obj as VSPropertyBase);
						Object com = propBase.target;
						if (propBase.target is GameObject) com = propBase.target as GameObject;
						else com = propBase.target as Component;
						if (type == GroupType.GameObject) parrent.obj = com is GameObject ? com as GameObject : (com as Component).gameObject;
						else parrent.obj = com;
						var index = propBase.property.LastIndexOf('.');

						parrent.displayName = index >= 0 ? propBase.property.Substring(0, index) : propBase.property;
						rows.Add(parrent);
						parrent.children = new List<TreeViewItem>();
						if (!IsExpanded(maxId))
						{
							parrent.children = new List<TreeViewItem>();
							// parrent.children = CreateChildListForCollapsedParent();
							// break;
						}
					}
					if (!IsExpanded(maxId))
					{
						parrent.children.Add(prop.toTreeViewItem(1));
					}
					else
					{
						rows.Add(prop.toTreeViewItem(1));
					}
					if (prop.isDuplicated)
					{
						parrent.isDuplicated = true;
					}

					// rows.Add(prop.toTreeViewItem(1));

				}

			}
		}
		private Dictionary<string, List<int>> getGrouping(GroupType type, List<TreeItemData> allItem)
		{
			var dict = new Dictionary<string, Dictionary<string, int>>();
			for (var i = 0; i < allItem.Count; i++)
			{
				var item = allItem[i];
				item.isDuplicated = false;
				if (item.id < 0) continue; //root
				var data = m_TreeModel.GetData(item.id);
				if (data == null)
				{
					Debug.LogWarning("data null " + item.id + "  name " + item.displayName);
					continue;
				}
				var prop = data.obj as VSPropertyBase;
				if (prop == null)
				{
					Debug.LogWarning("something wrong data is not property" + item.id + "  name " + item.displayName + " " + data.obj);
					continue;
				}
				var key = getKey(type, prop);
			GOTOC: Dictionary<string, int> hash = null;
				var itemHash = prop.target.GetInstanceID() + prop.property;
				if (!dict.TryGetValue(key, out hash))
				{
					hash = new Dictionary<string, int>();
					dict.Add(key, hash);
				}
				else if (hash.ContainsKey(itemHash))
				{
					key += "_clone";
					var oldM = m_TreeModel.GetData(hash[itemHash]);
					if (oldM != null)
					{
						oldM.isDuplicated = true;
					}
					item.isDuplicated = true;
					goto GOTOC;
				}
				hash.Add(itemHash, item.id);


			}
			return dict.ToDictionary(x => x.Key, x => x.Value.Values.ToList());
		}
		private string getKey(GroupType type, VSPropertyBase prop)
		{
			if (prop == null) return string.Empty;
			if (prop.target == null) return string.Empty;
			if (!(prop.target is GameObject) && (prop.target as Component) == null) return string.Empty;
			var gameObject = prop.target is GameObject ? prop.target as GameObject : (prop.target as Component).gameObject;
			if (gameObject == null) return string.Empty;
			var propName = prop.property;
			var index = propName.LastIndexOf('.');
			var strStruct = (index >= 0) ? propName.Substring(0, propName.LastIndexOf('.')) : propName;
			switch (type)
			{
				case GroupType.FullStruct:
					return gameObject.GetInstanceID() + "_" + prop.target.GetInstanceID() + "_" + strStruct;
				case GroupType.Component:
					return gameObject.GetInstanceID() + "_" + prop.target.GetInstanceID();
			}
			return gameObject.GetInstanceID().ToString();
		}


		protected override void RowGUI(RowGUIArgs args)
		{
			var item = args.item;
			var data = m_TreeModel.GetData(item.id);
			if (data == null) data = item as TreeItemData;
			if (data == null)
			{
				Debug.LogWarning("data null " + item.id + "  name " + item.displayName);
				return;
			}
			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{

				CellGUI(args.GetCellRect(i), data, args.GetColumn(i), ref args);
			}
		}

		void CellGUI(Rect cellRect, TreeItemData item, int column, ref RowGUIArgs args)
		{
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);
			var prop = item.obj as VSPropertyBase;
			if (column == 0) //property
			{
				DrawPropertyName(cellRect, item, column, ref args);
				return;
			}



			//draw property state value
			int stateIndex = vsEditor.getStateIndexWithColumn(column);
			if (stateIndex == VSEditor.ColumnMinMax)
			{
				DrawMinMax(prop, cellRect);
				return;
			}
			if (stateIndex < 0) return;


			if (prop == null) return;

			if (stateIndex >= prop.stateValues.Count) return;
			var val = prop.stateValues[stateIndex];
			if (prop.isReference)
			{
				var newVal = EditorGUI.ObjectField(cellRect, val.referenceValue, typeof(UnityEngine.Object), true);
				if (newVal != val.referenceValue)
				{
					if (newVal == null || (prop.getFieldType() == newVal.GetType()))
					{
						val.referenceValue = newVal;
						SetPropertyDirty(prop);
					}

				}
			}
			else
			{
				CustomType(cellRect, prop, stateIndex);
			}
		}
		private void DrawMinMax(VSPropertyBase prop, Rect cellGUI)
		{
			if (prop == null) return;
			var overrideData = prop.animOverrideData;
			EditorGUI.MinMaxSlider(cellGUI, ref overrideData.min, ref overrideData.max, 0, 1);
		}

		private void CustomType(Rect cellRect, VSPropertyBase prop, int stateIndex)
		{
			var val = prop.stateValues[stateIndex];

			if (prop.getFieldType() == typeof(bool))
			{
				var curBool = (bool)JsonWrapper.FromJson(val.jsonValue, prop.getFieldType(), true);
				var newBool = EditorGUI.Toggle(cellRect, curBool);
				if (newBool != curBool)
				{
					val.jsonValue = JsonWrapper.ToJson(newBool ? 1 : 0, true);
					SetPropertyDirty(prop);
				}

			}
			else if (prop.getFieldType().IsEnum)
			{
				var curEnum = (Enum)JsonWrapper.FromJson(val.jsonValue, prop.getFieldType(), true);
				var newEnum = EditorGUI.EnumPopup(cellRect, curEnum);
				if (newEnum != curEnum)
				{
					val.jsonValue = Convert.ChangeType(newEnum, newEnum.GetTypeCode()).ToString();
					SetPropertyDirty(prop);
				}
			}
			else
			{
				var newVal = EditorGUI.TextField(cellRect, val.jsonValue);
				if (newVal != val.jsonValue)
				{
					try
					{
						var v = JsonWrapper.FromJson(newVal, prop.getFieldType(), true);
						val.jsonValue = v.ToString();
						SetPropertyDirty(prop);
					}
					catch { }

				}
			}



		}
		private void SetPropertyDirty(VSPropertyBase vSProperty)
		{
			vSProperty.states.Clear();
			EditorUtility.SetDirty(vsEditor.vs);

		}
		private void DrawPropertyName(Rect baseCellRect, TreeItemData item, int column, ref RowGUIArgs args)
		{
			var cache = GUI.color;
			var isIgnore = checkIgnored(item);
			if (isIgnore) GUI.color = new Color(1, 1, 1, .5f);
			else if (item.isDuplicated)
			{
				GUI.color = Setting.DuplicatedPropertyColor;
			}


			var prop = item.obj as VSPropertyBase;
			var indent = PROPERTY_MARGIN_LEFT + (args.item.depth > 0 ? VSStyle.INDENT_SPACE : 0);

			var settingSize = VSStyle.PROPERTY_ELEMENT_SETTING_SIZE;
			var cellRect = baseCellRect;
			cellRect.x += indent;
			cellRect.width -= indent;



			const float MARGIN = 2;
			var width = cellRect.width / 2;

			var leftRect = cellRect;
			leftRect.width = width;

			var rightRect = leftRect;
			rightRect.x += width + MARGIN;
			rightRect.width -= MARGIN + settingSize;
			DrawPropertySettingButton(cellRect, item);
			if (!item.isProperty)
			{
				var target = item.obj as Object;
				// if (target == null) Debug.Log(item.obj);
								
				var newtarget = EditorGUI.ObjectField(leftRect, target, typeof(Object), true);
				if (newtarget != target)
				{
					if (newtarget != null)
					{
						var hasType = false;
						Component com = null;
						if (newtarget is GameObject)
						{
							com = (newtarget as GameObject).GetComponent(target.GetType());
							hasType = com != null;
						}
						if (!hasType)
						{

							EditorUtility.DisplayDialog("Change Target Fail", "Cannot change to this target", "Ok");
						}
						else
						{
							if (com != target)
							{
								if (EditorUtility.DisplayDialog("Change Target", "Change target from: " + target.name + " to: " + newtarget.name + "?", "Ok", "Cancel"))
								{
									foreach (var child in item.children)
									{
										var c = m_TreeModel.GetData(child.id);
										if (c == null)
										{
											continue;
										}
										var p = c.obj as VSPropertyBase;
										p.target = com;
									}
									Reload();
								}
							}
						}
					}

				}
				EditorGUI.LabelField(rightRect, item.displayName);
				//DrawPropertySettingButton(cellRect, item);

				var pathR = args.rowRect;
				pathR.x += baseCellRect.width + 5;
				pathR.width = args.rowRect.width - baseCellRect.width;
				DrawFullPathName(pathR, target);

				if (isIgnore) GUI.color = Color.white;
				else if (item.isDuplicated)
				{
					GUI.color = cache;
				}
				return;
			}
			EditorGUI.LabelField(leftRect, item.displayName);





			DrawCurrentValue(rightRect, prop);

			//DrawPropertySettingButton(cellRect, item);

			if (item.isDuplicated)
			{
				GUI.color = cache;
			}
			if (isIgnore) GUI.color = Color.white;

		}
		private void DrawFullPathName(Rect r, Object obj)
		{
			var cache = GUI.color;
			GUI.color = new Color(1, 1, 1, .5f);
			EditorGUI.LabelField(r, VisualState_Unity.GetFullPath(obj), EditorStyles.miniLabel);
			GUI.color = cache;
		}

		private void DrawCurrentValue(Rect rightRect, VSPropertyBase prop)
		{
			if (prop == null) return;
			if (prop.cacheCurValue == null) return;

			EditorGUI.LabelField(rightRect, prop.cacheCurValue.ToString(), EditorStyles.miniLabel);
		}
		private void DrawPropertySettingButton(Rect cellRect, TreeItemData item)
		{
			var size = VSStyle.PROPERTY_ELEMENT_SETTING_SIZE;
			var r = new Rect(cellRect.x + cellRect.width - size - 1, cellRect.y, size, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(r, VSStyle.propertySettingContent, GUIStyle.none))
			{
				//vsEditor.RemoveProperty(prop);
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Remove properties"), false, (item1)=>
				{
					BatchProcess(item1, (prop)=> 
					{
						var vs = vsEditor.GetVisualState();
						vs.RemoveProperty(prop);
					});
				}, item);

				var ignored = checkIgnored(item);
				menu.AddItem(new GUIContent("Ignore Properties"), ignored, (item1) =>
				{
					BatchProcess(item1, (prop)=> 
					{
						var vs = vsEditor.GetVisualState();
						prop.isIgnore = !ignored;
					});
				}, item);

				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Duplicate..."), false, DuplicateProp, item);
				menu.ShowAsContext();
			}
		}
		private bool checkIgnored(TreeItemData data)
		{
			if (data.isProperty)
			{
				VSPropertyBase prop = data.obj as VSPropertyBase;
				if (!isNull(prop, "VSPropertyBase"))
				{
					return prop.isIgnore;
				}
				return false;
			}

			if (isNull(data.children, "data.children")) return false;

			foreach (var item in data.children)
			{
				if (isNull(data.children, "item")) continue;
				var d = m_TreeModel.GetData(item.id);
				if (isNull(d, "data " + item.id)) continue;

				VSPropertyBase prop = d.obj as VSPropertyBase;
				if (isNull(prop, "prop " + item.id + " obj " + d.obj)) continue;

				if (!prop.isIgnore) return false;
			}
			return true;
		}
		private void IgnoreProp(object itemRemove)
		{
			var data = (TreeItemData)itemRemove;
			var valueSet = !checkIgnored(data);
			var vs = vsEditor.GetVisualState();
			if (data.isProperty)
			{
				VSPropertyBase prop = data.obj as VSPropertyBase;
				if (isNull(prop, "VSPropertyBase")) return;
				prop.isIgnore = valueSet;
			}
			else
			{
				if (isNull(data.children, "data.children")) return;

				foreach (var item in data.children)
				{
					if (isNull(data.children, "item")) continue;
					var d = m_TreeModel.GetData(item.id);
					if (isNull(d, "data " + item.id)) continue;

					VSPropertyBase prop = d.obj as VSPropertyBase;
					if (isNull(prop, "prop " + item.id + " obj " + d.obj)) continue;
					prop.isIgnore = valueSet;
				}
			}
		}

		public VSPropertyBase GetPropertyBase(int treeItemId)
		{
			TreeItemData data = m_TreeModel.GetData(treeItemId);
			if (!data.isProperty) return null;
			
			VSPropertyBase prop = data.obj as VSPropertyBase;
			if (isNull(prop, "VSPropertyBase")) return null;
			return prop;
		}

		public void BatchProcess(object itemRemove, Action<VSPropertyBase> func)
		{
			TreeItemData data;

			var rid = ((TreeViewItem)itemRemove).id;
			
			var selection = new List<int>();
			selection.AddRange(this.GetSelection());
			if (!selection.Contains(rid))
			{
				selection.Add(rid);
			}

			var rows =this.FindRows(selection);

			for (int i = 0; i < selection.Count; i++)
			{
				data = this.m_TreeModel.GetData(selection[i]);
				
				if (data == null) 
				{
					var children = rows[i].children;
					foreach (var c in children)
					{
						var prop1 = GetPropertyBase(c.id);
						if (prop1 != null) func(prop1);
					}

					continue;
				}

				var prop = GetPropertyBase(data.id);
				if (prop != null) func(prop);
			}
		}




		public bool isNull(object obj, string name = "")
		{
			if (obj == null)
			{
				Debug.LogWarning("Null: " + name);
				return true;
			}
			return false;
		}
		private TreeItemData _cacheCopyItem = null;
		public void DuplicateProp(object itemRemove)
		{
			var data = (TreeItemData)itemRemove;

			var vs = vsEditor.GetVisualState();
			if (data.isProperty)
			{
				VSPropertyBase prop = data.obj as VSPropertyBase;
				if (isNull(prop, "VSPropertyBase")) return;
				vs.listValue.Add((VSPropertyBase)prop.Clone());
			}
			else
			{
				if (isNull(data.children, "data.children")) return;

				foreach (var item in data.children)
				{
					if (isNull(data.children, "item")) continue;
					var d = m_TreeModel.GetData(item.id);
					if (isNull(d, "data " + item.id)) continue;

					VSPropertyBase prop = d.obj as VSPropertyBase;
					if (isNull(prop, "prop " + item.id + " obj " + d.obj)) continue;
					vs.listValue.Add((VSPropertyBase)prop.Clone());
				}
			}
			vs.OnDataStructChanged();

		}
		private void OnSelectTarget(Component com)
		{
			Debug.Log("select: " + com);
			if (_cacheCopyItem == null) return;

			var vs = vsEditor.GetVisualState();
			if (_cacheCopyItem.isProperty)
			{
				VSPropertyBase prop = _cacheCopyItem.obj as VSPropertyBase;
				if (prop == null) return;
				vs.CopyProperty(prop, com);
			}
			else
			{
				if (_cacheCopyItem.children == null) return;
				Debug.Log(_cacheCopyItem.children.Count);

				foreach (var item in _cacheCopyItem.children)
				{
					if (item == null) continue;
					var data = m_TreeModel.GetData(item.id);
					if (data == null) continue;

					VSPropertyBase prop = data.obj as VSPropertyBase;
					if (prop == null) continue;
					vs.CopyProperty(prop, com);
				}
			}
		}


		// Rename
		//--------

		protected override bool CanRename(TreeViewItem item)
		{
			// Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
			Rect renameRect = GetRenameRect(treeViewRect, 0, item);
			return renameRect.width > 30;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
			Debug.Log("Rename ended");
			// Set the backend name and reload the tree to reflect the new model
			if (args.acceptedRename)
			{
				//var element = m_TreeModel.Find(args.itemID);
				//element.name = args.newName;
				Reload();
			}
		}

		protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
		{
			Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
			CenterRectUsingSingleLineHeight(ref cellRect);
			return base.GetRenameRect(cellRect, row, item);
		}

		// Misc
		//--------

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return true;
		}


	}
}
