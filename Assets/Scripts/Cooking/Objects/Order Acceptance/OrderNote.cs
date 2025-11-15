using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderNote : Draggable
{
    [Inject]
    private readonly OrderController _orderController;

    [Inject]
    private readonly CameraController _cameraController;

    [SerializeField]
    private TextMeshProUGUI _orderNumber;
    
    [SerializeField]
    private OrderNoteFillingGroup _fillingGroupPrefab;
    
    [SerializeField]
    private Image _doughIcon;

    [SerializeField]
    private Transform _fillingsRoot;

    [SerializeField]
    private AudioClip _failAudio;
    
    [SerializeField]
    private Slider _remainingTimeSlider;

    private Vector3 _positionOnHold;
    
    public Order Order { get; private set; }

    private void Awake()
    {
        _orderController.OrderExpired += OnOrderExpired;
    }

    private void OnDestroy()
    {
        _orderController.OrderExpired -= OnOrderExpired;
    }

    public void Initialize(Order order)
    {
        Order = order;

        _orderNumber.SetText($"#{order.Number}");
        
        _doughIcon.sprite = order.Recipe.Dough.Icon;

        var fillingGroup = Instantiate(_fillingGroupPrefab, _fillingsRoot); 
        
        foreach (var (filling, amount) in order.Recipe.Fillings)
        {
            if (!fillingGroup.TryAdd(filling, amount))
            {
                fillingGroup = Instantiate(_fillingGroupPrefab, _fillingsRoot);
                fillingGroup.TryAdd(filling, amount);
            }
            else
            {
                fillingGroup.TryAdd(filling, amount);
            }
        }
        
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

    protected override void Update()
    {
        base.Update();

        _remainingTimeSlider.normalizedValue = Order.NormalizedRemainingTime;
    }

    protected override void OnHold(PointerEventData eventData)
    {
        _positionOnHold = transform.position;
    }

    protected override void OnDrop(PointerEventData eventData)
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
}
