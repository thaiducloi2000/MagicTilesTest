using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundKeyAnimationHelper : MonoBehaviour
{
    public void PlaySound(SoundName soundName)
    {
        SoundManager.Instance.PlaySoundFX(soundName);
    }
}
