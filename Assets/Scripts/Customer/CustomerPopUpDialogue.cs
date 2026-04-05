using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
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
    private LocalizedStringTable _customerGaveUpDialogues;
    
    [SerializeField]
    private LocalizedStringTable _customerOrderExpiredDialogues;
    
    [SerializeField]
    private AudioSource _audioSource;

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
        _customerImage.sprite = customer.Icone;
        _popUpRoot.SetActive(true);
        var dialogue = GetRandomLocalizedDialogue(dialogueOptions, fieldName);
        
        return _dialogueWriter.WriteText(dialogue, _popUpText, _audioSource)
            .Chain(Tween.Delay(_delayAfterWritingIsDone, () =>
        {
            _popUpRoot.SetActive(false);
        }));
    }

    private static string GetRandomLocalizedDialogue(LocalizedStringTable tableReference, string fieldName) =>
        GetRandomLocalizedEntry(tableReference, fieldName).GetLocalizedString();

    private static StringTableEntry GetRandomLocalizedEntry(LocalizedStringTable tableReference, string fieldName)
    {
        var table = GetRequiredTable(tableReference, fieldName);

        if (table.SharedData?.Entries == null || table.SharedData.Entries.Count == 0)
            throw new System.InvalidOperationException($"Localized table '{fieldName}' has no entries.");

        var randomIndex = Random.Range(0, table.SharedData.Entries.Count);
        var sharedEntry = table.SharedData.Entries[randomIndex];
        var entry = table.GetEntry(sharedEntry.Id);

        if (entry == null)
        {
            throw new System.InvalidOperationException(
                $"Localized table '{fieldName}' is missing entry id '{sharedEntry.Id}' for the selected locale.");
        }

        return entry;
    }

    private static StringTable GetRequiredTable(LocalizedStringTable tableReference, string fieldName)
    {
        if (tableReference == null)
            throw new System.InvalidOperationException($"CustomerPopUpDialogue field '{fieldName}' is not assigned in the inspector.");

        var table = tableReference.GetTable();

        if (table == null)
        {
            throw new System.InvalidOperationException(
                $"CustomerPopUpDialogue field '{fieldName}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");
        }

        return table;
    }
}
