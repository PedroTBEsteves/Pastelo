using System;
using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class CustomerDialogue : MonoBehaviour, ICustomerDialogue
{
    private const string PortugueseLanguageCode = "pt";

    [SerializeField]
    private SpriteRenderer _customerSprite;

    [SerializeField]
    private SpriteRenderer _iconSprite;

    [SerializeField]
    private GameObject _dialogueObject;
    
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private LocalizedStringTable _orderDialogueTable;

    [SerializeField]
    private LocalizedStringTable _emptyPastelDialogueTable;

    [SerializeField]
    private LocalizedStringTable _correctOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _incorrectOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _ingredientsTable;

    [SerializeField]
    private GameObject _deliveryBag;
    
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private float _delayAfterTextIsDone = 1f;

    [Inject]
    private DialogueWriter _dialogueWriter;
    
    [Inject]
    private readonly CustomerQueue _customerQueue;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private Sequence _dialogueSequence;
    private TutorialTarget _tutorialTarget;

    public bool IsPlaying => _dialogueSequence.isAlive;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.OrderBell);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        if (_tutorialTarget != null)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    private void Start()
    {
        _customerQueue.CustomerArrived += customer =>
        {
            if (_customerSprite.sprite == null)
            {
                _customerSprite.sprite = customer.Sprite;
                _iconSprite.enabled = true;
            }
        };

        _customerQueue.CustomerExpired += customer =>
        {
            if (_customerQueue.TryPeek(out var nextCustomer))
                _customerSprite.sprite = nextCustomer.Sprite;
            else
            {
                _customerSprite.sprite = null;
                _iconSprite.enabled = false;
            }
        };
    }

    public Sequence OrderDialogue(Order order)
    {
        _iconSprite.enabled = false;
        _customerSprite.sprite = order.Customer.Sprite;

        var text = GetOrderDialogueText(order);
        
        _dialogueObject.SetActive(true);

        _dialogueSequence = _dialogueWriter.WriteText(text, _text, _audioSource)
            .Chain(Tween.Delay(_delayAfterTextIsDone, () =>
            {
                _dialogueObject.SetActive(false);
                if (_customerQueue.TryPeek(out var nextCustomer))
                {
                    _customerSprite.sprite = nextCustomer.Sprite;
                    _iconSprite.enabled = true;
                }
                else
                    _customerSprite.sprite = null;
            }));
        
        return _dialogueSequence;
    }

    public Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController)
    {
        _iconSprite.enabled = false;
        _customerSprite.sprite = order.Customer.Sprite;
        _deliveryBag.SetActive(true);

        var dialogue = GetRandomDeliveryDialogue(delivery.IsCorrectFor(order));
        
        _dialogueSequence = Sequence.Create(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(true);
            
            orderController.DeliverOrder(order, delivery);
        }))
        .Chain(_dialogueWriter.WriteText(dialogue, _text, _audioSource))
        .Chain(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(false);
            if (_customerQueue.TryPeek(out var nextCustomer))
                _customerSprite.sprite = nextCustomer.Sprite;
            else
                _customerSprite.sprite = null;
            _deliveryBag.SetActive(false);
        }));

        return _dialogueSequence;
    }

    private string GetOrderDialogueText(Order order)
    {
        if (order.Recipe.Fillings.Count == 0)
            return GetRandomLocalizedDialogue(_emptyPastelDialogueTable, nameof(_emptyPastelDialogueTable));

        var doughName = GetLocalizedIngredientName(order.Recipe.Dough);
        var fillingsText = GetFillingsText(order.Recipe.Fillings);

        var templateEntry = GetRandomLocalizedEntry(_orderDialogueTable, nameof(_orderDialogueTable));
        templateEntry.IsSmart = true;
        return templateEntry.GetLocalizedString(new { dough = doughName, fillings = fillingsText });
    }

    private string GetRandomDeliveryDialogue(bool isCorrectDelivery)
    {
        var tableReference = isCorrectDelivery ? _correctOrderDialoguesTable : _incorrectOrderDialoguesTable;
        var fieldName = isCorrectDelivery ? nameof(_correctOrderDialoguesTable) : nameof(_incorrectOrderDialoguesTable);
        return GetRandomLocalizedDialogue(tableReference, fieldName);
    }

    private static string GetRandomLocalizedDialogue(LocalizedStringTable tableReference, string fieldName) =>
        GetRandomLocalizedEntry(tableReference, fieldName).GetLocalizedString();

    private string GetFillingsText(IReadOnlyDictionary<Filling, int> fillings)
    {
        var fillingsParts = new List<string>(fillings.Count);

        foreach (var (filling, amount) in fillings)
            fillingsParts.Add($"{amount} {GetLocalizedIngredientName(filling, amount)}");

        return JoinLocalizedList(fillingsParts);
    }

    private static string JoinLocalizedList(IReadOnlyList<string> items)
    {
        if (items.Count == 0)
            return string.Empty;

        if (items.Count == 1)
            return items[0];

        var separator = ", ";
        var finalSeparator = IsPortugueseSelected() ? " e " : " and ";

        if (items.Count == 2)
            return $"{items[0]}{finalSeparator}{items[1]}";

        var combinedText = items[0];

        for (var i = 1; i < items.Count - 1; i++)
            combinedText += separator + items[i];

        return combinedText + finalSeparator + items[^1];
    }

    private string GetLocalizedIngredientName(Ingredient ingredient, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(ingredient.LocalizationKey))
            throw new InvalidOperationException($"Ingredient '{ingredient.name}' is missing {nameof(Ingredient.LocalizationKey)}.");

        var entry = GetLocalizedEntry(_ingredientsTable, ingredient.LocalizationKey, nameof(_ingredientsTable));
        return entry.GetLocalizedString(new { amount });
    }

    private static bool IsPortugueseSelected()
    {
        var localeCode = LocalizationSettings.SelectedLocale?.Identifier.Code;
        return !string.IsNullOrEmpty(localeCode) && localeCode.StartsWith(PortugueseLanguageCode, StringComparison.OrdinalIgnoreCase);
    }

    private static StringTableEntry GetRandomLocalizedEntry(LocalizedStringTable tableReference, string fieldName)
    {
        var table = GetRequiredTable(tableReference, fieldName);

        if (table.SharedData?.Entries == null || table.SharedData.Entries.Count == 0)
            throw new InvalidOperationException($"Localized table '{fieldName}' has no entries.");

        var randomIndex = UnityEngine.Random.Range(0, table.SharedData.Entries.Count);
        var sharedEntry = table.SharedData.Entries[randomIndex];
        var entry = table.GetEntry(sharedEntry.Id);

        if (entry == null)
            throw new InvalidOperationException($"Localized table '{fieldName}' is missing entry id '{sharedEntry.Id}' for the selected locale.");

        return entry;
    }

    private static StringTableEntry GetLocalizedEntry(LocalizedStringTable tableReference, string key, string fieldName)
    {
        var table = GetRequiredTable(tableReference, fieldName);
        var entry = table.GetEntry(key);

        if (entry == null)
            throw new InvalidOperationException($"Localized table '{fieldName}' does not contain key '{key}' for the selected locale.");

        return entry;
    }

    private static StringTable GetRequiredTable(LocalizedStringTable tableReference, string fieldName)
    {
        if (tableReference == null)
            throw new InvalidOperationException($"CustomerDialogue field '{fieldName}' is not assigned in the inspector.");

        var table = tableReference.GetTable();

        if (table == null)
            throw new InvalidOperationException($"CustomerDialogue field '{fieldName}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");

        return table;
    }
}
