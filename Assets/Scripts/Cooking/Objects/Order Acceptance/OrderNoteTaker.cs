using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteTaker : MonoBehaviour
{
    [SerializeField]
    private int _maxVisibleOrders = 4;

    private sealed class VisibleOrderState
    {
        public VisibleOrderState(OrderNote orderNote)
        {
            OrderNote = orderNote;
        }

        public OrderNote OrderNote { get; }
        public bool IsAwaitingFlowFinish { get; set; }
    }

    [SerializeField]
    private OrderNote _orderNotePrefab;
    
    [Inject]
    private OrderController _orderController;

    private readonly Queue<Order> _queuedOrders = new();
    private readonly Dictionary<Order, VisibleOrderState> _visibleOrders = new();

    private void Awake()
    {
        _orderController.OrderStarted += OnOrderStarted;
        _orderController.OrderExpired += OnOrderExpired;
        _orderController.OrderSucceeded += OnOrderResolved;
        _orderController.OrderFailed += OnOrderResolved;
        _orderController.OrderFlowFinished += OnOrderFlowFinished;
    }

    private void OnDestroy()
    {
        _orderController.OrderStarted -= OnOrderStarted;
        _orderController.OrderExpired -= OnOrderExpired;
        _orderController.OrderSucceeded -= OnOrderResolved;
        _orderController.OrderFailed -= OnOrderResolved;
        _orderController.OrderFlowFinished -= OnOrderFlowFinished;
    }

    private void OnOrderStarted(Order order)
    {
        if (_visibleOrders.Count >= _maxVisibleOrders)
        {
            _queuedOrders.Enqueue(order);
            return;
        }

        ShowOrder(order);
    }

    private void OnOrderExpired(Order order)
    {
        if (_visibleOrders.Remove(order))
        {
            ShowQueuedOrdersIfPossible();
            return;
        }

        RemoveQueuedOrder(order);
    }

    private void OnOrderResolved(Order order)
    {
        if (_visibleOrders.TryGetValue(order, out var visibleOrderState))
            visibleOrderState.IsAwaitingFlowFinish = true;
    }

    private void OnOrderFlowFinished(Order order)
    {
        if (!_visibleOrders.Remove(order))
            return;

        ShowQueuedOrdersIfPossible();
    }

    private void ShowQueuedOrdersIfPossible()
    {
        while (_visibleOrders.Count < _maxVisibleOrders && _queuedOrders.Count > 0)
            ShowOrder(_queuedOrders.Dequeue());
    }

    private void ShowOrder(Order order)
    {
        var orderNote = Instantiate(_orderNotePrefab, transform);
        orderNote.Initialize(order);
        _visibleOrders[order] = new VisibleOrderState(orderNote);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        orderNote.PostInitialize();
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
}
