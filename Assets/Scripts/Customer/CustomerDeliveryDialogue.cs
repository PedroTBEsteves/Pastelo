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
        public CustomerAnimationController CustomerAnimation;
        public GameObject DeliveryBag;
        public VisualEffect HappyVisualEffect;
        public string Dialogue;
        public Vector3 DialogueWorldPosition;
    }

    [SerializeField]
    private LocalizedStringTable _correctOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _incorrectOrderDialoguesTable;

    [SerializeField]
    private float _delayBeforeText = 2f;

    [SerializeField]
    private float _delayAfterTextIsDone = 2f;

    [Inject]
    private readonly DialoguePresentationService _dialoguePresentation;

    [Inject]
    private readonly DeliverySequence _deliverySequence;

    [Inject]
    private readonly StrikesController _strikesController;

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
            CustomerAnimation = customerAnimation,
            DeliveryBag = deliveryBag,
            HappyVisualEffect = happyVisualEffect,
            Dialogue = GetRandomDeliveryDialogue(isCorrect),
            DialogueWorldPosition = dialogueWorldPosition,
        };

        _deliverySequence.Deliver(order, delivery);

        return Sequence.Create(Tween.Delay(_delayBeforeText, () =>
            {
                if (isCorrect && !Application.isMobilePlatform && dialogueContext.HappyVisualEffect != null)
                    dialogueContext.HappyVisualEffect.Play();
            }))
            .OnComplete(this, dialogueService =>
            {
                dialogueService.PlayDialogue(dialogueContext).ChainCallback(() =>
                {
                    if (!isCorrect) 
                        _strikesController.Strike();
                });
            });
    }

    private string GetRandomDeliveryDialogue(bool isCorrectDelivery)
    {
        var tableReference = isCorrectDelivery ? _correctOrderDialoguesTable : _incorrectOrderDialoguesTable;
        var fieldName = isCorrectDelivery ? nameof(_correctOrderDialoguesTable) : nameof(_incorrectOrderDialoguesTable);
        return CustomerDialogueLocalization.GetRandomLocalizedDialogue(tableReference, nameof(CustomerDeliveryDialogue), fieldName);
    }

    private Sequence PlayDialogue(DeliveryDialogueContext context)
    {
        return _dialoguePresentation.Show(context.Dialogue, context.DialogueWorldPosition)
            .Chain(Tween.Delay(_delayAfterTextIsDone, () =>
            {
                context.CustomerAnimation.CompleteDialogue();

                if (context.DeliveryBag != null)
                    context.DeliveryBag.SetActive(false);

                if (context.HappyVisualEffect != null)
                    context.HappyVisualEffect.Stop();

                _deliverySequence.FinishOrderFlow(context.Order);
            }));
    }
}
