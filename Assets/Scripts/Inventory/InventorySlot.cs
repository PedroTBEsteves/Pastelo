using System;
using UnityEngine;

[Serializable]
public sealed class InventorySlot
{
    [SerializeField]
    private ItemDefinition _item;

    [SerializeField]
    private int _quantity;

    [NonSerialized]
    private Inventory _owner;

    public event Action<InventorySlot> Changed;

    public InventorySlot(ItemDefinition item, Inventory owner, int quantity = 0)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _item = item;
        _quantity = 0;
        _owner = owner;

        if (quantity > 0)
            Add(quantity);
    }

    public ItemDefinition Item => _item;
    public int Quantity => _quantity;
    public bool IsUnlimited => _item != null && _item.MaxStack <= 0;
    public int RemainingCapacity => IsUnlimited ? int.MaxValue : Mathf.Max(0, _item.MaxStack - _quantity);

    public void Add(int amount)
    {
        if (amount <= 0 || _item == null)
            return;

        var quantityBefore = _quantity;

        if (IsUnlimited)
        {
            _quantity += amount;
            NotifyChangedIfNeeded(quantityBefore);
            return;
        }

        _quantity = Mathf.Min(_quantity + amount, _item.MaxStack);
        NotifyChangedIfNeeded(quantityBefore);
    }

    public void Remove(int amount)
    {
        if (amount <= 0 || _quantity <= 0)
            return;

        var quantityBefore = _quantity;
        _quantity = Mathf.Max(0, _quantity - amount);
        NotifyChangedIfNeeded(quantityBefore);

        if (_quantity == 0)
            _owner?.RemoveSlot(this);
    }

    private void NotifyChangedIfNeeded(int quantityBefore)
    {
        if (quantityBefore != _quantity)
            Changed?.Invoke(this);
    }
}
