using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientStoreView : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TMP_Text _priceText;

    [SerializeField]
    private Image _priceBackgroundImage;

    [SerializeField]
    private Sprite _dailyPriceBackgroundSprite;

    [SerializeField]
    private Sprite _fixedPriceBackgroundSprite;

    [SerializeField]
    private Image _iconImage;

    [Inject]
    private readonly Store _store;

    private Ingredient _ingredient;
    private IngredientStorePurchasePrompt _purchasePrompt;
    private bool _isDaily;
    private int _remainingDays;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    public void BindDaily(Ingredient ingredient, IngredientStorePurchasePrompt purchasePrompt)
    {
        _ingredient = ingredient;
        _purchasePrompt = purchasePrompt;
        _isDaily = true;
        _remainingDays = 0;
        Refresh();
    }

    public void BindFixed(StoreFixedIngredientOffer offer, IngredientStorePurchasePrompt purchasePrompt)
    {
        _ingredient = offer.Ingredient;
        _purchasePrompt = purchasePrompt;
        _isDaily = false;
        _remainingDays = offer.RemainingDays;
        Refresh();
    }

    private void OnButtonClicked()
    {
        if (!_store.HasStock(_ingredient))
        {
            Refresh();
            return;
        }

        _purchasePrompt.Show(_ingredient, _isDaily, _remainingDays, Refresh);
    }

    private void Refresh()
    {
        _priceText.SetText(TextUtils.FormatAsMoney(_ingredient.BuyPrice));
        _priceBackgroundImage.sprite = _isDaily ? _dailyPriceBackgroundSprite : _fixedPriceBackgroundSprite;
        _iconImage.sprite = _ingredient.Icon;
        _button.interactable = _store.HasStock(_ingredient);
    }
}
