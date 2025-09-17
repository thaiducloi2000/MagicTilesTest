using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using Object = UnityEngine.Object;

namespace nano.vs2
{
    public class VSEditor : IColumnAdapter
    {
        private VSTreeModel treeModel;
        private VSTreeViewMultipleColumn m_TreeView;
        private TreeViewState m_TreeViewState;
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        VisualState visualState;
        private VisualStateEditorWindow parentWindow;
        public int nStates { get { return visualState.nStates; } }
        public int State { get { return visualState.state;  } set { visualState.SetState(value); } }

        public List<AmountData> BlemData
        {
            get { return visualState.getAmounts(); }
        }
        private bool _lockTarget = false;
        public bool lockTarget
        {
            get { return _lockTarget; }
            set
            {
                _lockTarget = value; if (!_lockTarget) parentWindow.OnSelectionChange();
            }
        }
        private float xScroll;
        //int fromState = 0;
        //int toState = 1;
        float amount = 0;
        public bool isValueTab = true;
        [SerializeField] private bool m_Initialized = false;

        private float treeViewPosY = 0;

        public VisualState GetVisualState()
        {
            return visualState;
        }
        public VisualState vs { get { return GetVisualState(); } }
        internal VSEditor(VisualState vs, VisualStateEditorWindow parentWindow)
        {
            this.parentWindow = parentWindow;
            visualState = vs;
            InitIfNeeded();
            visualState.OnDataStructChanged += OnDataStructChange;
            visualState.OnDataChanged += parentWindow.Repaint;
            parentWindow.Repaint();
        }
        internal void OnDataStructChange()
        {
            //current: reinit all
            m_Initialized = false;
            //m_TreeViewState = null;
            InitIfNeeded();
            parentWindow.Repaint();
        }

        void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();


                //var headerState = VSTreeViewMultipleColumn.CreateDefaultMultiColumnHeaderState(parentWindow.position.width);
                //if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                //    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                //m_MultiColumnHeaderState = headerState;



                treeModel = new VSTreeModel(GetData()); 

                m_MultiColumnHeaderState = InitColumn();
                var multiColumnHeader = new VSColumnHeader(m_MultiColumnHeaderState, this);
                m_TreeView = new VSTreeViewMultipleColumn(m_TreeViewState, multiColumnHeader, treeModel, this);
                
                m_Initialized = true;
            }
        }
        public void FitToView()
        {
            var columns = m_MultiColumnHeaderState.columns;
            float firstWidth = columns[0].width;
            var windowWidth = parentWindow.position.width;
            var elementSize = (windowWidth - firstWidth) / (columns.Length - 3);
            for (int i = 1; i < columns.Length - 2; i++)
            {
                columns[i].width = elementSize;
            }
        }
        private MultiColumnHeaderState InitColumn()
        {
            var columns = new List<MultiColumnHeaderState.Column>
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(""),
                    // contextMenuText = "Poperties",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = VSStyle.PROPERTY_SIZE,
                    minWidth = VSStyle.PROPERTY_MIN_SIZE,
                    maxWidth = VSStyle.PROPERTY_MAX_SIZE,
                    autoResize = true,
                    
                    
                    allowToggleVisibility = false
                }
            };
            for(int i = 0; i < visualState.nStates; i++)
            {
                var col = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(visualState.stateNames[i]),
                    // contextMenuText = "RemoveState",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 120,
                    minWidth = 30,
                    maxWidth = 1000,
                    autoResize = false,
                    allowToggleVisibility = false
                };
                columns.Add(col);
            }

            columns.Add
            (
                new MultiColumnHeaderState.Column
                {
                    headerContent = VSStyle.addStateContent,
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = VSStyle.ADD_STATE_BUTTON_WIDTH,
                    minWidth = VSStyle.ADD_STATE_BUTTON_WIDTH,
                    maxWidth = VSStyle.ADD_STATE_BUTTON_WIDTH,
                    autoResize = false,
                    
                    allowToggleVisibility = false
                }
            );

            columns.Add
            (
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Min/Max"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 100,
                    autoResize = false,

                    allowToggleVisibility = false
                }
            );

            var state = new MultiColumnHeaderState(columns.ToArray());
            return state;
        }
        List<TreeItemData> GetData()
        {
            var allItems = new List<TreeItemData>();

            var root = (new TreeItemData { id = -1, depth = -1, displayName = "" });
            allItems.Add(root);
            int generateId = 1;
            foreach (var item in visualState.listValue)
            {
                var data = new TreeItemData
                {
                    depth = 0,
                    obj = item,
                    isProperty = true,
                    id = generateId,
                    displayName = item.property
                };
                allItems.Add(data);

                generateId++;
            }
            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            return allItems;
        }
        List<TreeItemData> GetDataVer1()
        {
            var allItems = new List<TreeItemData>();

            var root = (new TreeItemData { id = 0, depth = -1, displayName = "" });
            allItems.Add(root);
            int generateId = 1;
            foreach (var item in visualState.group)
            {
                var data = new TreeItemData
                {
                    depth = 0,
                    obj = item.Value.gameObject,
                    isProperty = false,
                    id = generateId,
                    displayName = item.Value.gameObject.name
                };
                allItems.Add(data);

                generateId++;

                foreach (var item1 in item.Value.lst)
                {
                    if (data.children == null) data.children = new List<TreeViewItem>();
                    var data1 = new TreeItemData
                    {
                        depth = 1,
                        obj = item1,
                        isProperty = true,
                        id = generateId,
                        displayName = item1.property
                    };
                    allItems.Add(data1);
                    generateId++;
                }
            }
            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            return allItems;
        }

       



        internal void OnAnimEditorGUI(Rect position)
        {
            if (m_TreeView == null) return;
            if (m_MultiColumnHeaderState == null) return;
			var option = visualState.stateNames.ToArray();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var togle = GUILayout.Toggle(visualState.isRecording, VSStyle.recordContent, EditorStyles.toolbarButton, GUILayout.Width(30));
                if (togle != visualState.isRecording)
                {
                    if (togle) visualState.StartRecord();
                    else visualState.StopRecord();
                }
                vs.useAnim = GUILayout.Toggle(vs.useAnim, VSStyle.AnimateContent, EditorStyles.toolbarButton);

                GUILayout.FlexibleSpace();
                var curSpeed = 1f / visualState.animTime;
                var speed = EditorGUILayout.Slider("Speed", curSpeed, 0.1f, 5 );
                if(speed != curSpeed)
                {
                    visualState.animTime = 1f / speed;
                }
                GUILayout.Space(5);

                var guiContent = new GUIContent(VSStyle.playContent);
                if(visualState.animTime <= 0)
                {
                    guiContent.tooltip = Setting.PLAY_BUTTON_TOOLTIP;
                    GUI.enabled = false;
                }

                var tween = (GUILayout.Toggle(isTweening, guiContent, EditorStyles.toolbarButton, GUILayout.Width(30)));
                if(tween != isTweening)
                {
                    if (tween)
                        PlayState();
                    else
                        StopPlay();
                }
                if(visualState.animTime <= 0)
                {
                    GUI.enabled = true;
                }
                var m_amount = EditorGUILayout.Slider(amount, 0, 1);
                if (m_amount.Equals(amount) == false)
                {
                    visualState.SetAmountForAll(m_amount);
                    amount = m_amount;
                }
            }
            EditorGUILayout.EndHorizontal();

            var h = position.height - (EditorGUIUtility.singleLineHeight * 2.5f);
            var treeRect = EditorGUILayout.GetControlRect(false, h);
            treeViewPosY = treeRect.y;
            m_TreeView.OnGUI(treeRect);

            DrawFooter();

            // EditorUtility.SetDirty(visualState);

            ProcessMouseClick();
        }
        private Vector2 mouseDownRect;
        public void SetScrollX(float x)
        {
            xScroll = x;
        }
        private void ProcessMouseClick()
        {
            if(Event.current == null) return;
            var current = Event.current;
            if(current.type == EventType. MouseDown) mouseDownRect = current.mousePosition;
            else if(current.type == EventType.MouseUp)
            {
                // Debug.Log("click: " + current.mousePosition);
                var header = m_TreeView.multiColumnHeader;
                var cols = m_MultiColumnHeaderState.columns;
                if (header == null || cols == null) return;
                for(int i =0; i < cols.Length; i++)
                {
                    if(!header.IsColumnVisible(i)) continue;
                    var r = header.GetColumnRect(i);
                    r.y += treeViewPosY;
                    r.x -= xScroll;
                    if(r.Contains(current.mousePosition) && r.Contains(mouseDownRect))
                    {
                        SelectColumn(i, current.button == 0 );
                    }

                    
                }
            }

        }
        public int stateClicked{get{
            return cacheState;
        }}
        private int cacheState = -1;
        public const int ColumnMinMax = -3;
        public int getStateIndexWithColumn(int columnIndex)
        {
            if (columnIndex == 0) return -1;
            if (columnIndex > visualState.nStates + 1) return ColumnMinMax;
            if (columnIndex > visualState.nStates) return -2;
            return columnIndex - 1;
        }
        private void SelectColumn(int index, bool isLeftMouse)
        {
            cacheState = getStateIndexWithColumn(index);
            if(!isLeftMouse)
            {
                return;
            }
            if (index == 0) return;

            if(index == (visualState.nStates + 1))
            {
                AddState(visualState.state);
            }


            if (index > visualState.nStates ) return;
            visualState.OnUpdateChange.AddListener(OnVSUpdate);
            visualState.OnAfterChange.AddListener(OnVSComplete);
            visualState.SetState(index - 1);
            parentWindow.Repaint();
        }

        private void OnVSComplete()
        {
            amount = vs.state * 1f / (vs.nStates - 1);
            var amounts = visualState.getAmounts();
            for (int i =0;  i< amounts.Count; i++)
            {
                var a = amounts[i];
                a.amount = i == vs.state ? 1 : 0;
                amounts[i] = a;
            }
            visualState.OnUpdateChange.RemoveListener(OnVSUpdate);
            visualState.OnAfterChange.RemoveListener(OnVSComplete);
        }

        private void OnVSUpdate()
        {
            parentWindow.Repaint();
        }

        public void AddState(int cloneIndex)
        {
            if(cloneIndex < 0)
            {
                Debug.LogWarning("Invalid clone state " + cloneIndex);
                return;
            }
            // Debug.Log("add state " + cloneIndex);
            visualState.AddState(cloneIndex);
        }
        public void RemoveState(int state) 
        {
             if(state < 0)
            {
                Debug.LogWarning("Invalid state " + state);
                return;
            }
            // Debug.Log("remove state " + state);

            visualState.RemoveState(state);
        }
        public void RemoveProperty(VSPropertyBase prop)
        {
            visualState.RemoveProperty(prop);
        }
        private void OnSelectState(object val)
        {
            int state = (int) val;
            if(state == -1)//add state
            {
                // Debug.Log("add state");
                visualState.AddState(visualState.state);
            }
            else if(state == -2)//remove state
            {
                Debug.Log("remove state");
            }
            else if(state != visualState.state)
				 {
					 visualState.SetState(state);
				 }
        }

        private void DrawFooter()
        {
            var width = Setting.GetPropertyWidth(parentWindow.position.width) - 5;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                EditorGUILayout.LabelField("Group", GUILayout.Width(70)) ;
                var group = (GroupType)EditorGUILayout.EnumPopup(Setting.groupType, EditorStyles.toolbarDropDown);
                if(group != Setting.groupType)
                {
                    Setting.groupType = group;OnDataStructChange();
                }
                if(GUILayout.Button("Fit to view", EditorStyles.toolbarDropDown))
                {
                    FitToView();
                }

                GUILayout.FlexibleSpace();


    //           if(GUILayout.Toggle(isValueTab,"Values", EditorStyles.toolbarButton, GUILayout.Width(width /2)))
    //           {
    //               isValueTab = true;
    //           }
				//if(GUILayout.Toggle(!isValueTab,"Curves", EditorStyles.toolbarButton, GUILayout.Width(width /2)))
     //           {
     //               isValueTab = false;
     //           }
				 //EditorGUILayout.Separator();


            }
            EditorGUILayout.EndHorizontal();
        }
        private bool isTweening = false;
        private float countTime = 0;
        private double lastTimeUpadte;
        private void PlayState()
        {
            unRegistEvents();
            isTweening = true;
            countTime = 0;
            lastTimeUpadte = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnUpdate;
        }
        private void StopPlay()
        {

            unRegistEvents();
        }

        private void unRegistEvents()
        {
            // Debug.Log("complete");
            isTweening = false;
             EditorApplication.update -= OnUpdate;
        }

        private void OnUpdate()
        {
            countTime += (float)(EditorApplication.timeSinceStartup - lastTimeUpadte);
            lastTimeUpadte = EditorApplication.timeSinceStartup;


            amount = countTime / visualState.animTime;
            parentWindow.Repaint();

            if(amount >= 1) amount = 1;

            visualState.SetAmountForAll(amount);
            
            if(amount >= 1)
            {
                StopPlay();
            }
        }




        // private void PlayState()
        // {
        //     unRegistEvents();
        //     isTweening = true;
        //     visualState.OnUpdateChange.AddListener(OnUpDateChange);
        //     visualState.OnAfterChange.AddListener(unRegistEvents);
        //     visualState.EditorPlayState(fromState, toState);
        // }
        // private void StopPlay()
        // {

        //     unRegistEvents();
        // }

        // private void unRegistEvents()
        // {
        //     // Debug.Log("complete");
        //     isTweening = false;
        //     visualState.OnAfterChange.RemoveListener(unRegistEvents);
        //      visualState.OnUpdateChange.RemoveListener(OnUpDateChange);
        // }

        // private void OnUpDateChange()
        // {
        //     amount = visualState.cacheAmount;
        //     parentWindow.Repaint();

        // }
    }
}