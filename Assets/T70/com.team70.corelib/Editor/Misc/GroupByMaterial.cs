using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GroupByMaterial
{
    [MenuItem("T70/Tools/Group by Material", false, 111)]
    public static void Apply()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("Must select 1 GameObject in the scene");
            return;
        }

        var singleMap = new Dictionary<Material, List<Transform>>();
        var multiMap = new List<Transform>();
        Collect(go.transform, singleMap, multiMap);
        
        // Create others
        CreateAndAdd(go.transform, "others", multiMap);
        foreach (var kvp in singleMap)
        {
            CreateAndAdd(go.transform, kvp.Key.name, kvp.Value);
        }
    }
    
    static void CreateAndAdd(Transform root, string gName, List<Transform> list)
    {
        var c = new GameObject(){ name = gName };
        c.transform.SetParent(root, false);

        for (int i = 0; i < list.Count; i++)
        {
        	list[i].transform.SetParent(c.transform, true);
        }
    }
    
    static void Collect(Transform t, Dictionary<Material, List<Transform>> sMap, List<Transform> mMap)
    {
        var r = t.gameObject.GetComponent<MeshRenderer>();
        if (r != null)
        {
            if (r.sharedMaterials.Length != 1)
            {
                mMap.Add(t);
            }
            else
            {
                var m = r.sharedMaterial;
                List<Transform> list;
                if (!sMap.TryGetValue(m, out list))
                {
                    list = new List<Transform>();
                    sMap.Add(m, list);
                }
                list.Add(t);
            }
        }

        if (t.childCount == 0) return;
        foreach (Transform c in t)
        {
            if (c == t) continue;
            Collect(c, sMap, mMap);
        }
    }
}
