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

        if (IsUnlimited)
        {
            _quantity += amount;
            return;
        }

        _quantity = Mathf.Min(_quantity + amount, _item.MaxStack);
    }

    public void Remove(int amount)
    {
        if (amount <= 0 || _quantity <= 0)
            return;

        _quantity = Mathf.Max(0, _quantity - amount);

        if (_quantity == 0)
            _owner?.RemoveSlot(this);
    }
}
