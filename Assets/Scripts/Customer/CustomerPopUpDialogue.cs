using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class CustomerPopUpDialogue : MonoBehaviour, ICustomerPopUpDialogue
{
    [SerializeField]
    private CustomerPopUpDialogueView _popupViewPrefab;

    [SerializeField]
    private Transform _popupRoot;

    [SerializeField]
    private float _delayAfterWritingIsDone;
    
    [Inject]
    private DialogueWriter _dialogueWriter;

    public Sequence ShowDialogue(Customer customer, string dialogue)
    {
        var popupView = Instantiate(_popupViewPrefab, _popupRoot);
        var writeHandle = _dialogueWriter.CreateWriteHandle(dialogue, popupView.Text, popupView.AudioSource);
        var sequence = Sequence.Create()
            .Chain(writeHandle.Sequence)
            .Chain(Tween.Delay(_delayAfterWritingIsDone))
            .OnComplete(popupView, static view => Destroy(view.gameObject));

        popupView.CustomerImage.sprite = customer.Icone;
        popupView.Clicked += HandlePopupClicked;

        return sequence;

        void HandlePopupClicked()
        {
            if (!sequence.isAlive)
                return;

            if (!writeHandle.IsTextFullyVisible)
            {
                writeHandle.RevealTextImmediately();
                sequence.elapsedTime = Mathf.Min(writeHandle.WriteDuration, sequence.duration);
                return;
            }

            popupView.Clicked -= HandlePopupClicked;
            sequence.Complete();
        }
    }
}
