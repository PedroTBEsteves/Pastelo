using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Draggable))]
[RequireComponent(typeof(DisposableDraggable))]
public class Deliverable : ValidatedMonoBehaviour, IDiscardPolicy, IDiscardHandler
{
    [SerializeField] 
    private Transform _discardPositionTransform;

    [SerializeField]
    private DraggableClosedPastel _closedPastelPrefab;

    [SerializeField]
    private CustomerDisplay _customerDisplay;

    [SerializeField]
    private DeliveryIngredientHint _bagIngredientHint;

    [SerializeField]
    private Sprite _filledSprite;
    
    [SerializeField]
    private AudioSource _addedAudioSource;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private SpriteRenderer _spriteRenderer;

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

    private Draggable _draggable;
    private ClosedPastelDough _closedPastelDough;
    private Sprite _emptySprite;
    private Vector3 _dragStartPosition;
    private bool _isDraggingBag;
    
    private void Awake()
    {
        _draggable = GetComponent<Draggable>();
        _emptySprite = _spriteRenderer.sprite;
        _bagIngredientHint.SetVisible(false);
        _draggable.Held += OnHeld;
        _draggable.Dropped += OnDropped;
        _draggable.AddCanDragHandler(CanDragBag);

        ConfigureCustomerDisplay();
        UpdateSprite();
    }

    private void OnDestroy()
    {
        if (_draggable != null)
        {
            _draggable.Held -= OnHeld;
            _draggable.Dropped -= OnDropped;
            _draggable.RemoveCanDragHandler(CanDragBag);
        }
    }
    
    public bool TryAddPastel(DraggableClosedPastel closedPastel)
    {
        if (_closedPastelDough != null)
            return false;

        _closedPastelDough = closedPastel.GetClosedPastelDough();
        _addedAudioSource.Play();
        _bagIngredientHint?.Bind(_closedPastelDough.Recipe);
        _bagIngredientHint?.SetVisible(false);
        UpdateSprite();
        return true;
    }

    public bool CanBeDiscarded() => _closedPastelDough != null;

    public void Discard()
    {
        if (_closedPastelDough == null)
            return;

        ClearPastel();
        transform.position = _dragStartPosition;
    }

    private bool CanDragBag()
    {
        if (_closedPastelDough == null || _filledSprite == null)
            return false;

        return CanBeDiscarded() || (_customerDisplay != null && _customerDisplay.HasVisibleCustomer());
    }

    private void OnHeld(PointerEventData _)
    {
        _isDraggingBag = true;
        _dragStartPosition = transform.position;
        _bagIngredientHint?.SetVisible(true);
        _customerDisplay.SetHintsVisible(true);
    }

    private void OnDropped(PointerEventData eventData)
    {
        if (!_isDraggingBag)
            return;

        _isDraggingBag = false;
        _bagIngredientHint?.SetVisible(false);
        _customerDisplay.SetHintsVisible(false);

        var delivered = TryDeliverToCustomer(eventData);
        if (delivered)
            ClearPastel();

        transform.position = _dragStartPosition;
    }

    private void UpdateSprite()
    {
        _spriteRenderer.sprite = _closedPastelDough == null ? _emptySprite : _filledSprite;
    }

    private void ConfigureCustomerDisplay()
    {
        if (_customerDisplay == null)
        {
            Debug.LogError($"{nameof(Deliverable)} on '{name}' is missing a {nameof(CustomerDisplay)} reference.", this);
            return;
        }

        if (_bagIngredientHint == null)
            Debug.LogError($"{nameof(Deliverable)} on '{name}' is missing a scene-prepared {nameof(DeliveryIngredientHint)} for the bag.", this);

        // CustomerDisplay now configures itself through scene DI.
    }

    private void ClearPastel()
    {
        _closedPastelDough = null;
        _bagIngredientHint?.Bind(null);
        _bagIngredientHint?.SetVisible(false);
        UpdateSprite();
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
            if (raycastHit.collider.TryGetComponent<CustomerDisplaySlot>(out var slot))
                return slot.TryDeliver(_closedPastelDough);
        }

        return false;
    }

    private Vector3 GetPointerWorldPosition(PointerEventData eventData)
    {
        return _cameraController.ScreenToWorldPoint(eventData.position);
    }
}
