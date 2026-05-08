using System;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class DialoguePresentationService : MonoBehaviour
{
    [SerializeField]
    private DialoguePresentationView _dialoguePrefab;

    [SerializeField]
    private Transform _dialogueParent;

    [SerializeField]
    private AudioSource _audioSource;

    [Inject]
    private DialogueWriter _dialogueWriter;

    public Sequence Show(string text, Vector3 worldPosition)
    {
        if (_dialoguePrefab == null)
            throw new InvalidOperationException($"{nameof(DialoguePresentationService)} requires a dialogue prefab.");

        var dialogue = _dialogueParent == null
            ? Instantiate(_dialoguePrefab)
            : Instantiate(_dialoguePrefab, _dialogueParent);

        dialogue.transform.position = worldPosition;
        dialogue.gameObject.SetActive(false);

        if (dialogue.Text == null)
            throw new InvalidOperationException($"{nameof(DialoguePresentationView)} requires a text reference.");

        return Sequence.Create(Tween.Delay(0f, () => dialogue.gameObject.SetActive(true)))
            .Chain(_dialogueWriter.WriteText(text, dialogue.Text, _audioSource))
            .OnComplete(dialogue, static view => Destroy(view.gameObject));
    }
}
