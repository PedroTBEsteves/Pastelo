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
    private Sprite _filledSprite;
    
    [SerializeField]
    private AudioSource _addedAudioSource;

    [Inject]
    private readonly DeliverySequence _deliverySequence;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private ClosedPastelDough _closedPastelDough;
    private DraggableClosedPastel _draggedPastel;
    private SpriteRenderer _spriteRenderer;
    private Sprite _emptySprite;
    private TutorialTarget _tutorialTarget;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _emptySprite = _spriteRenderer != null ? _spriteRenderer.sprite : null;
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.DeliveryArea);
        _tutorialTargetRegistry.Register(_tutorialTarget);
        UpdateSprite();
    }

    private void OnDestroy()
    {
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
        UpdateSprite();
        return true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_closedPastelDough == null || _closedPastelPrefab == null)
            return;

        if (!_interactionGate.CanInteract(TutorialInteractionType.RemoveCookedPastel))
            return;

        var storedClosedPastelDough = _closedPastelDough;
        _closedPastelDough = null;
        UpdateSprite();

        var position = eventData.pointerCurrentRaycast.worldPosition;
        _draggedPastel = Instantiate(_closedPastelPrefab, position, Quaternion.identity, transform.parent);
        _draggedPastel.Initialize(storedClosedPastelDough);

        ExecuteEvents.Execute(_draggedPastel.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedPastel == null)
            return;

        ExecuteEvents.Execute(_draggedPastel.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggedPastel == null)
            return;

        ExecuteEvents.Execute(_draggedPastel.gameObject, eventData, ExecuteEvents.endDragHandler);
        _draggedPastel = null;
    }

    public bool TryDeliver(OrderNote orderNote)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.DeliverOrder, orderNote.Order))
            return false;

        if (_closedPastelDough == null)
            return false;

        var delivery = new Delivery(_closedPastelDough.Finish());
        _addedAudioSource.Play();
        _deliverySequence.StartSequence(orderNote.Order, delivery);
        _tutorialEvents.PublishOrderDelivered(orderNote.Order);

        _closedPastelDough = null;
        UpdateSprite();
        
        return true;
    }

    private void UpdateSprite()
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.sprite = _closedPastelDough == null ? _emptySprite : _filledSprite;
    }
}
