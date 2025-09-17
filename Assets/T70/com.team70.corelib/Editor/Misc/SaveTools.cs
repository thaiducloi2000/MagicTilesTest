using System.IO;
using UnityEditor;
using UnityEngine;
using com.team70;

public class SaveTools
{
    [MenuItem("T70/Data/Open Persistent DataPath", false, 100)]
    public static void Util_OpenPersistentDataPath()
    {
        string path = T70.GetPersistentPath("T70", "", true);
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("T70/Data/Delete Save File", false, 100)]
    public static void Util_DeleteSaveFile()
    {
        string path = T70.GetPersistentPath("T70", "", false);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Debug.Log("Delete file success");
        }
    }
    
    [MenuItem("T70/Data/Delete UserInfo", false, 100)]
    public static void Util_DeleteDatFile()
    {
        string path = T70.GetPersistentPath("T70", "", false);
        if (Directory.Exists(path))
        {
            File.Delete(path + "/UserInfo.dat");
            Debug.Log("Delete file success");
        }
    }
}