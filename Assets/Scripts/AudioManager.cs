using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundEffect
{
    put
}

public class AudioManager : MonoBehaviour
{
    [Header("AudioClip")]
    public AudioSource BGMAudioSource;
    public AudioSource SoundEffectAudioSource;

    [Header("AudioSource")]
    public List<AudioClip> BGMs = new List<AudioClip>();
    public List<AudioClip> SoundEffects = new List<AudioClip>();

    void Start()
    {
        SoundEffectAudioSource.loop = false;
    }

    public void PlaySoundEffect(SoundEffect sound)
    {
        SoundEffectAudioSource.Stop();
        SoundEffectAudioSource.clip = SoundEffects[(int)sound];
        SoundEffectAudioSource.Play();
    }

    public void PlayBGM(int soundIndex, bool isLoop)
    {
        BGMAudioSource.Stop();
        BGMAudioSource.clip = BGMs[soundIndex];
        BGMAudioSource.loop = isLoop;
        BGMAudioSource.Play();
    }
}
