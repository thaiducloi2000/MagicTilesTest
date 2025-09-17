using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class CollectorBase : ScriptableObject
{
    public virtual void Refresh()
    {
    }
}

public class CollectorT<T> : CollectorBase
{
    [NonSerialized] public string[] cacheIds;

    // cache
    [NonSerialized] protected Dictionary<string, T> cacheMap;
    [SerializeField] public List<T> list = new List<T>();
	
    protected virtual string GetId(T item)
	{
        return item?.ToString();
    }
	
    public override void Refresh()
    {
        cacheMap = new Dictionary<string, T>();

        for (var i = 0; i < list.Count; i++)
        {
            var m = list[i];
            if (m == null)
            {
                Debug.LogWarning(this + " Asset is null at: " + i);
                continue;
            }

            var id = GetId(m);
            if (cacheMap.ContainsKey(id))
            {
                Debug.LogWarning(this + " Duplicated asset with id: " + id);
                continue;
            }

            cacheMap.Add(id, m);
        }

        list.Sort((t1, t2) => { return GetId(t1).CompareTo(GetId(t2)); });

        cacheIds = cacheMap.Keys.ToArray();
        Array.Sort(cacheIds);
    }

    public int IndexOf(string id)
    {
        if (string.IsNullOrEmpty(id)) return -1;
        if (cacheMap == null || cacheIds == null || list == null || cacheMap.Count != list.Count) Refresh();
        if (!cacheMap.ContainsKey(id)) return -1; // O(1)
        return Array.IndexOf(cacheIds, id); // O(N) ? should we store index instead of value?
    }

    public string GetIdAt(int index)
    {
        if (cacheMap == null) Refresh();
        return cacheIds[index];
    }

    public T Get(string id)
    {
        if (cacheMap == null) Refresh();

        T result;
        if (cacheMap.TryGetValue(id, out result)) return result;
        return default(T);
    }


#if UNITY_EDITOR
    public bool DrawPicker(ref int selected)
    {
        if (cacheIds == null) Refresh();

        if (cacheIds.Length == 0)
        {
            EditorGUILayout.HelpBox("List is empty!", MessageType.Warning);
            return false;
        }

        var v1 = Mathf.Clamp(selected, 0, selected);
        var v2 = EditorGUILayout.Popup(v1, cacheIds);

        if (v2 != selected)
        {
            selected = v2;
            return true;
        }

        return false;
    }

#endif
}

public class AssetCollectorT<T> : CollectorT<T> where T : UnityObject
{
	protected override string GetId(T item)
	{
		return item?.name;
	}
}

#if UNITY_EDITOR
public class CollectorEditorT<T> : Editor
{
    int selected;
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var c = ((CollectorT<T>)target);

        if (c.DrawPicker(ref selected))
        {
            Debug.Log("Changed: " + c.cacheIds[selected]);
        }
    }
}
#endif