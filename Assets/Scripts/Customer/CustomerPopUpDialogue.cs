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

        popupView.CustomerImage.sprite = customer.Icone;

        return _dialogueWriter.WriteText(dialogue, popupView.Text, popupView.AudioSource)
            .Chain(Tween.Delay(_delayAfterWritingIsDone))
            .OnComplete(popupView, static view => Destroy(view.gameObject));
    }
}
