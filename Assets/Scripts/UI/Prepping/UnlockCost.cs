using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

public class UnlockCost : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _priceText;
    
    [Inject]
    private readonly IngredientsStorage _ingredientsStorage;

    private void Awake()
    {
        UpdatePriceText(_ingredientsStorage.CurrentPrice);
        _ingredientsStorage.PriceChanged += UpdatePriceText;
    }

    private void UpdatePriceText(float value)
    {
        _priceText.SetText($"Custo para desbloquear: {TextUtils.FormatAsMoney(value)}");
    }
}
