using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine.SceneManagement;
using System.Linq;

namespace nano.vs2
{
    public class DuplicatePropertiesWindow : EditorWindow
    {
        Action<Component> SelectedValue;
        Component ComCopy;
        Transform root;
        public static void ShowWindow(Action<Component> callback, Component com, Transform root)
        {
            var win = GetWindow<DuplicatePropertiesWindow>(true, "Select Target", true);
            win.SelectedValue = callback;
            win.ComCopy = com;
            win.root = root;
            win.Init();
            win.ShowUtility();
        }
        ComponentTreeView treeView;
        TreeViewState state;

        public void OnSelected(Component com)
        {
            if(SelectedValue != null)
            {
                SelectedValue(com);
            }
            Close();
        }

        public void Init()
        {
                state = new TreeViewState();
                treeView = new ComponentTreeView(state, OnSelected, ComCopy, root);

        }
        private void OnGUI()
        {
            if (treeView == null) return;
            var rect = EditorGUILayout.GetControlRect(false, position.height);
            treeView.OnGUI(rect);
            if(treeView.GetRows().Count == 0)
            {
                EditorGUILayout.LabelField("No target to duplicate!");
            }
        }

    }


    public class ComponentTreeView : TreeView
    {
        Action<Component> SelectedValue;
        Component ComCopy;
        Transform rootTf;
        public ComponentTreeView(TreeViewState state, Action<Component> callback, Component com, Transform root) : base(state)
        {
            SelectedValue = callback;
            ComCopy = com;
            this.rootTf = root;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {

            return new TreeViewItem(0, -1, "root");
        }
        private int count = 0;
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            count = 0;
            var rows = GetRows() ?? new List<TreeViewItem>(200);
            if (ComCopy == null || root == null) return rows;

            // We use the GameObject instanceIDs as ids for items as we want to 
            // select the game objects and not the transform components.
            rows.Clear();
            var t = ComCopy.GetType();
            var depth = 0;

            appendChild(rootTf, t, rows, ref depth);
            foreach (var item in getAllChild(rootTf, depth))
            {
                var d = item.childDepth;
                appendChild(item.transform, t, rows, ref d);
                //Debug.Log(item.transform+"\t"+item.childDepth+ "  " + curChildDepth);
            }
            //if (rows.Count > 0)
            //{
            //    var lastDepth = rows[0].depth;
            //    for (int i = 1; i < rows.Count; i++)
            //    {
            //        if (rows[i].depth - lastDepth > 1)
            //        {
            //            rows[i].depth = ++lastDepth;
            //        }
            //        else if (rows[i].depth < lastDepth)
            //        {
            //            lastDepth = rows[i].depth;
            //        }

            //    }

            //}

            SetupDepthsFromParentsAndChildren(root);
            //SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            //base.RowGUI(args);
            //return;
            var item = args.item as TreeItem;
            if(item == null)
            {
                Debug.LogWarning("something wrong data null");
                return;
            }
            const float width = 20;
            var rowRect = args.rowRect;
            var r = rowRect;
            var indent = VSStyle.INDENT_SPACE * item.depth;
            r.x += indent;
            r.width -= indent + width;
            EditorGUI.ObjectField(r, item.com, typeof(Component), true);

            var btnR = new Rect(rowRect.x + rowRect.width - width, rowRect.y, width, rowRect.height);
            if(GUI.Button(btnR, VSStyle.duplicateTargetButtonContent, GUIStyle.none))
            {
                if(SelectedValue != null)
                {
                    SelectedValue(item.com);
                }
            }
            count++;
        }
        private void appendChild(Transform item, Type t, IList<TreeViewItem> rows, ref int depth)
        {
            var allCom = item.GetComponents<Component>();
            if (allCom.Length == 0) return;
            var c = allCom.Where(x => x.GetType() == t && x != ComCopy);

            if (c == null) return;
            var coms = c.ToList();

            if (coms.Count == 0) return;
            for(int i = 0; i < coms.Count; i++)
            {
                var com = coms[i];
                rows.Add(new TreeItem(com.GetInstanceID(), depth, com.name, com));
            }
            depth++;
        }
        public class TransformData
        {
            public Transform transform;
            public int childDepth;
            public TransformData(Transform transform, int childDepth)
            {
                this.transform = transform;
                this.childDepth = childDepth;
            }
        }
        IEnumerable<TransformData> getAllChild(Transform root, int depth)
        {
            if(root.childCount > 0)
            {

                for(int i = 0; i < root.childCount; i++)
                {
                    var c = root.GetChild(i);
                    yield return new TransformData( c, depth);
                    foreach(var child in getAllChild(c, depth + 1))
                    {
                        yield return child;
                    }
                }
            }
        }

    }

    public class TreeItem : TreeViewItem
    {
        public Component com;
        public TreeItem(int id, int depth, string displayName, Component com):base(id, depth, displayName)
            {
            this.com = com;
        }

    }

}

