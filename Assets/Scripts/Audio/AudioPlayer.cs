using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    
    public static AudioPlayer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip audioClip)
    {
        _audioSource.PlayOneShot(audioClip);
    }
}
