using System;
using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class IngredientUnlockPurchasePrompt : ValidatedMonoBehaviour
{
    [SerializeField]
    private GameObject _panel;

    [SerializeField]
    private TextMeshProUGUI _priceText;

    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _closeButton;

    [SerializeField]
    private LocalizedStringTable _ingredientsTable;

    [SerializeField]
    private LocalizedStringTable _promptTable;

    [SerializeField]
    private string _purchasePromptKey;

    [Inject]
    private readonly MoneyManager _moneyManager;

    [Inject]
    private readonly IngredientsStorage _ingredientsStorage;

    private Ingredient _currentIngredient;
    private Action _onPurchased;
    private bool _isOpen;

    private void Awake()
    {
        _panel.SetActive(false);
        _confirmButton.onClick.AddListener(ConfirmPurchase);
        _closeButton.onClick.AddListener(Hide);
        _moneyManager.MoneyChanged += OnMoneyChanged;
        _ingredientsStorage.PriceChanged += OnPriceChanged;
    }

    private void OnDestroy()
    {
        _confirmButton.onClick.RemoveListener(ConfirmPurchase);
        _closeButton.onClick.RemoveListener(Hide);
        _moneyManager.MoneyChanged -= OnMoneyChanged;
        _ingredientsStorage.PriceChanged -= OnPriceChanged;
    }

    public void Show(Ingredient ingredient, Action onPurchased)
    {
        _currentIngredient = ingredient;
        _onPurchased = onPurchased;
        _isOpen = true;

        RefreshView();
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _isOpen = false;
        _currentIngredient = null;
        _onPurchased = null;
        _panel.SetActive(false);
    }

    private void ConfirmPurchase()
    {
        if (!_isOpen || _currentIngredient == null)
            return;

        if (_ingredientsStorage.Contains(_currentIngredient))
        {
            _onPurchased?.Invoke();
            Hide();
            return;
        }

        if (_ingredientsStorage.TryBuyIngredient(_currentIngredient))
        {
            _onPurchased?.Invoke();
            Hide();
            return;
        }

        RefreshView();
    }

    private void OnMoneyChanged(MoneyChangedEvent _)
    {
        RefreshView();
    }

    private void OnPriceChanged(float _)
    {
        RefreshView();
    }

    private void RefreshView()
    {
        if (!_isOpen)
            return;

        var currentPrice = _ingredientsStorage.CurrentPrice;
        var ingredientName = GetLocalizedIngredientName(_currentIngredient);
        var promptEntry = GetPurchasePromptEntry();

        _priceText.SetText(promptEntry.GetLocalizedString(new
        {
            ingredient = ingredientName,
            value = ((int)currentPrice).ToString()
        }));

        _confirmButton.interactable = _currentIngredient != null
                                      && !_ingredientsStorage.Contains(_currentIngredient)
                                      && _moneyManager.CanSpend(currentPrice);
    }

    private string GetLocalizedIngredientName(Ingredient ingredient)
    {
        if (ingredient == null)
            throw new InvalidOperationException($"{nameof(IngredientUnlockPurchasePrompt)} cannot localize a null ingredient.");

        if (string.IsNullOrWhiteSpace(ingredient.LocalizationKey))
            throw new InvalidOperationException($"Ingredient '{ingredient.name}' is missing {nameof(Ingredient.LocalizationKey)}.");

        var entry = GetIngredientsTable().GetEntry(ingredient.LocalizationKey);

        if (entry == null)
        {
            throw new InvalidOperationException(
                $"Localized ingredients table does not contain key '{ingredient.LocalizationKey}' for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");
        }

        return entry.GetLocalizedString(new { amount = 1 });
    }

    private StringTableEntry GetPurchasePromptEntry()
    {
        if (_promptTable == null)
            throw new InvalidOperationException($"{nameof(IngredientUnlockPurchasePrompt)} field '{nameof(_promptTable)}' is not assigned in the inspector.");

        if (string.IsNullOrWhiteSpace(_purchasePromptKey))
        {
            throw new InvalidOperationException(
                $"{nameof(IngredientUnlockPurchasePrompt)} field '{nameof(_purchasePromptKey)}' is not assigned in the inspector.");
        }

        var table = _promptTable.GetTable();

        if (table == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IngredientUnlockPurchasePrompt)} field '{nameof(_promptTable)}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");
        }

        var entry = table.GetEntry(_purchasePromptKey);

        if (entry == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IngredientUnlockPurchasePrompt)} could not find prompt key '{_purchasePromptKey}' for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");
        }

        return entry;
    }

    private StringTable GetIngredientsTable()
    {
        if (_ingredientsTable == null)
            throw new InvalidOperationException($"{nameof(IngredientUnlockPurchasePrompt)} field '{nameof(_ingredientsTable)}' is not assigned in the inspector.");

        var table = _ingredientsTable.GetTable();

        if (table == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IngredientUnlockPurchasePrompt)} field '{nameof(_ingredientsTable)}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.");
        }

        return table;
    }
}
