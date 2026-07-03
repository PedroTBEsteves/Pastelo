using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

public class CustomerDisplaySlot : MonoBehaviour, IPointerClickHandler
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

    [SerializeField]
    private SpriteRenderer _waitingServiceIcon;

    private bool _isResolving;
    private GameplayTutorialEvents _tutorialEvents;
    private GameplayInteractionGate _interactionGate;
    private TutorialTargetRegistry _tutorialTargetRegistry;
    private ICustomerDeliveryDialogue _customerDeliveryDialogue;
    private CustomerDisplay _customerDisplay;
    private CustomerWaitStatus _waitStatus;
    private bool _canReceiveDelivery;
    private int _waitingServiceIconVersion;

    public Order Order { get; private set; }
    public bool IsAvailable => Order == null && _waitStatus == null;
    public bool HasCustomer => Order != null || _waitStatus != null;

    public void Configure(
        CustomerDisplay customerDisplay,
        GameplayTutorialEvents tutorialEvents,
        GameplayInteractionGate interactionGate,
        TutorialTargetRegistry tutorialTargetRegistry,
        Vector3 startLocalOffset,
        ICustomerDeliveryDialogue customerDeliveryDialogue,
        int sortingOrder)
    {
        _customerDisplay = customerDisplay;
        _tutorialEvents = tutorialEvents;
        _interactionGate = interactionGate;
        _tutorialTargetRegistry = tutorialTargetRegistry;
        _customerDeliveryDialogue = customerDeliveryDialogue;

        if (!TryResolveSceneReferences())
            return;

        _collider.enabled = false;
        _hint.SetVisible(false);
        SetWaitingServiceIconVisible(false);
    }

    private void OnDestroy()
    {
        if (_tutorialTarget != null)
            _tutorialTargetRegistry?.Unregister(_tutorialTarget);
    }

    public void ShowWaitingCustomer(CustomerWaitStatus waitStatus, TweenSettings moveTweenSettings)
    {
        _waitStatus = waitStatus;
        Order = null;
        _isResolving = false;
        _canReceiveDelivery = false;
        SetWaitingServiceIconVisible(false);
        var iconVersion = ++_waitingServiceIconVersion;
        _customerAnimation.ShowDeliveryCustomer(
            waitStatus.Customer.Sprite,
            () => ShowWaitingServiceIconIfCurrent(iconVersion));
        _collider.enabled = true;
        _hint.Bind(null);
        _hint.SetVisible(false);
        _tutorialTarget.Configure(TutorialTargetId.DeliveryCustomer);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    public void StartOrder(Order order)
    {
        Order = order;
        _waitStatus = null;
        _canReceiveDelivery = false;
        _waitingServiceIconVersion++;
        SetWaitingServiceIconVisible(false);
        _collider.enabled = false;
        _hint.Bind(order.Recipe);
        _hint.SetVisible(false);
        _tutorialTarget.Configure(TutorialTargetId.DeliveryCustomer, order);
    }

    public void EnableDelivery()
    {
        if (Order == null)
            return;

        _canReceiveDelivery = true;
        _collider.enabled = true;
    }

    public void DisableDelivery()
    {
        _canReceiveDelivery = false;
        _collider.enabled = false;
        SetHintsVisible(false);
    }

    public void Hide(TweenSettings moveTweenSettings)
    {
        if (HasCustomer)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);

        Order = null;
        _waitStatus = null;
        _isResolving = false;
        _canReceiveDelivery = false;
        _waitingServiceIconVersion++;
        _collider.enabled = false;
        _hint.SetVisible(false);
        SetWaitingServiceIconVisible(false);
        _customerAnimation.HideDeliveryCustomer();
    }

    public bool CanReceiveDelivery()
    {
        return Order != null
            && _canReceiveDelivery
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_waitStatus == null || _customerDisplay == null)
            return;

        _customerDisplay.TakeOrder(this, _waitStatus);
    }

    public void HideWaitingServiceIcon()
    {
        _waitingServiceIconVersion++;
        SetWaitingServiceIconVisible(false);
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
            Debug.LogError($"{nameof(CustomerDisplaySlot)} on '{name}' is missing a scene-prepared {nameof(BoxCollider2D)}.", this);
        }

        if (_hint == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(CustomerDisplaySlot)} on '{name}' is missing a scene-prepared {nameof(DeliveryIngredientHint)} child.", this);
        }

        if (_tutorialTarget == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(CustomerDisplaySlot)} on '{name}' is missing a scene-prepared {nameof(TutorialTarget)}.", this);
        }

        if (_customerAnimation == null)
        {
            hasReferences = false;
            Debug.LogError($"{nameof(CustomerDisplaySlot)} on '{name}' is missing a scene-prepared {nameof(CustomerAnimationController)}.", this);
        }

        return hasReferences;
    }

    private void ShowWaitingServiceIconIfCurrent(int iconVersion)
    {
        if (iconVersion != _waitingServiceIconVersion || _waitStatus == null)
            return;

        SetWaitingServiceIconVisible(true);
    }

    private void SetWaitingServiceIconVisible(bool visible)
    {
        if (_waitingServiceIcon != null)
            _waitingServiceIcon.enabled = visible;
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
