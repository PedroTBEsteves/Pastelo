using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
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
    private readonly ICustomerPopUpDialogue _customerPopUpDialogue;
    private readonly LocalizedStringTable _customerGaveUpDialoguesTable;
    private readonly int _maxQueueCapacity;
    private readonly int _recentCustomersRepeatWindow;

    private bool _arrivalsEnabled = true;
    private bool _hasTutorialStarted;
    private bool _hasConfiguredFirstCustomerAfterTutorial;
    private bool HasQueueCapacityLimit => _maxQueueCapacity > 0;
    private bool IsQueueFull => HasQueueCapacityLimit && _queue.Count >= _maxQueueCapacity;
    private bool IsPausedByTutorial => _tutorialState.IsActive && _tutorialState.CurrentStep != TutorialStep.WaitForCustomer;

    public CustomerQueue(
        OrderLoopSettings orderLoopSettings,
        CustomersDatabase customers,
        ICustomerPopUpDialogue customerPopUpDialogue,
        LocalizedStringTable customerGaveUpDialoguesTable,
        GameplayTutorialState tutorialState,
        LevelSelector levelSelector)
    {
        if (levelSelector == null)
            throw new ArgumentNullException(nameof(levelSelector));

        var selectedLevel = levelSelector.SelectedLevel;
        if (selectedLevel == null)
            throw new InvalidOperationException($"{nameof(CustomerQueue)} requires a selected {nameof(Level)}.");

        _customers = customers;
        _customerPopUpDialogue = customerPopUpDialogue;
        _customerGaveUpDialoguesTable = customerGaveUpDialoguesTable;
        _tutorialState = tutorialState;
        _customerWaitTime = orderLoopSettings.QueueWaitTimeLimit;
        _minCustomerArrivalTime = orderLoopSettings.MinCustomerArrivalTime;
        _maxCustomerArrivalTime = orderLoopSettings.MaxCustomerArrivalTime;
        _maxQueueCapacity = orderLoopSettings.MaxQueueCapacity;
        _recentCustomersRepeatWindow = Mathf.Max(0, orderLoopSettings.RecentCustomersRepeatWindow);
        _firstCustomerArrivalDelayAfterTutorial = Mathf.Max(0f, orderLoopSettings.FirstCustomerArrivalDelayAfterTutorial);
        _nextArrivalTime = 1f;
    }
    
    public event Action<Customer> CustomerArrived = delegate { };
    public event Action<Customer> CustomerExpired = delegate { };
    public event Action<Customer> CustomerFlowFinished = delegate { };
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

    public void StopArrivals()
    {
        _arrivalsEnabled = false;
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
        if (!_arrivalsEnabled)
            return;

        if (IsQueueFull)
            return;

        _elapsedArrivalTime += deltaTime;

        if (!(_elapsedArrivalTime >= _nextArrivalTime)) 
            return;

        var customer = GetNextCustomer();
        var waitStatus = new CustomerWaitStatus(customer, _customerWaitTime);
        _queue.Enqueue(waitStatus);
        RememberRecentCustomer(customer);
        QueueEntryAdded(waitStatus);
        CustomersCountChanged(_queue.Count);
        _elapsedArrivalTime -= _nextArrivalTime;
        CustomerArrived(customer);

        if (_arrivalsEnabled)
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
            var dialogue = CustomerDialogueLocalization.GetRandomLocalizedDialogue(
                _customerGaveUpDialoguesTable,
                nameof(CustomerQueue),
                nameof(_customerGaveUpDialoguesTable));
            _customerPopUpDialogue.ShowDialogue(first.Customer, dialogue)
                .ChainCallback(() =>
                {
                    CustomerFlowFinished(first.Customer);
                });
        }
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
