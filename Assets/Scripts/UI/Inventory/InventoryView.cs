using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField]
    private Transform _slotsRoot;

    [SerializeField]
    private InventorySlotView _slotPrefab;

    [Inject]
    private readonly Inventory _inventory;

    private readonly List<InventorySlotView> _slotViews = new();

    private void Awake()
    {
        if (_inventory == null)
            return;

        _inventory.SlotAdded += OnSlotAdded;
        _inventory.SlotRemoved += OnSlotRemoved;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_inventory == null)
            return;

        _inventory.SlotAdded -= OnSlotAdded;
        _inventory.SlotRemoved -= OnSlotRemoved;
    }

    private void OnSlotAdded(InventorySlot _)
    {
        Rebuild();
    }

    private void OnSlotRemoved(InventorySlot _)
    {
        Rebuild();
    }

    private void Rebuild()
    {
        ClearViews();

        if (_slotPrefab == null || _inventory == null)
            return;

        var root = _slotsRoot != null ? _slotsRoot : transform;

        for (var index = 0; index < _inventory.Slots.Count; index++)
        {
            var slot = _inventory.Slots[index];
            var slotView = Instantiate(_slotPrefab, root);
            slotView.name = $"Inventory Slot {index}";
            slotView.Bind(slot);
            _slotViews.Add(slotView);
        }
    }

    private void ClearViews()
    {
        for (var index = 0; index < _slotViews.Count; index++)
        {
            if (_slotViews[index] == null)
                continue;

            _slotViews[index].Bind(null);
            Destroy(_slotViews[index].gameObject);
        }

        _slotViews.Clear();
    }
}
