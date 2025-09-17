using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundDatabaseConfig _soundDatabase = default;
    [SerializeField] private AudioSource _audioSourceBG = default;
    [SerializeField] private AudioSource _audioSourceAmbient = default;
    [SerializeField] private SoundEmitter _soundEmitPrefab = default;

    [ShowInInspector] private Dictionary<SoundName, List<SoundEmitter>> DctSoundCache = default;


    public static SoundManager Instance = default;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        DctSoundCache = new Dictionary<SoundName, List<SoundEmitter>>();
    }

    public void PlayBGMusic(SoundName name, float volume = 1f)
    {
        //if (!PlayerDataManager.Instance.Common.Data.MusicEnabled)
        //    return;
        
        var soundData = _soundDatabase.GetBGMusicByName(name);
        if (soundData == null)
        {
            Debug.LogWarning("Can't found audio name: " + name);
            return;
        }

        if (_audioSourceBG.isPlaying)
        {
            //_audioSourceBG.Stop();
            _audioSourceBG.DOFade(0, 1f).OnComplete(() =>
            {
                _audioSourceBG.clip = soundData.Clip;
                _audioSourceBG.loop = soundData.Loop;
                _audioSourceBG.volume = 0;
                _audioSourceBG.pitch = 1;
                _audioSourceBG.pitch += soundData.RandomPitch ? Random.Range(-0.05f, 0.05f) : 0;
        
        
                _audioSourceBG.DOFade(soundData.Volume, 3f);
                _audioSourceBG.Play();
            });
        }
        else
        {
            _audioSourceBG.clip = soundData.Clip;
            _audioSourceBG.loop = soundData.Loop;
            _audioSourceBG.volume = 0;
            _audioSourceBG.pitch = 1;
            _audioSourceBG.pitch += soundData.RandomPitch ? Random.Range(-0.05f, 0.05f) : 0;
        
        
            _audioSourceBG.DOFade(soundData.Volume, 3f);
            _audioSourceBG.Play();
        }
    }
    
    public void StopBGMusic()
    {
        if (_audioSourceBG.isPlaying)
        {
            _audioSourceBG.Stop();
        }
    }

    //[Button]
    //public void PlayBGMusic()
    //{
    //    //if (!PlayerDataManager.Instance.Common.Data.MusicEnabled)
    //    //    return;

    //    var currentScene = SceneManager.GetActiveScene().name;
    //    if (currentScene == SceneName.SceneMain.ToString())
    //    {
    //        PlayBGMusic(SoundName.BGM_Lobby);
    //    }
        
    //    if (currentScene == SceneName.SceneGame.ToString())
    //    {
    //        PlayBGMusic(SoundName.BGM_Battle);
    //    }

    //    if (currentScene == SceneName.SceneTutorial.ToString())
    //    {
    //        PlayBGMusic(SoundName.BGM_Battle);
    //    }
    //}

    public void PlayAmbientSound(SoundName soundName)
    {
        //if (!PlayerDataManager.Instance.Common.Data.MusicEnabled)
        //    return;
        
        var soundData = _soundDatabase.GetAmbientByName(soundName);
        if (soundData == null)
        {
            Debug.LogWarning("Can't found audio name: " + name);
            return;
        }

        if (_audioSourceAmbient.isPlaying)
        {
            //_audioSourceBG.Stop();
            _audioSourceAmbient.DOFade(0, 1f).OnComplete(() =>
            {
                _audioSourceAmbient.clip = soundData.Clip;
                _audioSourceAmbient.loop = soundData.Loop;
                _audioSourceAmbient.volume = 0;
                _audioSourceAmbient.pitch = 1;
                _audioSourceAmbient.pitch += soundData.RandomPitch ? Random.Range(-0.05f, 0.05f) : 0;
        
        
                _audioSourceAmbient.DOFade(soundData.Volume, 3f);
                _audioSourceAmbient.Play();
            });
        }
        else
        {
            _audioSourceAmbient.clip = soundData.Clip;
            _audioSourceAmbient.loop = soundData.Loop;
            _audioSourceAmbient.volume = 0;
            _audioSourceAmbient.pitch = 1;
            _audioSourceAmbient.pitch += soundData.RandomPitch ? Random.Range(-0.05f, 0.05f) : 0;
        
        
            _audioSourceAmbient.DOFade(soundData.Volume, 3f);
            _audioSourceAmbient.Play();
        }
    }
    
    public void StopAmbient()
    {
        if (_audioSourceAmbient.isPlaying)
        {
            _audioSourceAmbient.Stop();
        }
    }

    public SoundEmitter PlaySoundFX(SoundName soundName)
    {
        //if (!PlayerDataManager.Instance.Common.Data.SoundEnabled)
        //    return null;
        
        var soundEmitter = GetSoundEmitter(soundName);
        if (soundEmitter == null)
            return null;

        var soundData = _soundDatabase.GetVFXByName(soundName);
        if (soundData == null)
        {
            Debug.LogWarning("Can't found audio name: " + name);
            return null;
        }

        if (CanPlaySound(soundName, soundData) == false)
            return null;

        soundEmitter.Initialize(soundData);
        soundEmitter.Play();

        return soundEmitter;
    }

    public void PauseAllSoundFxLoop()
    {
        //if (!PlayerDataManager.Instance.Common.Data.SoundEnabled)
        //    return;

        foreach (var soundEmitter in DctSoundCache)
        {
            var lstEmitter = soundEmitter.Value;
            foreach (var emitter in lstEmitter)
            {
                if (emitter.SoundData.Loop && emitter.IsPlaying)
                {
                    emitter.Pause();
                }
            }
        }
    }
    
    public void StopAllSoundFx()
    {
        //if (!PlayerDataManager.Instance.Common.Data.SoundEnabled)
        //    return;

        foreach (var soundEmitter in DctSoundCache)
        {
            var lstEmitter = soundEmitter.Value;
            foreach (var emitter in lstEmitter)
            {
                emitter.Stop();
            }
        }
    }

    public void UnPauseAllSoundFX()
    {
        //if (!PlayerDataManager.Instance.Common.Data.SoundEnabled)
        //    return;

        foreach (var soundEmitter in DctSoundCache)
        {
            var lstEmitter = soundEmitter.Value;
            foreach (var emitter in lstEmitter)
            {
                emitter.UnPause();
            }
        }
    }

    private SoundEmitter GetSoundEmitter(SoundName soundName)
    {
        if (DctSoundCache.TryGetValue(soundName, out var lstSoundEmitter))
        {
            foreach (var item in lstSoundEmitter)
            {
                if (item.IsPlaying == false)
                    return item;
            }

            var soundEmitter = Instantiate(_soundEmitPrefab, transform);
            DctSoundCache[soundName].Add(soundEmitter);
            return soundEmitter;
        }
        else
        {
            var soundEmitter = Instantiate(_soundEmitPrefab, transform);
            DctSoundCache[soundName] = new List<SoundEmitter>();
            DctSoundCache[soundName].Add(soundEmitter);
            return soundEmitter;
        }
    }

    private bool CanPlaySound(SoundName soundName, SoundData soundData)
    {
        if (soundData.Frequent == false)
            return true;

        if (DctSoundCache.TryGetValue(soundName, out var lstEmitter))
        {
            var countSoundPlaying = 0;
            foreach (var emitter in lstEmitter)
            {
                if (emitter.IsPlaying)
                    countSoundPlaying += 1;
            }

            return countSoundPlaying < soundData.AmountFrequent;
        }
        else
        {
            return true;
        }
    }
}