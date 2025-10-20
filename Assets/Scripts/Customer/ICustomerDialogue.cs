using PrimeTween;
using UnityEngine;

public interface ICustomerDialogue
{
    Sequence OrderDialogue(Order order);
    Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController);
    
    bool IsPlaying { get; }
}
