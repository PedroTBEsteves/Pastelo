using System;

public sealed class LevelPerformanceTracker : IDisposable
{
    private readonly OrderController _orderController;
    private readonly CustomerQueue _customerQueue;
    private readonly EventBinding<PastelBurntEvent> _pastelBurntBinding;

    public LevelPerformanceTracker(
        OrderController orderController,
        CustomerQueue customerQueue)
    {
        _orderController = orderController;
        _customerQueue = customerQueue;
        _pastelBurntBinding = new EventBinding<PastelBurntEvent>(OnPastelBurnt);

        _orderController.OrderSucceeded += OnOrderSucceeded;
        _orderController.OrderFailed += OnOrderFailed;
        _orderController.OrderExpired += OnOrderExpired;
        _orderController.OrderHadMissingIngredients += OnOrderHadMissingIngredients;
        _customerQueue.QueueEntryRemoved += OnQueueEntryRemoved;
        EventBus<PastelBurntEvent>.Register(_pastelBurntBinding);
    }

    public int SuccessfulOrdersCount { get; private set; }
    public int FailedOrdersCount { get; private set; }
    public int OrdersMissingIngredientsCount { get; private set; }
    public int BurntPastelsCount { get; private set; }
    public int QueueAbandonmentsCount { get; private set; }
    public int PostServiceAbandonmentsCount { get; private set; }

    public void Dispose()
    {
        _orderController.OrderSucceeded -= OnOrderSucceeded;
        _orderController.OrderFailed -= OnOrderFailed;
        _orderController.OrderExpired -= OnOrderExpired;
        _orderController.OrderHadMissingIngredients -= OnOrderHadMissingIngredients;
        _customerQueue.QueueEntryRemoved -= OnQueueEntryRemoved;
        EventBus<PastelBurntEvent>.Deregister(_pastelBurntBinding);
    }

    private void OnOrderSucceeded(Order _)
    {
        SuccessfulOrdersCount++;
    }

    private void OnOrderFailed(Order _)
    {
        FailedOrdersCount++;
    }

    private void OnOrderExpired(Order _)
    {
        PostServiceAbandonmentsCount++;
    }

    private void OnOrderHadMissingIngredients(Order _)
    {
        OrdersMissingIngredientsCount++;
    }

    private void OnQueueEntryRemoved(CustomerWaitStatus _, CustomerQueueEntryRemovedReason reason)
    {
        if (reason != CustomerQueueEntryRemovedReason.Expired)
            return;

        QueueAbandonmentsCount++;
    }

    private void OnPastelBurnt(PastelBurntEvent _)
    {
        BurntPastelsCount++;
    }
}
