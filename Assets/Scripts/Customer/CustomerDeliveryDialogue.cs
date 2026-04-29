using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.VFX;

public class CustomerDeliveryDialogue : MonoBehaviour, ICustomerDeliveryDialogue
{
    [SerializeField]
    private CustomerAnimationController _customerAnimation;

    [SerializeField]
    private Transform _dialoguePosition;

    [SerializeField]
    private LocalizedStringTable _correctOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _incorrectOrderDialoguesTable;

    [SerializeField]
    private GameObject _deliveryBag;

    [SerializeField]
    private float _delayBeforeText = 2f;

    [SerializeField]
    private float _delayAfterTextIsDone = 2f;

    [SerializeField]
    private VisualEffect _happyVisualEffect;

    [Inject]
    private readonly DialoguePresentationService _dialoguePresentation;

    private Sequence _dialogueSequence;

    public bool IsPlaying => _dialogueSequence.isAlive || (_customerAnimation != null && _customerAnimation.IsDialoguePlaying);

    public Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController)
    {
        _customerAnimation.ShowDialogueCustomer(order.Customer.Sprite);
        _deliveryBag.SetActive(true);

        var isCorrect = delivery.IsCorrectFor(order);
        var dialogue = GetRandomDeliveryDialogue(isCorrect);

        _dialogueSequence = Sequence.Create(Tween.Delay(_delayBeforeText, () =>
            {
                orderController.DeliverOrder(order, delivery);

                if (isCorrect && !Application.isMobilePlatform)
                    _happyVisualEffect.Play();
            }))
            .Chain(_dialoguePresentation.Show(dialogue, GetDialogueWorldPosition()))
            .Chain(Tween.Delay(_delayAfterTextIsDone, () =>
            {
                _customerAnimation.ShowNextCustomerAfterDialogue();
                _deliveryBag.SetActive(false);
                _happyVisualEffect.Stop();
            }));

        return _dialogueSequence;
    }

    private Vector3 GetDialogueWorldPosition() => _dialoguePosition == null ? transform.position : _dialoguePosition.position;

    private string GetRandomDeliveryDialogue(bool isCorrectDelivery)
    {
        var tableReference = isCorrectDelivery ? _correctOrderDialoguesTable : _incorrectOrderDialoguesTable;
        var fieldName = isCorrectDelivery ? nameof(_correctOrderDialoguesTable) : nameof(_incorrectOrderDialoguesTable);
        return CustomerDialogueLocalization.GetRandomLocalizedDialogue(tableReference, nameof(CustomerDeliveryDialogue), fieldName);
    }
}
