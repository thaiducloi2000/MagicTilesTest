using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace nano.vs2
{
    public class VSTreeview : TreeView
    {
        protected VSTreeModel m_TreeModel;
        protected readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);
        public VSTreeview(TreeViewState state, VSTreeModel model) : base(state)
        {
            Init(model);
        }

        public VSTreeview(TreeViewState state, MultiColumnHeader multiColumnHeader, VSTreeModel model) : base(state, multiColumnHeader)
        {
            Init(model);
        }
        void Init(VSTreeModel model)
        {
            m_TreeModel = model;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem(m_TreeModel.root.id, -1, m_TreeModel.root.displayName); ;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (m_TreeModel.root == null)
            {
                Debug.LogError("tree model root is null. did you call SetData()?");
            }

            m_Rows.Clear();
            //if (!string.IsNullOrEmpty(searchString))
            //{
            //    Search(m_TreeModel.root, searchString, m_Rows);
            //}
            //else
            {
                if (m_TreeModel.root.hasChildren)
                    AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
            }

            // We still need to setup the child parent information for the rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths(root, m_Rows);

            return m_Rows;
        }

        void AddChildrenRecursive(TreeViewItem parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (TreeViewItem child in parent.children)
            {
                if (child == null) continue;
                var item = new TreeViewItem(child.id, depth, child.displayName);
                newRows.Add(item);

                if (child.hasChildren)
                {
                    if (IsExpanded(child.id))
                    {
                        AddChildrenRecursive(child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }


    }
}

