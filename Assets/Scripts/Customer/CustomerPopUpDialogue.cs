using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Random = UnityEngine.Random;

public class CustomerPopUpDialogue : MonoBehaviour, ICustomerPopUpDialogue
{
    [SerializeField]
    private CustomerPopUpDialogueView _popupViewPrefab;

    [SerializeField]
    private Transform _popupRoot;

    [SerializeField]
    private LocalizedStringTable _customerGaveUpDialogues;
    
    [SerializeField]
    private LocalizedStringTable _customerOrderExpiredDialogues;

    [SerializeField]
    private float _delayAfterWritingIsDone;
    
    [Inject]
    private DialogueWriter _dialogueWriter;

    public Sequence CustomerGaveUpDialogue(Customer customer) =>
        DialogueSequence(customer, _customerGaveUpDialogues, nameof(_customerGaveUpDialogues));

    public Sequence CustomerOrderExpiredDialogue(Customer customer) =>
        DialogueSequence(customer, _customerOrderExpiredDialogues, nameof(_customerOrderExpiredDialogues));

    private Sequence DialogueSequence(Customer customer, LocalizedStringTable dialogueOptions, string fieldName)
    {
        var dialogue = GetRandomLocalizedDialogue(dialogueOptions, fieldName);
        var popupView = Instantiate(_popupViewPrefab, _popupRoot);

        popupView.CustomerImage.sprite = customer.Icone;

        return _dialogueWriter.WriteText(dialogue, popupView.Text, popupView.AudioSource)
            .Chain(Tween.Delay(_delayAfterWritingIsDone))
            .OnComplete(popupView, static view => Destroy(view.gameObject));
    }

    private static string GetRandomLocalizedDialogue(LocalizedStringTable tableReference, string fieldName) =>
        GetRandomLocalizedEntry(tableReference, fieldName).GetLocalizedString();

    private static StringTableEntry GetRandomLocalizedEntry(LocalizedStringTable tableReference, string fieldName)
    {
        var table = tableReference.GetTable();

        var randomIndex = Random.Range(0, table.SharedData.Entries.Count);
        var sharedEntry = table.SharedData.Entries[randomIndex];
        return table.GetEntry(sharedEntry.Id);
    }
}
