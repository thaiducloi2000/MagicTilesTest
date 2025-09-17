using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Configs/SoundDatabase")]
public class SoundDatabaseConfig : SerializedScriptableObject
{
    [SerializeField] private Dictionary<SoundName, SoundData> DctBgMusic = default;
    [SerializeField] private Dictionary<SoundName, SoundData> DctFXMusic = default;
    [SerializeField] private Dictionary<SoundName, SoundData> DctAmbient = default;

    public SoundData GetBGMusicByName(SoundName name)
    {
        return DctBgMusic.TryGetValue(name, out var soundData) ? soundData : null;
    }

    public SoundData GetVFXByName(SoundName name)
    {
        return DctFXMusic.TryGetValue(name, out var soundData) ? soundData : null;
    }

    public SoundData GetAmbientByName(SoundName name)
    {
        return DctAmbient.TryGetValue(name, out var soundData) ? soundData : null;
    }

/*    [Button]
    private void InitSoundNameVFX()
    {
        DctFXMusic.Clear();
        foreach (SoundName suit in (SoundName[]) Enum.GetValues(typeof(SoundName)))
        {
            if(suit == SoundName.BGM_Lobby || suit == SoundName.BGM_Stage || suit == SoundName.BGM_Title)
                continue;
            
            DctFXMusic[suit] = new SoundData();
        }
    }*/
}

[Serializable]
public class SoundData
{
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
    public bool Loop;
    public bool RandomPitch = false;
    [ShowIf("RandomPitch")]
    public float MinPitch = -0.02f;
    [ShowIf("RandomPitch")]
    public float MaxPitch = 0.02f;

    [Range(0, 2f)] public float Pitch = 1f;
    
    public bool Frequent = false;
    
    [ShowIf("Frequent")]
    public int AmountFrequent = 15;
}

public enum SoundName
{
    BGM_Test = 0,
    
    None = 999,
}