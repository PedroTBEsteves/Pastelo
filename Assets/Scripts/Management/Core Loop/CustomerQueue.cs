using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerQueue : ITickable
{
    private readonly float _customerWaitTime;
    private readonly GameplayTutorialState _tutorialState;
    private readonly float _firstCustomerArrivalDelayAfterTutorial;
    
    private readonly float _minCustomerArrivalTime;
    private readonly float _maxCustomerArrivalTime;
    private float _nextArrivalTime;
    private float _elapsedArrivalTime;
    
    private readonly Queue<CustomerWaitStatus> _queue = new();
    private readonly Queue<Customer> _recentCustomers = new();
    private readonly HashSet<Customer> _recentCustomersLookup = new();

    private readonly CustomersDatabase _customers;
    private readonly OrderController _orderController;
    private readonly StrikesController _strikesController;
    private readonly ICustomerPopUpDialogue _customerPopUpDialogue;
    private readonly int _maxCustomers;
    private readonly int _recentCustomersRepeatWindow;

    private int _generatedCustomers;
    private int _resolvedCustomers;
    private bool _hasTutorialStarted;
    private bool _hasConfiguredFirstCustomerAfterTutorial;

    private bool HasCustomersLimit => _maxCustomers > 0;
    private bool HasGeneratedAllCustomers => HasCustomersLimit && _generatedCustomers >= _maxCustomers;
    private bool IsPausedByTutorial => _tutorialState.IsActive && _tutorialState.CurrentStep != TutorialStep.WaitForCustomer;

    public CustomerQueue(OrderLoopSettings orderLoopSettings, CustomersDatabase customers, OrderController orderController, StrikesController strikesController, ICustomerPopUpDialogue customerPopUpDialogue, GameplayTutorialState tutorialState)
    {
        _customers = customers;
        _orderController = orderController;
        _strikesController = strikesController;
        _customerPopUpDialogue = customerPopUpDialogue;
        _tutorialState = tutorialState;
        _customerWaitTime = orderLoopSettings.QueueWaitTimeLimit;
        _minCustomerArrivalTime = orderLoopSettings.MinCustomerArrivalTime;
        _maxCustomerArrivalTime = orderLoopSettings.MaxCustomerArrivalTime;
        _recentCustomersRepeatWindow = Mathf.Max(0, orderLoopSettings.RecentCustomersRepeatWindow);
        _firstCustomerArrivalDelayAfterTutorial = Mathf.Max(0f, orderLoopSettings.FirstCustomerArrivalDelayAfterTutorial);
        _maxCustomers = orderLoopSettings.MaxCustomers;
        _nextArrivalTime = 1f;

        _orderController.OrderFlowFinished += OnOrderFinished;
    }
    
    public event Action<Customer> CustomerArrived = delegate { };
    public event Action<Customer> CustomerExpired = delegate { };
    public event Action<int> CustomersCountChanged = delegate { };
    public event Action<CustomerWaitStatus> QueueEntryAdded = delegate { };
    public event Action<CustomerWaitStatus, CustomerQueueEntryRemovedReason> QueueEntryRemoved = delegate { };

    public IEnumerable<CustomerWaitStatus> Entries => _queue;

    public bool TryGetNext(out Customer customer)
    {
        customer = null;
        var hasCustomer = _queue.TryDequeue(out var waitStatus);

        if (hasCustomer)
        {
            customer = waitStatus.Customer;
            QueueEntryRemoved(waitStatus, CustomerQueueEntryRemovedReason.TakenForService);
            CustomersCountChanged(_queue.Count);
        }

        return hasCustomer;
    }

    public bool TryPeek(out Customer customer)
    {
        var hasNext =  _queue.TryPeek(out var status);
        customer = hasNext ? status.Customer : null;
        return hasNext;
    }
    
    public void Tick(float deltaTime)
    {
        SyncTutorialArrivalDelay();

        if (IsPausedByTutorial)
            return;

        CheckForCustomerArrival(deltaTime);
        AdvanceWaitStatuses(deltaTime);
    }

    private void CheckForCustomerArrival(float deltaTime)
    {
        if (HasGeneratedAllCustomers)
            return;

        _elapsedArrivalTime += deltaTime;

        if (!(_elapsedArrivalTime >= _nextArrivalTime)) 
            return;

        var customer = GetNextCustomer();
        var waitStatus = new CustomerWaitStatus(customer, _customerWaitTime);
        _queue.Enqueue(waitStatus);
        RememberRecentCustomer(customer);
        _generatedCustomers++;
        QueueEntryAdded(waitStatus);
        CustomersCountChanged(_queue.Count);
        _elapsedArrivalTime -= _nextArrivalTime;
        CustomerArrived(customer);

        if (!HasGeneratedAllCustomers)
            _nextArrivalTime = GetNextArrivalTime();
    }

    private void AdvanceWaitStatuses(float deltaTime)
    {
        foreach (var waitStatus in _queue)
            waitStatus.Tick(deltaTime);

        if (!_queue.TryPeek(out var first))
            return;

        if (first.IsExpired())
        {
            CustomerExpired(first.Customer);
            _queue.Dequeue();
            QueueEntryRemoved(first, CustomerQueueEntryRemovedReason.Expired);
            CustomersCountChanged(_queue.Count);
            _customerPopUpDialogue.CustomerGaveUpDialogue(first.Customer)
                .ChainCallback(() =>
                {
                    _strikesController.Strike();
                    ResolveCustomerFlow();
                });
        }
    }

    private void OnOrderFinished(Order _)
    {
        ResolveCustomerFlow();
    }

    private void ResolveCustomerFlow()
    {
        _resolvedCustomers++;

        if (!HasGeneratedAllCustomers || _resolvedCustomers < _generatedCustomers)
            return;

        _strikesController.EndGame();
    }

    private void SyncTutorialArrivalDelay()
    {
        if (_tutorialState.IsActive)
            _hasTutorialStarted = true;

        if (!_hasTutorialStarted || _hasConfiguredFirstCustomerAfterTutorial || !_tutorialState.IsFinished)
            return;

        _hasConfiguredFirstCustomerAfterTutorial = true;
        _elapsedArrivalTime = 0f;
        _nextArrivalTime = _firstCustomerArrivalDelayAfterTutorial;
    }

    private Customer GetNextCustomer()
    {
        if (_recentCustomersRepeatWindow <= 0)
            return _customers.GetRandom();

        return _customers.GetRandomExcluding(_recentCustomersLookup);
    }

    private void RememberRecentCustomer(Customer customer)
    {
        if (_recentCustomersRepeatWindow <= 0)
            return;

        _recentCustomers.Enqueue(customer);
        _recentCustomersLookup.Add(customer);

        while (_recentCustomers.Count > _recentCustomersRepeatWindow)
        {
            var removedCustomer = _recentCustomers.Dequeue();

            if (_recentCustomers.Contains(removedCustomer))
                continue;

            _recentCustomersLookup.Remove(removedCustomer);
        }
    }
    
    private float GetNextArrivalTime() => Random.Range(_minCustomerArrivalTime, _maxCustomerArrivalTime);
}
