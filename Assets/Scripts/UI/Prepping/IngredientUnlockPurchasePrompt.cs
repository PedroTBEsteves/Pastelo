using System;
using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
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

    [Inject]
    private readonly Money _money;

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
        _money.MoneyChanged += OnMoneyChanged;
        _ingredientsStorage.PriceChanged += OnPriceChanged;
    }

    private void OnDestroy()
    {
        _confirmButton.onClick.RemoveListener(ConfirmPurchase);
        _closeButton.onClick.RemoveListener(Hide);
        _money.MoneyChanged -= OnMoneyChanged;
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

    private void OnMoneyChanged(Money.MoneyChangedEvent _)
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
        
        var ingridientName = _currentIngredient is Dough ? $"Massa {_currentIngredient.Name}" : _currentIngredient.Name;

        var currentPrice = _ingredientsStorage.CurrentPrice;
        _priceText.SetText($"Desbloquear {ingridientName} por {TextUtils.FormatAsMoney(currentPrice)}?");
        _confirmButton.interactable = _currentIngredient != null
                                      && !_ingredientsStorage.Contains(_currentIngredient)
                                      && _money.CanSpend(currentPrice);
    }
}
