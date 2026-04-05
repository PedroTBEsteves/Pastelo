using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Draggable))]
public class OrderNote : ValidatedMonoBehaviour
{
    [Inject]
    private readonly OrderController _orderController;

    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    [SerializeField]
    private TextMeshProUGUI _orderNumber;

    [SerializeField]
    private AudioClip _failAudio;
    
    [SerializeField]
    private Slider _remainingTimeSlider;
    
    [SerializeField, Self]
    private Draggable _draggable;

    private Vector3 _positionOnHold;
    private TutorialTarget _tutorialTarget;
    
    public Order Order { get; private set; }

    private void Awake()
    {
        _orderController.OrderExpired += OnOrderExpired;
        _draggable.Held += OnHeld;
        _draggable.Dropped += OnDropped;
        _draggable.AddCanDragHandler(CanDragOrderNote);
    }

    private void OnDestroy()
    {
        _orderController.OrderExpired -= OnOrderExpired;
        _draggable.Held -= OnHeld;
        _draggable.Dropped -= OnDropped;
        _draggable.RemoveCanDragHandler(CanDragOrderNote);
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    public void Initialize(Order order)
    {
        Order = order;
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.OrderNote, order);
        _tutorialTargetRegistry.Register(_tutorialTarget);

        _orderNumber.SetText($"#{order.Number}");
        _remainingTimeSlider.transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void PostInitialize()
    {
        Destroy(GetComponent<ContentSizeFitter>());
    }

    private void OnOrderExpired(Order order)
    {
        if (order != Order)
            return;
        
        Destroy(gameObject);
        AudioPlayer.Instance.Play(_failAudio);
    }

    private void Update()
    {
        _remainingTimeSlider.normalizedValue = Order.NormalizedRemainingTime;
    }

    private void OnHeld(PointerEventData _)
    {
        _positionOnHold = transform.position;
    }

    private void OnDropped(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPointy(eventData.position);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));
        
        if (raycastHit
            && raycastHit.collider.TryGetComponent<Deliverable>(out var deliverable))
        {
            if (deliverable.TryDeliver(this))
            {
                Destroy(gameObject);
                return;
            }
        }
        
        transform.position = _positionOnHold;
    }

    private bool CanDragOrderNote() => _interactionGate.CanInteract(TutorialInteractionType.DeliverOrder, Order);
}
