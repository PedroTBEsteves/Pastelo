using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.VFX;

public class CustomerDeliveryDialogue : MonoBehaviour, ICustomerDeliveryDialogue
{
    private sealed class DeliveryDialogueContext
    {
        public Order Order;
        public Customer Customer;
        public CustomerAnimationController CustomerAnimation;
        public GameObject DeliveryBag;
        public VisualEffect HappyVisualEffect;
        public string Dialogue;
    }

    [SerializeField]
    private LocalizedStringTable _correctOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _incorrectOrderDialoguesTable;

    [SerializeField]
    private float _delayBeforeText = 2f;

    [Inject]
    private readonly ICustomerPopUpDialogue _customerPopUpDialogue;

    [Inject]
    private readonly DeliverySequence _deliverySequence;

    public Sequence DeliveryDialogue(
        Order order,
        Delivery delivery,
        CustomerAnimationController customerAnimation,
        Vector3 dialogueWorldPosition,
        GameObject deliveryBag,
        VisualEffect happyVisualEffect)
    {
        customerAnimation.ShowDialogueCustomer(order.Customer.Sprite);

        if (deliveryBag != null)
            deliveryBag.SetActive(true);

        var isCorrect = delivery.IsCorrectFor(order);
        var dialogueContext = new DeliveryDialogueContext
        {
            Order = order,
            Customer = order.Customer,
            CustomerAnimation = customerAnimation,
            DeliveryBag = deliveryBag,
            HappyVisualEffect = happyVisualEffect,
            Dialogue = GetRandomDeliveryDialogue(isCorrect),
        };

        _deliverySequence.Deliver(order, delivery);

        return Sequence.Create(Tween.Delay(_delayBeforeText, () =>
            {
                if (isCorrect && !Application.isMobilePlatform && dialogueContext.HappyVisualEffect != null)
                    dialogueContext.HappyVisualEffect.Play();
            }))
            .OnComplete(this, dialogueService => dialogueService.PlayDialogue(dialogueContext));
    }

    private string GetRandomDeliveryDialogue(bool isCorrectDelivery)
    {
        var tableReference = isCorrectDelivery ? _correctOrderDialoguesTable : _incorrectOrderDialoguesTable;
        var fieldName = isCorrectDelivery ? nameof(_correctOrderDialoguesTable) : nameof(_incorrectOrderDialoguesTable);
        return CustomerDialogueLocalization.GetRandomLocalizedDialogue(tableReference, nameof(CustomerDeliveryDialogue), fieldName);
    }

    private Sequence PlayDialogue(DeliveryDialogueContext context)
    {
        return _customerPopUpDialogue.ShowDialogue(context.Customer, context.Dialogue)
            .ChainCallback(() =>
            {
                context.CustomerAnimation.CompleteDialogue();

                if (context.DeliveryBag != null)
                    context.DeliveryBag.SetActive(false);

                if (context.HappyVisualEffect != null)
                    context.HappyVisualEffect.Stop();

                _deliverySequence.FinishOrderFlow(context.Order);
            });
    }
}
