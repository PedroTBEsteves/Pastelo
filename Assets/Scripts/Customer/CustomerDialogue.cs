using System;
using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.VFX;

public class CustomerDialogue : MonoBehaviour, ICustomerDialogue
{
    private const string PortugueseLanguageCode = "pt";

    [SerializeField]
    private SpriteRenderer _customerSprite;

    [SerializeField]
    private SpriteRenderer _customerTransitionSprite;

    [SerializeField]
    private SpriteRenderer _iconSprite;

    [SerializeField]
    private SpriteRenderer _queuedCustomersIndicatorSprite;

    [SerializeField]
    private Vector3 _customerStartLocalOffset;

    [SerializeField]
    private TweenSettings _customerMoveTweenSettings;

    [SerializeField]
    private float _customerBobbingRange = 0.1f;

    [SerializeField]
    private TweenSettings _customerBobbingTweenSettings = new(1f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);

    [SerializeField]
    private GameObject _dialogueObject;
    
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private LocalizedStringTable _orderDialogueTable;

    [SerializeField]
    private LocalizedStringTable _emptyPastelDialogueTable;

    [SerializeField]
    private LocalizedStringTable _failedOrderDialoguesTable;

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

    [SerializeField]
    private VisualEffect _happyVisualEffect;

    [Inject]
    private DialogueWriter _dialogueWriter;
    
    [Inject]
    private readonly CustomerQueue _customerQueue;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private Sequence _dialogueSequence;
    private TutorialTarget _tutorialTarget;
    private Tween _customerMoveTween;
    private Tween _customerTransitionMoveTween;
    private Tween _customerBobbingTween;
    private Vector3 _customerIdleLocalPosition;
    private HoverTransformTween _customerHoverTween;

    public bool IsPlaying => _dialogueSequence.isAlive;

    private void Awake()
    {
        _customerIdleLocalPosition = _customerSprite.transform.localPosition;
        _customerSprite.TryGetComponent(out _customerHoverTween);
        ResetTransitionSprite();
        SetRendererState(_customerSprite, _customerSprite.sprite, _customerIdleLocalPosition);
        SetCustomerHoverEnabled(false);
        StartCustomerBobbing();
        SetQueuedCustomersIndicator(GetQueuedCustomersCount());

        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.OrderBell);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        StopCustomerTweens();

        if (_customerQueue != null)
        {
            _customerQueue.CustomerArrived -= OnCustomerArrived;
            _customerQueue.CustomerExpired -= OnCustomerExpired;
            _customerQueue.CustomersCountChanged -= OnCustomersCountChanged;
        }

        if (_tutorialTarget != null)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    private void Start()
    {
        _customerQueue.CustomerArrived += OnCustomerArrived;
        _customerQueue.CustomerExpired += OnCustomerExpired;
        _customerQueue.CustomersCountChanged += OnCustomersCountChanged;
        SetQueuedCustomersIndicator(GetQueuedCustomersCount());
    }

    public Sequence OrderDialogue(Order order)
    {
        ShowCurrentCustomer(order.Customer.Sprite);
        SetCustomerHoverEnabled(false);
        _iconSprite.enabled = false;

        var text = GetOrderDialogueText(order);
        
        _dialogueObject.SetActive(true);

        _dialogueSequence = _dialogueWriter.WriteText(text, _text, _audioSource)
            .Chain(Tween.Delay(_delayAfterTextIsDone, () =>
            {
                _dialogueObject.SetActive(false);
                AnimateNextCustomerFromQueue();
            }));
        
        return _dialogueSequence;
    }

    public Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController)
    {
        ShowCurrentCustomer(order.Customer.Sprite);
        SetCustomerHoverEnabled(false);
        _iconSprite.enabled = false;
        _deliveryBag.SetActive(true);

        var isCorrect = delivery.IsCorrectFor(order);

        var dialogue = GetRandomDeliveryDialogue(isCorrect);
        
        _dialogueSequence = Sequence.Create(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(true);
            
            orderController.DeliverOrder(order, delivery);
            if (isCorrect)
                _happyVisualEffect.Play();
        }))
        .Chain(_dialogueWriter.WriteText(dialogue, _text, _audioSource))
        .Chain(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(false);
            AnimateNextCustomerFromQueue();
            _deliveryBag.SetActive(false);
            
            _happyVisualEffect.Stop();
        }));

        return _dialogueSequence;
    }

    private void OnCustomerArrived(Customer customer)
    {
        if (_customerSprite.sprite != null)
            return;

        AnimateCustomerArrival(customer.Sprite);
        _iconSprite.enabled = true;
    }

    private void OnCustomerExpired(Customer customer)
    {
        if (!TryGetCurrentAndNextQueuedCustomers(out var currentCustomer, out var nextCustomer))
            return;

        if (currentCustomer != customer)
            return;

        if (nextCustomer != null)
        {
            AnimateCustomerSwap(currentCustomer.Sprite, nextCustomer.Sprite);
            _iconSprite.enabled = true;
            return;
        }

        AnimateCustomerExit();
        _iconSprite.enabled = false;
    }

    private void OnCustomersCountChanged(int count) => SetQueuedCustomersIndicator(count);

    private void AnimateNextCustomerFromQueue()
    {
        if (_customerQueue.TryPeek(out var nextCustomer))
        {
            AnimateCustomerSwap(_customerSprite.sprite, nextCustomer.Sprite);
            _iconSprite.enabled = true;
            return;
        }

        AnimateCustomerExit();
        _iconSprite.enabled = false;
    }

    private void ShowCurrentCustomer(Sprite sprite)
    {
        StopCustomerTweens();
        ResetTransitionSprite();
        SetRendererState(_customerSprite, sprite, _customerIdleLocalPosition);
        StartCustomerBobbing();
    }

    private void AnimateCustomerArrival(Sprite sprite)
    {
        StopCustomerTweens();
        ResetTransitionSprite();
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        SetRendererState(_customerSprite, sprite, customerStartLocalPosition);
        _customerMoveTween = Tween.Position(
            _customerSprite.transform,
            GetWorldPosition(customerStartLocalPosition),
            GetWorldPosition(_customerIdleLocalPosition),
            _customerMoveTweenSettings)
            .OnComplete(this, static dialogue => dialogue.StartCustomerBobbing());
    }

    private void AnimateCustomerExit()
    {
        if (_customerSprite.sprite == null)
        {
            SetCustomerHoverEnabled(false);
            ResetTransitionSprite();
            return;
        }

        StopCustomerTweens();
        ResetTransitionSprite();
        SetRendererState(_customerSprite, _customerSprite.sprite, _customerIdleLocalPosition);
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        _customerMoveTween = Tween.Position(
            _customerSprite.transform,
            GetWorldPosition(_customerIdleLocalPosition),
            GetWorldPosition(customerStartLocalPosition),
            _customerMoveTweenSettings)
            .OnComplete(this, static dialogue =>
            {
                dialogue.SetRendererState(dialogue._customerSprite, null, dialogue._customerIdleLocalPosition);
            });
    }

    private void AnimateCustomerSwap(Sprite outgoingSprite, Sprite incomingSprite)
    {
        if (incomingSprite == null)
        {
            AnimateCustomerExit();
            return;
        }

        if (outgoingSprite == null)
        {
            AnimateCustomerArrival(incomingSprite);
            return;
        }

        StopCustomerTweens();
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        SetRendererState(_customerTransitionSprite, outgoingSprite, _customerIdleLocalPosition);
        SetRendererState(_customerSprite, incomingSprite, customerStartLocalPosition);

        _customerTransitionMoveTween = Tween.Position(
            _customerTransitionSprite.transform,
            GetWorldPosition(_customerIdleLocalPosition),
            GetWorldPosition(customerStartLocalPosition),
            _customerMoveTweenSettings)
            .OnComplete(this, static dialogue => dialogue.ResetTransitionSprite());

        _customerMoveTween = Tween.Position(
            _customerSprite.transform,
            GetWorldPosition(customerStartLocalPosition),
            GetWorldPosition(_customerIdleLocalPosition),
            _customerMoveTweenSettings)
            .OnComplete(this, static dialogue => dialogue.StartCustomerBobbing());
    }

    private void StopCustomerTweens()
    {
        SetCustomerHoverEnabled(false);

        if (_customerMoveTween.isAlive)
            _customerMoveTween.Stop();

        if (_customerTransitionMoveTween.isAlive)
            _customerTransitionMoveTween.Stop();

        StopCustomerBobbing(true);
    }

    private void StartCustomerBobbing()
    {
        if (_customerSprite.sprite == null || !_customerSprite.enabled || _customerBobbingRange <= 0f)
            return;

        StopCustomerBobbing(true);

        _customerBobbingTween = Tween.LocalPositionY(
            _customerSprite.transform,
            _customerIdleLocalPosition.y,
            _customerIdleLocalPosition.y + _customerBobbingRange,
            _customerBobbingTweenSettings);

        SetCustomerHoverEnabled(true);
    }

    private void StopCustomerBobbing(bool resetToIdlePosition)
    {
        if (_customerBobbingTween.isAlive)
            _customerBobbingTween.Stop();

        if (!resetToIdlePosition)
            return;

        var localPosition = _customerSprite.transform.localPosition;
        localPosition.y = _customerIdleLocalPosition.y;
        _customerSprite.transform.localPosition = localPosition;
    }

    private void ResetTransitionSprite() => SetRendererState(_customerTransitionSprite, null, GetCustomerStartLocalPosition());

    private void SetCustomerHoverEnabled(bool enabled)
    {
        if (_customerHoverTween == null)
            return;

        _customerHoverTween.SetTweenEnabled(enabled);
    }

    private void SetRendererState(SpriteRenderer renderer, Sprite sprite, Vector3 localPosition)
    {
        renderer.sprite = sprite;
        renderer.enabled = sprite != null;
        renderer.transform.localPosition = localPosition;
    }

    private Vector3 GetWorldPosition(Vector3 localPosition)
    {
        var parent = _customerSprite.transform.parent;
        return parent == null ? localPosition : parent.TransformPoint(localPosition);
    }

    private Vector3 GetCustomerStartLocalPosition() => _customerIdleLocalPosition + _customerStartLocalOffset;

    private void SetQueuedCustomersIndicator(int count) => _queuedCustomersIndicatorSprite.enabled = count > 1;

    private int GetQueuedCustomersCount()
    {
        var count = 0;

        foreach (var _ in _customerQueue.Entries)
            count++;

        return count;
    }

    private bool TryGetCurrentAndNextQueuedCustomers(out Customer currentCustomer, out Customer nextCustomer)
    {
        currentCustomer = null;
        nextCustomer = null;

        using var entries = _customerQueue.Entries.GetEnumerator();
        if (!entries.MoveNext())
            return false;

        currentCustomer = entries.Current.Customer;

        if (entries.MoveNext())
            nextCustomer = entries.Current.Customer;

        return true;
    }

    private string GetOrderDialogueText(Order order)
    {
        if (order.HadMissingIngredients)
        {
            var missingIngredientsText = GetMissingIngredientsText(order.MissingIngredients);
            var templateMissingEntry = GetRandomLocalizedEntry(_failedOrderDialoguesTable, nameof(_failedOrderDialoguesTable));
            templateMissingEntry.IsSmart = true;
            return templateMissingEntry.GetLocalizedString(new { ingredients = missingIngredientsText });
        }

        if (order.Recipe == null)
            throw new InvalidOperationException("Expected a recipe for a valid order dialogue.");

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

    private static string GetMissingIngredientsText(IReadOnlyList<Ingredient> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0)
            return string.Empty;

        var ingredientParts = new List<string>(ingredients.Count);

        foreach (var ingredient in ingredients)
        {
            if (ingredient == null)
                continue;

            ingredientParts.Add(ingredient.GetDisplayName());
        }

        return JoinLocalizedList(ingredientParts);
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
