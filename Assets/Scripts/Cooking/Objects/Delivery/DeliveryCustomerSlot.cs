using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

public class DeliveryCustomerSlot : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D _collider;

    [SerializeField]
    private DeliveryIngredientHint _hint;

    [SerializeField]
    private TutorialTarget _tutorialTarget;

    [SerializeField]
    private CustomerAnimationController _customerAnimation;

    [SerializeField]
    private Transform _dialoguePosition;

    [SerializeField]
    private GameObject _deliveryBag;

    [SerializeField]
    private VisualEffect _happyVisualEffect;

    private bool _isResolving;
    private GameplayTutorialEvents _tutorialEvents;
    private GameplayInteractionGate _interactionGate;
    private TutorialTargetRegistry _tutorialTargetRegistry;
    private ICustomerDeliveryDialogue _customerDeliveryDialogue;

    public Order Order { get; private set; }
    public bool IsAvailable => Order == null;

    public void Configure(
        GameplayTutorialEvents tutorialEvents,
        GameplayInteractionGate interactionGate,
        TutorialTargetRegistry tutorialTargetRegistry,
        Vector3 startLocalOffset,
        ICustomerDeliveryDialogue customerDeliveryDialogue,
        int sortingOrder)
    {
        _tutorialEvents = tutorialEvents;
        _interactionGate = interactionGate;
        _tutorialTargetRegistry = tutorialTargetRegistry;
        _customerDeliveryDialogue = customerDeliveryDialogue;

        if (!TryResolveSceneReferences())
            return;

        _collider.enabled = false;
        _hint.SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_tutorialTarget != null)
            _tutorialTargetRegistry?.Unregister(_tutorialTarget);
    }

    public void Show(Order order, TweenSettings moveTweenSettings)
    {
        Order = order;
        _isResolving = false;
        _customerAnimation.ShowDeliveryCustomer(order.Customer.Sprite);
        _collider.enabled = true;
        _hint.Bind(order.Recipe);
        _tutorialTarget.Configure(TutorialTargetId.DeliveryCustomer, order);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    public void Hide(TweenSettings moveTweenSettings)
    {
        if (Order != null)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);

        Order = null;
        _isResolving = false;
        _collider.enabled = false;
        _hint.SetVisible(false);
        _customerAnimation.HideDeliveryCustomer();
    }

    public bool CanReceiveDelivery()
    {
        return Order != null
            && !_isResolving
            && _interactionGate.CanInteract(TutorialInteractionType.DeliverOrder, Order);
    }

    public bool TryDeliver(ClosedPastelDough closedPastelDough)
    {
        if (closedPastelDough == null || !CanReceiveDelivery() || _customerDeliveryDialogue == null)
            return false;

        _isResolving = true;
        _collider.enabled = false;
        SetHintsVisible(false);

        var delivery = new Delivery(closedPastelDough.Finish());
        var deliveredOrder = Order;
        _tutorialEvents.PublishOrderDelivered(deliveredOrder);
        _customerDeliveryDialogue.DeliveryDialogue(
            deliveredOrder,
            delivery,
            _customerAnimation,
            GetDialogueWorldPosition(),
            _deliveryBag,
            _happyVisualEffect);
        return true;
    }

    public void SetHintsVisible(bool visible)
    {
        if (Order != null)
            _hint.SetVisible(visible);
    }

    private bool TryResolveSceneReferences()
    {
        if (_collider == null)
            _collider = GetComponent<BoxCollider2D>();

        if (_hint == null)
            _hint = GetComponentInChildren<DeliveryIngredientHint>(true);

        if (_tutorialTarget == null)
            _tutorialTarget = GetComponent<TutorialTarget>();

        if (_customerAnimation == null)
            _customerAnimation = GetComponentInChildren<CustomerAnimationController>(true);

        var hasReferences = true;
        if (_collider == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(DeliveryCustomerSlot)} on '{name}' is missing a scene-prepared {nameof(BoxCollider2D)}.", this);
        }

        if (_hint == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(DeliveryCustomerSlot)} on '{name}' is missing a scene-prepared {nameof(DeliveryIngredientHint)} child.", this);
        }

        if (_tutorialTarget == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(DeliveryCustomerSlot)} on '{name}' is missing a scene-prepared {nameof(TutorialTarget)}.", this);
        }

        if (_customerAnimation == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(DeliveryCustomerSlot)} on '{name}' is missing a scene-prepared {nameof(CustomerAnimationController)}.", this);
        }

        return hasReferences;
    }

    private Vector3 GetDialogueWorldPosition()
    {
        if (_dialoguePosition != null)
            return _dialoguePosition.position;

        if (_customerAnimation != null)
            return _customerAnimation.transform.position;

        return transform.position;
    }
}
