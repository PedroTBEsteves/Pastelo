using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Inventory
{
    private readonly List<InventorySlot> _slots = new();

    public event Action<InventorySlot> SlotAdded;
    public event Action<InventorySlot> SlotRemoved;

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public bool Contains(ItemDefinition item, int amount = 1)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return GetQuantity(item) >= amount;
    }

    public int GetQuantity(ItemDefinition item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        return _slots.Where(slot => slot.Item == item).Sum(slot => slot.Quantity);
    }

    public void Add(ItemDefinition item, int amount = 1)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (amount <= 0)
            return;

        var remainingAmount = amount;
        var firstAvailableSlot = FindFirstAvailableSlot(item);

        if (firstAvailableSlot != null)
        {
            var amountToAdd = firstAvailableSlot.IsUnlimited
                ? remainingAmount
                : Math.Min(remainingAmount, firstAvailableSlot.RemainingCapacity);

            firstAvailableSlot.Add(amountToAdd);
            remainingAmount -= amountToAdd;
        }

        while (remainingAmount > 0)
        {
            var slotAmount = item.MaxStack <= 0 ? remainingAmount : Math.Min(remainingAmount, item.MaxStack);
            var newSlot = new InventorySlot(item, this, slotAmount);
            _slots.Add(newSlot);
            SlotAdded?.Invoke(newSlot);
            remainingAmount -= slotAmount;
        }
    }

    public void Remove(ItemDefinition item, int amount = 1)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (amount <= 0)
            return;

        var remainingAmount = amount;

        for (var i = 0; i < _slots.Count && remainingAmount > 0; i++)
        {
            var slot = _slots[i];

            if (slot.Item != item)
                continue;

            var quantityBeforeRemoval = slot.Quantity;
            slot.Remove(remainingAmount);
            remainingAmount -= quantityBeforeRemoval - slot.Quantity;

            if (i < _slots.Count && _slots[i] != slot)
                i--;
        }
    }

    public void RemoveSlot(InventorySlot slot)
    {
        if (slot == null)
            return;

        if (_slots.Remove(slot))
            SlotRemoved?.Invoke(slot);
    }

    private InventorySlot FindFirstAvailableSlot(ItemDefinition item)
    {
        return _slots.Where(slot => slot.Item == item).FirstOrDefault(slot => slot.IsUnlimited || slot.RemainingCapacity > 0);
    }
}
