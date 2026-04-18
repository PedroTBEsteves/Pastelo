using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _quantityText;

    [SerializeField]
    private GameObject _quantityRoot;

    private InventorySlot _slot;

    public void Bind(InventorySlot slot)
    {
        Unsubscribe();

        _slot = slot;

        if (_slot != null)
            _slot.Changed += OnSlotChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnSlotChanged(InventorySlot _)
    {
        Refresh();
    }

    private void Refresh()
    {
        var item = _slot?.Item;

        if (_iconImage != null)
            _iconImage.sprite = item != null ? item.Icon : null;

        if (_nameText != null)
            _nameText.SetText(item != null ? item.GetDisplayName() : string.Empty);

        var shouldShowQuantity = item != null && item.MaxStack != 1;

        if (_quantityRoot != null)
            _quantityRoot.SetActive(shouldShowQuantity);
        else if (_quantityText != null)
            _quantityText.gameObject.SetActive(shouldShowQuantity);

        if (_quantityText != null)
            _quantityText.SetText(shouldShowQuantity ? _slot.Quantity.ToString() : string.Empty);
    }

    private void Unsubscribe()
    {
        if (_slot == null)
            return;

        _slot.Changed -= OnSlotChanged;
        _slot = null;
    }
}
