using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientStorePurchaseButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _priceText;

    [SerializeField]
    private Image _iconImage;

    [Inject]
    private readonly Store _store;

    [Inject]
    private readonly MoneyManager _moneyManager;

    private Ingredient _ingredient;

    private void Awake()
    {
        if (_button != null)
            _button.onClick.AddListener(OnButtonClicked);

        _moneyManager.MoneyChanged += OnMoneyChanged;
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnButtonClicked);

        if (_moneyManager != null)
            _moneyManager.MoneyChanged -= OnMoneyChanged;
    }

    public void Bind(Ingredient ingredient)
    {
        _ingredient = ingredient;
        Refresh();
    }

    private void OnButtonClicked()
    {
        if (_ingredient == null)
            return;

        if (_store.TryBuyIngredient(_ingredient))
            RefreshInteractable();
    }

    private void OnMoneyChanged(MoneyChangedEvent _)
    {
        RefreshInteractable();
    }

    private void Refresh()
    {
        if (_nameText != null)
            _nameText.SetText(_ingredient != null ? _ingredient.GetName() : string.Empty);

        if (_priceText != null)
            _priceText.SetText(_ingredient != null ? TextUtils.FormatAsMoney(_ingredient.BuyPrice) : string.Empty);

        if (_iconImage != null)
            _iconImage.sprite = _ingredient != null ? _ingredient.Icon : null;

        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        if (_button == null)
            return;

        _button.interactable = _ingredient != null && _moneyManager.CanSpend(_ingredient.BuyPrice);
    }
}
