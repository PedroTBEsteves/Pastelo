using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class DialogueWriter
{
    private readonly float _delayBetweenCharacters;
    private readonly IReadOnlyList<AudioClip> _audioClips;
    private readonly float _minPitch;
    private readonly float _maxPitch;
    private readonly float _audioFrequency;

    public DialogueWriter(float delayBetweenCharacters, IReadOnlyList<AudioClip> audioClips, float minPitch, float maxPitch, float audioFrequency)
    {
        _delayBetweenCharacters = delayBetweenCharacters;
        _audioClips = audioClips;
        _minPitch = minPitch;
        _maxPitch = maxPitch;
        _audioFrequency = audioFrequency;
    }

    public Sequence WriteText(string text, TextMeshProUGUI textMesh, AudioSource audioSource)
    {
        var sequence = Sequence.Create();
        textMesh.SetText(text);
        textMesh.maxVisibleCharacters = 0;

        for (var i = 0; i < text.Length; i++)
        {
            sequence
                .ChainCallback(() =>
                    {
                        textMesh.maxVisibleCharacters++;
                        // if (textMesh.maxVisibleCharacters % _audioFrequency == 0)
                        //     PlayAudio(audioSource);
                            
                    })
                .ChainDelay(_delayBetweenCharacters);
        }

        return sequence;
    }

    private void PlayAudio(AudioSource audioSource)
    {
        var audioClip = _audioClips.GetRandomElement();
        audioSource.pitch = Random.Range(_minPitch, _maxPitch);
        audioSource.PlayOneShot(audioClip);
    }
}
