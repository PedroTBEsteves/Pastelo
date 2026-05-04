using System;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class IngredientStorePurchasePrompt : MonoBehaviour
{
    [SerializeField]
    private GameObject _panel;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _unitPriceText;

    [SerializeField]
    private TMP_Text _availabilityText;

    [SerializeField]
    private TMP_Text _stockText;

    [SerializeField]
    private TMP_Text _quantityText;

    [SerializeField]
    private TMP_Text _totalPriceText;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private Image _priceBackgroundImage;

    [SerializeField]
    private Sprite _dailyPriceBackgroundSprite;

    [SerializeField]
    private Sprite _fixedPriceBackgroundSprite;

    [SerializeField]
    private Image _dailyAdditionalImage;

    [SerializeField]
    private LocalizedSprite _dailyAdditionalSprite;

    [SerializeField]
    private LocalizedString _dailyAvailabilityText;

    [SerializeField]
    private LocalizedString _fixedAvailabilityText;

    [SerializeField]
    private Button _increaseQuantityButton;

    [SerializeField]
    private Button _decreaseQuantityButton;

    [SerializeField]
    private Button _buyButton;

    [SerializeField]
    private Button _closeButton;

    [Inject]
    private readonly Store _store;

    [Inject]
    private readonly MoneyManager _moneyManager;

    private Ingredient _ingredient;
    private Action _onPurchased;
    private bool _isDaily;
    private int _remainingDays;
    private int _quantity;
    private bool _isOpen;
    private Sprite _localizedDailyAdditionalSprite;

    private void Awake()
    {
        _panel.SetActive(false);
        _increaseQuantityButton.onClick.AddListener(IncreaseQuantity);
        _decreaseQuantityButton.onClick.AddListener(DecreaseQuantity);
        _buyButton.onClick.AddListener(Buy);
        _closeButton.onClick.AddListener(Hide);
        _moneyManager.MoneyChanged += OnMoneyChanged;
        _dailyAdditionalSprite.AssetChanged += OnDailyAdditionalSpriteChanged;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDestroy()
    {
        _increaseQuantityButton.onClick.RemoveListener(IncreaseQuantity);
        _decreaseQuantityButton.onClick.RemoveListener(DecreaseQuantity);
        _buyButton.onClick.RemoveListener(Buy);
        _closeButton.onClick.RemoveListener(Hide);
        _moneyManager.MoneyChanged -= OnMoneyChanged;
        _dailyAdditionalSprite.AssetChanged -= OnDailyAdditionalSpriteChanged;
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    public void Show(Ingredient ingredient, bool isDaily, int remainingDays, Action onPurchased)
    {
        _ingredient = ingredient;
        _isDaily = isDaily;
        _remainingDays = remainingDays;
        _onPurchased = onPurchased;
        _quantity = 1;
        _isOpen = true;

        Refresh();
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _isOpen = false;
        _ingredient = null;
        _onPurchased = null;
        _panel.SetActive(false);
    }

    private void IncreaseQuantity()
    {
        var stock = _store.GetRemainingStock(_ingredient);
        _quantity = Mathf.Min(stock, _quantity + 1);
        RefreshPurchaseValues(stock);
    }

    private void DecreaseQuantity()
    {
        _quantity = Mathf.Max(1, _quantity - 1);
        RefreshPurchaseValues(_store.GetRemainingStock(_ingredient));
    }

    private void Buy()
    {
        if (_store.TryBuyIngredient(_ingredient, _quantity))
        {
            _onPurchased?.Invoke();
            Hide();
            return;
        }

        Refresh();
    }

    private void OnMoneyChanged(MoneyChangedEvent _)
    {
        if (_isOpen)
            RefreshPurchaseValues(_store.GetRemainingStock(_ingredient));
    }

    private void OnSelectedLocaleChanged(Locale _)
    {
        if (_isOpen)
            Refresh();
    }

    private void OnDailyAdditionalSpriteChanged(Sprite sprite)
    {
        _localizedDailyAdditionalSprite = sprite;

        if (_isOpen && _isDaily)
            _dailyAdditionalImage.sprite = sprite;
    }

    private void Refresh()
    {
        var stock = _store.GetRemainingStock(_ingredient);
        _quantity = Mathf.Clamp(_quantity, 1, Mathf.Max(1, stock));

        _nameText.SetText(_ingredient.GetName());
        _unitPriceText.SetText(TextUtils.FormatAsMoney(_ingredient.BuyPrice));
        _availabilityText.SetText(_isDaily
            ? _dailyAvailabilityText.GetLocalizedString()
            : _fixedAvailabilityText.GetLocalizedString(new { days = _remainingDays }));
        _stockText.SetText(stock.ToString());
        _iconImage.sprite = _ingredient.Icon;
        _priceBackgroundImage.sprite = _isDaily ? _dailyPriceBackgroundSprite : _fixedPriceBackgroundSprite;
        _dailyAdditionalImage.gameObject.SetActive(_isDaily);

        if (_isDaily)
            _dailyAdditionalImage.sprite = _localizedDailyAdditionalSprite;

        RefreshPurchaseValues(stock);
    }

    private void RefreshPurchaseValues(int stock)
    {
        var totalPrice = _ingredient.BuyPrice * _quantity;

        _quantityText.SetText(_quantity.ToString());
        _totalPriceText.SetText(TextUtils.FormatAsMoney(totalPrice));
        _decreaseQuantityButton.interactable = _quantity > 1;
        _increaseQuantityButton.interactable = _quantity < stock;
        _buyButton.interactable = stock > 0 && _moneyManager.CanSpend(totalPrice);
    }
}
