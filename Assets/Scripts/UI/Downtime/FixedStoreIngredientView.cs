using KBCore.Refs;
using TMPro;
using UnityEngine;

public class FixedStoreIngredientView : ValidatedMonoBehaviour
{
    [SerializeField]
    private IngredientStorePurchaseButton _purchaseButton;

    [SerializeField]
    private TMP_Text _remainingDaysText;

    public void Bind(StoreFixedIngredientOffer offer)
    {
        if (_purchaseButton != null)
            _purchaseButton.Bind(offer.Ingredient);

        if (_remainingDaysText != null)
            _remainingDaysText.SetText($"Dias restantes: {offer.RemainingDays}");
    }
}
