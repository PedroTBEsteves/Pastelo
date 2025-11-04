using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerQueue : ITickable
{
    private readonly float _customerWaitTime;
    
    private readonly float _minCustomerArrivalTime;
    private readonly float _maxCustomerArrivalTime;
    private float _nextArrivalTime;
    private float _elapsedArrivalTime;
    
    private readonly Queue<CustomerWaitStatus> _queue = new();

    private readonly CustomersDatabase _customers;
    private readonly StrikesController _strikesController;
    private readonly ICustomerPopUpDialogue _customerPopUpDialogue;

    public CustomerQueue(OrderLoopSettings orderLoopSettings, CustomersDatabase customers, StrikesController strikesController, ICustomerPopUpDialogue customerPopUpDialogue)
    {
        _customers = customers;
        _strikesController = strikesController;
        _customerPopUpDialogue = customerPopUpDialogue;
        _customerWaitTime = orderLoopSettings.QueueWaitTimeLimit;
        _minCustomerArrivalTime = orderLoopSettings.MinCustomerArrivalTime;
        _maxCustomerArrivalTime = orderLoopSettings.MaxCustomerArrivalTime;
        _nextArrivalTime = 1f;
    }
    
    public event Action<Customer> CustomerArrived = delegate { };
    public event Action<Customer> CustomerExpired = delegate { };
    public event Action<int> CustomersCountChanged = delegate { };

    public bool TryGetNext(out Customer customer)
    {
        customer = null;
        var hasCustomer = _queue.TryDequeue(out var waitStatus);

        if (hasCustomer)
        {
            customer = waitStatus.Customer;
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
        CheckForCustomerArrival(deltaTime);
        AdvanceWaitStatuses(deltaTime);
    }

    private void CheckForCustomerArrival(float deltaTime)
    {
        _elapsedArrivalTime += deltaTime;

        if (!(_elapsedArrivalTime >= _nextArrivalTime)) 
            return;

        var customer = _customers.GetRandom();
        var waitStatus = new CustomerWaitStatus(customer, _customerWaitTime);
        _queue.Enqueue(waitStatus);
        CustomersCountChanged(_queue.Count);
        _elapsedArrivalTime -= _nextArrivalTime;
        CustomerArrived(customer);
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
            CustomersCountChanged(_queue.Count);
            _customerPopUpDialogue.CustomerGaveUpDialogue(first.Customer)
                .ChainCallback(_strikesController.Strike);
        }
    }
    
    private float GetNextArrivalTime() => Random.Range(_minCustomerArrivalTime, _maxCustomerArrivalTime);
}
