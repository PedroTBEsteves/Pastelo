using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class DialogueWriter
{
    private readonly float _delayBetweenCharacters;
    private readonly float _audioFrequency;

    public DialogueWriter(float delayBetweenCharacters, float audioFrequency)
    {
        _delayBetweenCharacters = delayBetweenCharacters;
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
                        if (textMesh.maxVisibleCharacters % _audioFrequency == 0)
                             audioSource.Play();
                            
                    })
                .ChainDelay(_delayBetweenCharacters);
        }

        return sequence;
    }
}
