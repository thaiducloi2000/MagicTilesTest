using System;
using com.team70;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "sound collection.asset", menuName = "T70 Collection/Sound", order = 110)]
public class SoundCollector : AssetCollectorT<AudioClip>
{
    public AudioClip GetRandom()
    {
        if (list.Count == 0)
        {
            Debug.LogWarning("Sound Collector null");
            return null;
        }

        return list[Random.Range(0, list.Count)];
    }
    
#if UNITY_EDITOR
    [ContextMenu("BuildStaticStringClass")]
    public void BuildStaticStringClass()
    {
        var filePath = AssetDatabase.GetAssetPath(this);
        filePath = filePath.Remove(filePath.LastIndexOf("/", StringComparison.Ordinal));
        filePath += "/" + $"{this.name}.string.asset";
        Debug.Log(filePath);
        T70.CreateStringCollector(filePath, list.ToArray());
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SoundCollector))]
public class SoundCollectorEditor : CollectorEditorT<AudioClip>
{
}
#endif