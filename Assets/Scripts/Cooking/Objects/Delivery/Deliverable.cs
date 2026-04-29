using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deliverable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] 
    private Transform _discardPositionTransform;

    [SerializeField]
    private DraggableClosedPastel _closedPastelPrefab;

    [SerializeField]
    private DeliveryCustomerDisplay _customerDisplay;

    [SerializeField]
    private DeliveryIngredientHint _bagIngredientHint;

    [SerializeField]
    private Sprite _filledSprite;
    
    [SerializeField]
    private AudioSource _addedAudioSource;

    [Inject]
    private readonly DeliverySequence _deliverySequence;

    [Inject]
    private readonly OrderController _orderController;

    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private ClosedPastelDough _closedPastelDough;
    private SpriteRenderer _spriteRenderer;
    private Sprite _emptySprite;
    private TutorialTarget _tutorialTarget;
    private Vector3 _dragStartPosition;
    private Vector3 _holdOffset;
    private int _baseSortingOrder;
    private bool _isDraggingBag;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _emptySprite = _spriteRenderer != null ? _spriteRenderer.sprite : null;
        _baseSortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder : 0;
        _tutorialTarget = GetComponent<TutorialTarget>();
        _bagIngredientHint.SetVisible(false);
        if (_tutorialTarget != null)
        {
            _tutorialTarget.Configure(TutorialTargetId.DeliveryArea);
            _tutorialTargetRegistry.Register(_tutorialTarget);
        }
        else
        {
            Debug.LogError($"{nameof(Deliverable)} on '{name}' is missing a scene-prepared {nameof(TutorialTarget)}.", this);
        }

        ConfigureCustomerDisplay();
        UpdateSprite();
    }

    private void OnDestroy()
    {
        if (_tutorialTarget != null)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }
    
    public bool TryAddPastel(DraggableClosedPastel closedPastel)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.PlaceOnDelivery))
            return false;

        if (_closedPastelDough != null)
            return false;

        _closedPastelDough = closedPastel.GetClosedPastelDough();
        _addedAudioSource.Play();
        _tutorialEvents.PublishPastelPlacedOnDelivery(closedPastel);
        _bagIngredientHint?.Bind(_closedPastelDough.Recipe);
        _bagIngredientHint?.SetVisible(false);
        UpdateSprite();
        return true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_closedPastelDough == null || _filledSprite == null || _customerDisplay == null)
            return;

        if (!_customerDisplay.HasVisibleCustomer())
            return;

        _isDraggingBag = true;
        _dragStartPosition = transform.position;
        _holdOffset = transform.position - GetPointerWorldPosition(eventData);
        _bagIngredientHint?.SetVisible(true);
        _customerDisplay.SetHintsVisible(true);
        _tutorialEvents.PublishDeliveryBagPickedUp(this);

        if (_spriteRenderer != null)
            _spriteRenderer.sortingOrder = 9;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggingBag)
            return;

        transform.position = GetPointerWorldPosition(eventData) + _holdOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDraggingBag)
            return;

        _isDraggingBag = false;
        _bagIngredientHint?.SetVisible(false);
        _customerDisplay.SetHintsVisible(false);

        var delivered = TryDeliverToCustomer(eventData);
        if (delivered)
        {
            _closedPastelDough = null;
            _bagIngredientHint?.Bind(null);
            UpdateSprite();
        }

        transform.position = _dragStartPosition;
        _tutorialEvents.PublishDeliveryBagDropped(this);

        if (_spriteRenderer != null)
            _spriteRenderer.sortingOrder = _baseSortingOrder;
    }

    public bool TryRestorePastel(ClosedPastelDough closedPastelDough)
    {
        if (closedPastelDough == null || _closedPastelDough != null)
            return false;

        _closedPastelDough = closedPastelDough;
        _bagIngredientHint?.Bind(_closedPastelDough.Recipe);
        _bagIngredientHint?.SetVisible(false);
        UpdateSprite();
        return true;
    }

    private void UpdateSprite()
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.sprite = _closedPastelDough == null ? _emptySprite : _filledSprite;
    }

    private void ConfigureCustomerDisplay()
    {
        if (_customerDisplay == null)
        {
            Debug.LogError($"{nameof(Deliverable)} on '{name}' is missing a {nameof(DeliveryCustomerDisplay)} reference.", this);
            return;
        }

        if (_bagIngredientHint == null)
            Debug.LogError($"{nameof(Deliverable)} on '{name}' is missing a scene-prepared {nameof(DeliveryIngredientHint)} for the bag.", this);

        _customerDisplay.Configure(
            _orderController,
            _deliverySequence,
            _tutorialEvents,
            _interactionGate,
            _tutorialTargetRegistry);
    }

    private bool TryDeliverToCustomer(PointerEventData eventData)
    {
        var mousePosition = GetPointerWorldPosition(eventData);
        var raycastHits = Physics2D.RaycastAll(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));

        foreach (var raycastHit in raycastHits)
        {
            if (raycastHit.collider.TryGetComponent<DeliveryCustomerSlot>(out var slot))
                return slot.TryDeliver(_closedPastelDough);
        }

        return false;
    }

    private Vector3 GetPointerWorldPosition(PointerEventData eventData)
    {
        return _cameraController.ScreenToWorldPoint(eventData.position);
    }
}
