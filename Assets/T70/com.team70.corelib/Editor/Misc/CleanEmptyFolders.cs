using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CleanEmptyFolders
{
    [MenuItem("T70/Tools/Clean Empty Folders", false, 111)]
    public static void CleanEmptyFolder()
    {
        var listDelete = new List<string>();
        GetEmptyFolder("Assets/", listDelete);

        if (listDelete.Count == 0)
        {
            Debug.Log("There are no empty folder!");
            return;
        }
        
        Debug.Log("Deleted [" + listDelete.Count + "] empty folders!\n\n" + string.Join("\n", listDelete.ToArray()));

        AssetDatabase.StartAssetEditing();
        {
            for (int i =0;i < listDelete.Count; i++)
            {
                var p = listDelete[i];
                AssetDatabase.DeleteAsset(p);
            }
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }
    
    public static bool GetEmptyFolder(string path, List<string> deleteList)
    {
        path = path.Replace("\\", "/");
        if (path.Contains("/.") || path.Contains("/~"))
        {
            return false; // ignore folders starts with .
        }
        
        var isEmpty = true;
        var subs = Directory.GetDirectories(path); // must get at once!
        foreach (var d in subs)
        {
            if (false == GetEmptyFolder(d, deleteList))
            {
                isEmpty = false;
            }
        }
        
        if (isEmpty)
        {
            var files = Directory.GetFiles(path);
            
            foreach (var f in files)
            {
                if (f.EndsWith(".meta")) continue;
                if (f.ToUpper().EndsWith(".DS_STORE")) continue;

                // Found a file:
                // Debug.Log(path + " --> " + files.Length + " : " + subs.Length + ":" + string.Join("\n", files));
                return false;
            }
            
            deleteList.Add(path);
        }
        
        return isEmpty;
    }
}