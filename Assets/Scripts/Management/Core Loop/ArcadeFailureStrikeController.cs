using System;
using System.Collections.Generic;

public sealed class ArcadeFailureStrikeController : IDisposable
{
    private readonly LevelRunContext _runContext;
    private readonly StrikesController _strikesController;
    private readonly OrderController _orderController;
    private readonly CustomerQueue _customerQueue;
    private readonly HashSet<Order> _failedOrdersAwaitingFlowEnd = new();
    private readonly HashSet<Customer> _expiredCustomersAwaitingFlowEnd = new();

    public ArcadeFailureStrikeController(
        LevelRunContext runContext,
        StrikesController strikesController,
        OrderController orderController,
        CustomerQueue customerQueue)
    {
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
        _strikesController = strikesController ?? throw new ArgumentNullException(nameof(strikesController));
        _orderController = orderController ?? throw new ArgumentNullException(nameof(orderController));
        _customerQueue = customerQueue ?? throw new ArgumentNullException(nameof(customerQueue));

        _orderController.OrderFailed += OnOrderFailed;
        _orderController.OrderExpired += OnOrderExpired;
        _orderController.OrderFlowFinished += OnOrderFlowFinished;
        _customerQueue.CustomerFlowFinished += OnCustomerFlowFinished;
        _customerQueue.QueueEntryRemoved += OnQueueEntryRemoved;
    }

    public void Dispose()
    {
        _orderController.OrderFailed -= OnOrderFailed;
        _orderController.OrderExpired -= OnOrderExpired;
        _orderController.OrderFlowFinished -= OnOrderFlowFinished;
        _customerQueue.CustomerFlowFinished -= OnCustomerFlowFinished;
        _customerQueue.QueueEntryRemoved -= OnQueueEntryRemoved;
    }

    private void OnOrderFailed(Order order)
    {
        if (!_runContext.IsArcade)
            return;

        _failedOrdersAwaitingFlowEnd.Add(order);
    }

    private void OnOrderExpired(Order order)
    {
        if (!_runContext.IsArcade)
            return;

        _failedOrdersAwaitingFlowEnd.Add(order);
    }

    private void OnOrderFlowFinished(Order order)
    {
        if (!_runContext.IsArcade)
            return;

        if (!_failedOrdersAwaitingFlowEnd.Remove(order))
            return;

        _strikesController.Strike();
    }

    private void OnQueueEntryRemoved(CustomerWaitStatus waitStatus, CustomerQueueEntryRemovedReason reason)
    {
        if (reason != CustomerQueueEntryRemovedReason.Expired)
            return;

        if (!_runContext.IsArcade)
            return;

        _expiredCustomersAwaitingFlowEnd.Add(waitStatus.Customer);
    }

    private void OnCustomerFlowFinished(Customer customer)
    {
        if (!_runContext.IsArcade)
            return;

        if (!_expiredCustomersAwaitingFlowEnd.Remove(customer))
            return;

        _strikesController.Strike();
    }
}
