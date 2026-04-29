using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class DeliveryCustomerDisplay : MonoBehaviour
{
    [SerializeField]
    private int _maxVisibleCustomers = 3;

    [SerializeField]
    private Vector3 _customerStartLocalOffset = new(0f, -1.8f, 0f);

    [SerializeField]
    private DeliveryCustomerSlot[] _slots;

    [SerializeField]
    private TweenSettings _customerMoveTweenSettings = new(1f, Ease.OutQuad);

    [SerializeField]
    private int _customerSortingOrder = -1;

    private readonly Queue<Order> _queuedOrders = new();
    private readonly Dictionary<Order, DeliveryCustomerSlot> _visibleOrders = new();

    private OrderController _orderController;
    private DeliverySequence _deliverySequence;
    private GameplayTutorialEvents _tutorialEvents;
    private GameplayInteractionGate _interactionGate;
    private TutorialTargetRegistry _tutorialTargetRegistry;

    [Inject]
    private readonly ICustomerDeliveryDialogue _customerDeliveryDialogue;

    public void Configure(
        OrderController orderController,
        DeliverySequence deliverySequence,
        GameplayTutorialEvents tutorialEvents,
        GameplayInteractionGate interactionGate,
        TutorialTargetRegistry tutorialTargetRegistry)
    {
        _orderController = orderController;
        _deliverySequence = deliverySequence;
        _tutorialEvents = tutorialEvents;
        _interactionGate = interactionGate;
        _tutorialTargetRegistry = tutorialTargetRegistry;

        ConfigureSlots();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public bool HasReceivableCustomer()
    {
        if (_slots == null)
            return false;

        foreach (var slot in _slots)
        {
            if (slot == null)
                continue;

            if (slot.CanReceiveDelivery())
                return true;
        }

        return false;
    }

    public bool HasVisibleCustomer()
    {
        if (_slots == null)
            return false;

        foreach (var slot in _slots)
        {
            if (slot == null)
                continue;

            if (slot.Order != null)
                return true;
        }

        return false;
    }

    public void SetHintsVisible(bool visible)
    {
        if (_slots == null)
            return;

        foreach (var slot in _slots)
        {
            if (slot == null)
                continue;

            slot.SetHintsVisible(visible);
        }
    }

    private void Subscribe()
    {
        _orderController.OrderStarted += OnOrderStarted;
        _orderController.OrderExpired += OnOrderExpired;
        _orderController.OrderFlowFinished += OnOrderFlowFinished;
    }

    private void Unsubscribe()
    {
        if (_orderController == null)
            return;

        _orderController.OrderStarted -= OnOrderStarted;
        _orderController.OrderExpired -= OnOrderExpired;
        _orderController.OrderFlowFinished -= OnOrderFlowFinished;
    }

    private void OnOrderStarted(Order order)
    {
        if (!TryShowOrder(order))
            _queuedOrders.Enqueue(order);
    }

    private void OnOrderExpired(Order order)
    {
        if (TryRemoveVisibleOrder(order))
        {
            ShowQueuedOrdersIfPossible();
            return;
        }

        RemoveQueuedOrder(order);
    }

    private void OnOrderFlowFinished(Order order)
    {
        if (!TryRemoveVisibleOrder(order))
            return;

        ShowQueuedOrdersIfPossible();
    }

    private bool TryShowOrder(Order order)
    {
        if (_slots == null)
            return false;

        foreach (var slot in _slots)
        {
            if (slot == null)
                continue;

            if (!slot.IsAvailable)
                continue;

            slot.Show(order, _customerMoveTweenSettings);
            _visibleOrders[order] = slot;
            return true;
        }

        return false;
    }

    private bool TryRemoveVisibleOrder(Order order)
    {
        if (!_visibleOrders.Remove(order, out var slot))
            return false;

        slot.Hide(_customerMoveTweenSettings);
        return true;
    }

    private void ShowQueuedOrdersIfPossible()
    {
        while (_queuedOrders.Count > 0 && TryShowOrder(_queuedOrders.Peek()))
            _queuedOrders.Dequeue();
    }

    private void RemoveQueuedOrder(Order order)
    {
        if (_queuedOrders.Count == 0)
            return;

        var remainingOrders = new Queue<Order>(_queuedOrders.Count);
        while (_queuedOrders.Count > 0)
        {
            var queuedOrder = _queuedOrders.Dequeue();
            if (queuedOrder != order)
                remainingOrders.Enqueue(queuedOrder);
        }

        while (remainingOrders.Count > 0)
            _queuedOrders.Enqueue(remainingOrders.Dequeue());
    }

    private void ConfigureSlots()
    {
        if (_slots == null || _slots.Length == 0)
        {
            Debug.LogError($"{nameof(DeliveryCustomerDisplay)} on '{name}' must have scene-prepared customer slots assigned.", this);
            return;
        }

        if (_customerDeliveryDialogue == null)
        {
            Debug.LogError($"{nameof(DeliveryCustomerDisplay)} on '{name}' could not resolve {nameof(ICustomerDeliveryDialogue)}.", this);
            return;
        }

        var slotsCount = Mathf.Min(Mathf.Max(1, _maxVisibleCustomers), _slots.Length);
        for (var i = 0; i < slotsCount; i++)
        {
            if (_slots[i] == null)
            {
                Debug.LogError($"{nameof(DeliveryCustomerDisplay)} on '{name}' has an empty slot reference at index {i}.", this);
                continue;
            }

            _slots[i].Configure(
                _tutorialEvents,
                _interactionGate,
                _tutorialTargetRegistry,
                _customerStartLocalOffset,
                _customerDeliveryDialogue,
                _customerSortingOrder);
        }
    }
}
