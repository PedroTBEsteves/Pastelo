using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

public interface ICustomerDeliveryDialogue
{
    Sequence DeliveryDialogue(
        Order order,
        Delivery delivery,
        CustomerAnimationController customerAnimation,
        Vector3 dialogueWorldPosition,
        GameObject deliveryBag,
        VisualEffect happyVisualEffect);
}
