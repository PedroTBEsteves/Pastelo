using PrimeTween;

public interface ICustomerDeliveryDialogue
{
    Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController);

    bool IsPlaying { get; }
}
