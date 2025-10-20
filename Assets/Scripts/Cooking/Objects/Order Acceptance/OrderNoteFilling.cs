using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteFilling : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Image _icon;

    public void Initialize(Filling filling, int amount)
    {
        _icon.sprite = filling.Icon;
        _text.SetText($"x{amount}");
    }
}
