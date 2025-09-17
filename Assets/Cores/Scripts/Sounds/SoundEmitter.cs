using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource = default;
    [field: SerializeField] public SoundData SoundData { get; private set; }
    public bool IsPlaying => _audioSource.isPlaying;

    public void Initialize(SoundData soundData)
    {
        SoundData = soundData;
        _audioSource.clip = soundData.Clip;
        _audioSource.loop = soundData.Loop;
        _audioSource.volume = soundData.Volume;
        _audioSource.pitch = soundData.Pitch;
        _audioSource.pitch += soundData.RandomPitch ? Random.Range(soundData.MinPitch, soundData.MaxPitch) : 0;
    }

    public void Play()
    {
        _audioSource.Play();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    public void UnPause()
    {
        _audioSource.UnPause();
    }
    
    
}
