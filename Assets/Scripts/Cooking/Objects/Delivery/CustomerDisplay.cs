using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class CustomerDisplay : MonoBehaviour
{
    [SerializeField]
    private int _maxVisibleCustomers = 3;

    [SerializeField]
    private Vector3 _customerStartLocalOffset = new(0f, -1.8f, 0f);

    [SerializeField]
    private CustomerDisplaySlot[] _slots;

    [SerializeField]
    private TweenSettings _customerMoveTweenSettings = new(1f, Ease.OutQuad);

    [SerializeField]
    private int _customerSortingOrder = -1;

    private readonly Queue<CustomerWaitStatus> _queuedCustomers = new();
    private readonly Dictionary<CustomerWaitStatus, CustomerDisplaySlot> _visibleWaitingCustomers = new();
    private readonly Dictionary<Order, CustomerDisplaySlot> _visibleOrders = new();

    private OrderController _orderController;
    private GameplayTutorialEvents _tutorialEvents;
    private GameplayInteractionGate _interactionGate;
    private TutorialTargetRegistry _tutorialTargetRegistry;
    private bool _isConfigured;
    private int _configuredSlotsCount;

    [Inject]
    private readonly OrderController _injectedOrderController;

    [Inject]
    private readonly GameplayTutorialEvents _injectedTutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _injectedInteractionGate;

    [Inject]
    private readonly TutorialTargetRegistry _injectedTutorialTargetRegistry;

    [Inject]
    private readonly CustomerDeliveryHintService _hintService;

    [Inject]
    private readonly CustomerQueue _customerQueue;

    [Inject]
    private readonly ICustomerServiceDialogue _customerServiceDialogue;

    [Inject]
    private readonly ICustomerDeliveryDialogue _customerDeliveryDialogue;

    private void Awake()
    {
        Configure(
            _injectedOrderController,
            null,
            _injectedTutorialEvents,
            _injectedInteractionGate,
            _injectedTutorialTargetRegistry);
    }

    public void Configure(
        OrderController orderController,
        DeliverySequence deliverySequence,
        GameplayTutorialEvents tutorialEvents,
        GameplayInteractionGate interactionGate,
        TutorialTargetRegistry tutorialTargetRegistry)
    {
        if (_isConfigured)
            return;

        _orderController = orderController;
        _tutorialEvents = tutorialEvents;
        _interactionGate = interactionGate;
        _tutorialTargetRegistry = tutorialTargetRegistry;

        if (!ConfigureSlots())
            return;

        _isConfigured = true;
        _hintService?.Register(this);
        Subscribe();
        ShowExistingQueuedCustomers();
    }

    private void OnDestroy()
    {
        _hintService?.Unregister(this);
        Unsubscribe();
    }

    public bool HasReceivableCustomer()
    {
        if (_slots == null)
            return false;

        for (var i = 0; i < _configuredSlotsCount; i++)
        {
            var slot = _slots[i];
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

        for (var i = 0; i < _configuredSlotsCount; i++)
        {
            var slot = _slots[i];
            if (slot == null)
                continue;

            if (slot.HasCustomer)
                return true;
        }

        return false;
    }

    public void SetHintsVisible(bool visible)
    {
        if (_slots == null)
            return;

        for (var i = 0; i < _configuredSlotsCount; i++)
        {
            var slot = _slots[i];
            if (slot == null)
                continue;

            slot.SetHintsVisible(visible);
        }
    }

    private void Subscribe()
    {
        _customerQueue.QueueEntryAdded += OnQueueEntryAdded;
        _customerQueue.QueueEntryRemoved += OnQueueEntryRemoved;
        _orderController.OrderExpired += OnOrderExpired;
        _orderController.OrderFlowFinished += OnOrderFlowFinished;
    }

    private void Unsubscribe()
    {
        if (_customerQueue != null)
        {
            _customerQueue.QueueEntryAdded -= OnQueueEntryAdded;
            _customerQueue.QueueEntryRemoved -= OnQueueEntryRemoved;
        }

        if (_orderController != null)
        {
            _orderController.OrderExpired -= OnOrderExpired;
            _orderController.OrderFlowFinished -= OnOrderFlowFinished;
        }
    }

    public void TakeOrder(CustomerDisplaySlot displaySlot, CustomerWaitStatus waitStatus)
    {
        if (displaySlot == null || waitStatus == null || _customerServiceDialogue == null)
            return;

        if (_customerServiceDialogue.IsPlaying)
            return;

        if (!_interactionGate.CanInteract(TutorialInteractionType.TakeOrder))
            return;

        if (!_visibleWaitingCustomers.Remove(waitStatus))
            return;

        displaySlot.HideWaitingServiceIcon();

        if (!_customerQueue.TryTake(waitStatus, out var customer))
        {
            displaySlot.Hide(_customerMoveTweenSettings);
            ShowQueuedCustomersIfPossible();
            return;
        }

        var order = _orderController.AcceptOrder(customer);
        _visibleOrders[order] = displaySlot;
        displaySlot.StartOrder(order);

        _customerServiceDialogue.OrderDialogue(order).ChainCallback(() =>
        {
            if (order.HadMissingIngredients)
            {
                _orderController.FailOrderFromMissingIngredients(order);
                return;
            }

            _orderController.StartOrder(order);
            displaySlot.EnableDelivery();
        });
    }

    private void OnOrderExpired(Order order)
    {
        if (_visibleOrders.TryGetValue(order, out var slot))
            slot.DisableDelivery();
    }

    private void OnOrderFlowFinished(Order order)
    {
        if (!TryRemoveVisibleOrder(order))
            return;

        ShowQueuedCustomersIfPossible();
    }

    private void OnQueueEntryAdded(CustomerWaitStatus waitStatus)
    {
        if (!TryShowCustomer(waitStatus))
            _queuedCustomers.Enqueue(waitStatus);
    }

    private void OnQueueEntryRemoved(CustomerWaitStatus waitStatus, CustomerQueueEntryRemovedReason reason)
    {
        if (TryRemoveVisibleCustomer(waitStatus))
        {
            ShowQueuedCustomersIfPossible();
            return;
        }

        RemoveQueuedCustomer(waitStatus);
    }

    private void ShowExistingQueuedCustomers()
    {
        foreach (var waitStatus in _customerQueue.Entries)
        {
            if (_visibleWaitingCustomers.ContainsKey(waitStatus))
                continue;

            if (_queuedCustomers.Contains(waitStatus))
                continue;

            if (!TryShowCustomer(waitStatus))
                _queuedCustomers.Enqueue(waitStatus);
        }
    }

    private bool TryShowCustomer(CustomerWaitStatus waitStatus)
    {
        if (_slots == null)
            return false;

        for (var i = 0; i < _configuredSlotsCount; i++)
        {
            var slot = _slots[i];
            if (slot == null)
                continue;

            if (!slot.IsAvailable)
                continue;

            slot.ShowWaitingCustomer(waitStatus, _customerMoveTweenSettings);
            _visibleWaitingCustomers[waitStatus] = slot;
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

    private bool TryRemoveVisibleCustomer(CustomerWaitStatus waitStatus)
    {
        if (!_visibleWaitingCustomers.Remove(waitStatus, out var slot))
            return false;

        slot.Hide(_customerMoveTweenSettings);
        return true;
    }

    private void ShowQueuedCustomersIfPossible()
    {
        while (_queuedCustomers.Count > 0 && TryShowCustomer(_queuedCustomers.Peek()))
            _queuedCustomers.Dequeue();
    }

    private void RemoveQueuedCustomer(CustomerWaitStatus waitStatus)
    {
        if (_queuedCustomers.Count == 0)
            return;

        var remainingCustomers = new Queue<CustomerWaitStatus>(_queuedCustomers.Count);
        while (_queuedCustomers.Count > 0)
        {
            var queuedCustomer = _queuedCustomers.Dequeue();
            if (queuedCustomer != waitStatus)
                remainingCustomers.Enqueue(queuedCustomer);
        }

        while (remainingCustomers.Count > 0)
            _queuedCustomers.Enqueue(remainingCustomers.Dequeue());
    }

    private bool ConfigureSlots()
    {
        if (_slots == null || _slots.Length == 0)
        {
            Debug.LogError($"{nameof(CustomerDisplay)} on '{name}' must have scene-prepared customer slots assigned.", this);
            return false;
        }

        if (_customerQueue == null)
        {
            Debug.LogError($"{nameof(CustomerDisplay)} on '{name}' could not resolve {nameof(CustomerQueue)}.", this);
            return false;
        }

        if (_customerServiceDialogue == null)
        {
            Debug.LogError($"{nameof(CustomerDisplay)} on '{name}' could not resolve {nameof(ICustomerServiceDialogue)}.", this);
            return false;
        }

        if (_customerDeliveryDialogue == null)
        {
            Debug.LogError($"{nameof(CustomerDisplay)} on '{name}' could not resolve {nameof(ICustomerDeliveryDialogue)}.", this);
            return false;
        }

        _configuredSlotsCount = Mathf.Min(Mathf.Max(1, _maxVisibleCustomers), _slots.Length);
        for (var i = 0; i < _configuredSlotsCount; i++)
        {
            if (_slots[i] == null)
            {
                Debug.LogError($"{nameof(CustomerDisplay)} on '{name}' has an empty slot reference at index {i}.", this);
                continue;
            }

            _slots[i].Configure(
                this,
                _tutorialEvents,
                _interactionGate,
                _tutorialTargetRegistry,
                _customerStartLocalOffset,
                _customerDeliveryDialogue,
                _customerSortingOrder);
        }

        return true;
    }
}
