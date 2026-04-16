using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteTooltipIngredientRow : MonoBehaviour
{
    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _amount;

    public bool Bind(Filling filling, int amount)
    {
        if (filling == null || _icon == null || _amount == null)
            return false;

        _icon.sprite = filling.OrderIcon;
        _amount.SetText($"x{amount}");
        return true;
    }
}
