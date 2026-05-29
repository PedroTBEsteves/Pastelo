using PrimeTween;
using TMPro;
using UnityEngine;

public class DialogueWriter
{
    public sealed class DialogueWriteHandle
    {
        private readonly TextMeshProUGUI _textMesh;
        private readonly AudioSource _audioSource;

        public DialogueWriteHandle(TextMeshProUGUI textMesh, AudioSource audioSource, Sequence sequence)
        {
            _textMesh = textMesh;
            _audioSource = audioSource;
            Sequence = sequence;
            WriteDuration = sequence.duration;
        }

        public Sequence Sequence { get; }
        public float WriteDuration { get; }
        public bool IsTextFullyVisible { get; private set; }

        public void RevealTextImmediately()
        {
            if (IsTextFullyVisible)
                return;

            _textMesh.maxVisibleCharacters = int.MaxValue;
            _audioSource.Stop();
            IsTextFullyVisible = true;
        }

        public void MarkCompleted() => IsTextFullyVisible = true;
    }

    private readonly float _delayBetweenCharacters;
    private readonly float _audioFrequency;

    public DialogueWriter(float delayBetweenCharacters, float audioFrequency)
    {
        _delayBetweenCharacters = delayBetweenCharacters;
        _audioFrequency = audioFrequency;
    }

    public Sequence WriteText(string text, TextMeshProUGUI textMesh, AudioSource audioSource)
    {
        return CreateWriteHandle(text, textMesh, audioSource).Sequence;
    }

    public DialogueWriteHandle CreateWriteHandle(string text, TextMeshProUGUI textMesh, AudioSource audioSource)
    {
        textMesh.SetText(text);
        textMesh.maxVisibleCharacters = 0;

        var previousVisibleCharacters = 0;
        var writeTween = Tween.Custom(0f, text.Length, duration: text.Length * _delayBetweenCharacters, onValueChange: visibleCharacters =>
        {
            var currentVisibleCharacters = Mathf.Clamp(Mathf.FloorToInt(visibleCharacters), 0, text.Length);
            textMesh.maxVisibleCharacters = currentVisibleCharacters;

            if (currentVisibleCharacters <= previousVisibleCharacters || currentVisibleCharacters == 0 || _audioFrequency <= 0f)
            {
                previousVisibleCharacters = currentVisibleCharacters;
                return;
            }

            if (Mathf.Approximately(currentVisibleCharacters % _audioFrequency, 0f))
                audioSource.Play();

            previousVisibleCharacters = currentVisibleCharacters;
        });

        var sequence = Sequence.Create(writeTween);
        var handle = new DialogueWriteHandle(textMesh, audioSource, sequence);
        sequence.ChainCallback(handle, static target => target.MarkCompleted());
        return handle;
    }
}
