using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace nano.vs2
{
    //public class VSTreeViewItem : TreeViewIem
    // {
        
    // }
    public class TreeItemModel{

    }
    public class VSTreeModel
    {
        public TreeViewItem root;
        public List<TreeItemData> m_Data;
        int m_MaxID;
        public VSTreeModel(List<TreeItemData> data)
        {
            SetData(data);
        }

        public void SetData(List<TreeItemData> data)
        {
            Init(data);
        }

        void Init(List<TreeItemData> data)
        {
            if (data == null)
                throw new ArgumentNullException("data", "Input data is null. Ensure input is a non-null list.");

            m_Data = data;
            if (m_Data.Count > 0)
                root = TreeElementUtility.ListToTree(data);
            if (m_Data.Count > 0)
                m_MaxID = m_Data.Max(e => e.id);
            else m_MaxID = 1;

            dictData = new Dictionary<int, TreeItemData>();
            foreach(var item in data)
            {
                if(dictData.ContainsKey(item.id))
                {
                    Debug.LogWarning("duplicated item id : " + item.id + "  " + item.displayName);
                    continue;
                }
                dictData.Add(item.id, item);
            }
        }
        public TreeItemData GetData(int id)
        {
            if (dictData == null) return null;
            TreeItemData data = null;
            dictData.TryGetValue(id, out data);
            return data;
        }
        private Dictionary<int, TreeItemData> dictData;

        public int GenerateUniqueID()
        {
            return ++m_MaxID;
        }
    }
}


