using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPopUpDialogue : MonoBehaviour, ICustomerPopUpDialogue
{
    [SerializeField]
    private GameObject _popUpRoot;
    
    [SerializeField]
    private TextMeshProUGUI _popUpText;

    [SerializeField]
    private Image _customerImage;

    [SerializeField]
    private TextAsset[] _customerGaveUpDialogues;
    
    [SerializeField]
    private TextAsset[] _customerOrderExpiredDialogues;
    
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private float _delayAfterWritingIsDone;
    
    [Inject]
    private DialogueWriter _dialogueWriter;

    public Sequence CustomerGaveUpDialogue(Customer customer) => DialogueSequence(customer, _customerGaveUpDialogues);

    public Sequence CustomerOrderExpiredDialogue(Customer customer) => DialogueSequence(customer, _customerOrderExpiredDialogues);

    private Sequence DialogueSequence(Customer customer, TextAsset[] dialogueOptions)
    {
        _customerImage.sprite = customer.Sprite;
        _popUpRoot.SetActive(true);
        var dialogue = dialogueOptions.GetRandomElement();
        
        return _dialogueWriter.WriteText(dialogue.text, _popUpText, _audioSource)
            .Chain(Tween.Delay(_delayAfterWritingIsDone, () =>
        {
            _popUpRoot.SetActive(false);
        }));
    }
}
