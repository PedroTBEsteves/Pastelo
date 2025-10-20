using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderTakerBell : ValidatedMonoBehaviour, IPointerDownHandler
{
    [Inject]
    private readonly CustomerQueue _customerQueue;
    
    [Inject]
    private readonly OrderController _orderController;
    
    [Inject]
    private readonly ICustomerDialogue _customerDialogue;

    [SerializeField]
    private AudioSource _bellSound;

    private Tween _delay;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_delay.isAlive || !_customerQueue.TryGetNext(out var customer))
            return;

        var order = _orderController.AcceptOrder(customer);

        _customerDialogue.OrderDialogue(order).ChainCallback(() => _orderController.StartOrder(order));
        _bellSound.Play();
    }
}
