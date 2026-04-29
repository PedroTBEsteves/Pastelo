public class DeliverySequence
{
    private readonly OrderController _orderController;
    private readonly StrikesController _strikesController;

    public DeliverySequence(OrderController orderController, StrikesController strikesController)
    {
        _orderController = orderController;
        _strikesController = strikesController;
    }
    
    public void StartSequence(Order order, Delivery delivery)
    {
        Deliver(order, delivery);
        FinishOrderFlow(order);
    }

    public bool Deliver(Order order, Delivery delivery)
    {
        var isCorrect = delivery.IsCorrectFor(order);
        _orderController.DeliverOrder(order, delivery);

        return isCorrect;
    }

    public void FinishOrderFlow(Order order)
    {
        _orderController.FinishOrderFlow(order);
    }
}
