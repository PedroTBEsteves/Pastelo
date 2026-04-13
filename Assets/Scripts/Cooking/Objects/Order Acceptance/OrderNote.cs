using KBCore.Refs;
using PrimeTween;
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
    private Image _doughIcon;

    [SerializeField]
    private Transform _ingredientsRoot;

    [SerializeField]
    private OrderNoteTooltipIngredientRow _ingredientRowPrefab;

    [SerializeField]
    private AudioClip _failAudio;
    
    [SerializeField]
    private Slider _remainingTimeSlider;

    [SerializeField]
    private Graphic _remainingTimeFillGraphic;

    [SerializeField]
    private Color _highRemainingTimeColor = Color.green;

    [SerializeField]
    private Color _mediumRemainingTimeColor = Color.yellow;

    [SerializeField]
    private Color _lowRemainingTimeColor = Color.red;

    [SerializeField]
    private TweenSettings _remainingTimeColorTweenSettings;
    
    [SerializeField, Self]
    private Draggable _draggable;

    private Vector3 _positionOnHold;
    private TutorialTarget _tutorialTarget;
    private Tween _remainingTimeColorTween;
    private RemainingTimeBand _currentRemainingTimeBand;
    private bool _remainingTimeBandInitialized;
    private LayoutElement _layoutElement;
    private RectTransform _rectTransform;
    private float _baseWidth;
    
    public Order Order { get; private set; }

    private enum RemainingTimeBand
    {
        High,
        Medium,
        Low
    }

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _layoutElement = GetComponent<LayoutElement>();
        _baseWidth = Mathf.Max(_rectTransform.sizeDelta.x, _layoutElement != null ? _layoutElement.preferredWidth : 0f);
        _orderController.OrderExpired += OnOrderExpired;
        _draggable.Held += OnHeld;
        _draggable.Dropped += OnDropped;
        _draggable.AddCanDragHandler(CanDragOrderNote);
    }

    private void OnDestroy()
    {
        if (_remainingTimeColorTween.isAlive)
            _remainingTimeColorTween.Stop();

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
        BindRecipe(order.Recipe);
        ApplyRemainingTimeBand(order.NormalizedRemainingTime, false);
        _remainingTimeSlider.transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        UpdateWidthFromContent();
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
        var normalizedRemainingTime = Order.NormalizedRemainingTime;
        _remainingTimeSlider.normalizedValue = normalizedRemainingTime;
        ApplyRemainingTimeBand(normalizedRemainingTime, true);
    }

    private void OnHeld(PointerEventData _)
    {
        _positionOnHold = transform.position;
    }

    private void OnDropped(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPoint(eventData.position);
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

    private void BindRecipe(Recipe recipe)
    {
        if (recipe == null)
            return;

        if (_doughIcon != null)
        {
            _doughIcon.sprite = recipe.Dough.Icon;
            _doughIcon.preserveAspect = true;
        }

        if (_ingredientsRoot == null || _ingredientRowPrefab == null)
            return;
        
        foreach (var (filling, amount) in recipe.Fillings)
        {
            var ingredientRow = Instantiate(_ingredientRowPrefab, _ingredientsRoot);
            ingredientRow.Bind(filling, amount);
        }
    }

    private void UpdateWidthFromContent()
    {
        if (_rectTransform == null)
            return;

        var width = _baseWidth;
        RectTransform contentRect = null;
        if (_ingredientsRoot != null)
            contentRect = _ingredientsRoot.parent as RectTransform;
        else if (_doughIcon != null)
            contentRect = _doughIcon.transform.parent?.parent as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            var contentWidth = LayoutUtility.GetPreferredWidth(contentRect);
            width = Mathf.Max(width, contentRect.anchoredPosition.x + contentWidth + 16f);
        }

        var sizeDelta = _rectTransform.sizeDelta;
        sizeDelta.x = width;
        _rectTransform.sizeDelta = sizeDelta;

        if (_layoutElement != null)
            _layoutElement.preferredWidth = width;
    }

    private void ApplyRemainingTimeBand(float normalizedRemainingTime, bool useTween)
    {
        var remainingTimeBand = GetRemainingTimeBand(normalizedRemainingTime);
        if (_remainingTimeBandInitialized
            && remainingTimeBand == _currentRemainingTimeBand
            && _remainingTimeFillGraphic != null)
            return;

        _remainingTimeBandInitialized = true;
        _currentRemainingTimeBand = remainingTimeBand;
        if (_remainingTimeFillGraphic == null)
            return;

        var targetColor = GetRemainingTimeColor(remainingTimeBand);
        if (!useTween)
        {
            _remainingTimeFillGraphic.color = targetColor;
            return;
        }

        if (_remainingTimeColorTween.isAlive)
            _remainingTimeColorTween.Stop();

        _remainingTimeColorTween = Tween.Color(
            _remainingTimeFillGraphic,
            _remainingTimeFillGraphic.color,
            targetColor,
            _remainingTimeColorTweenSettings);
    }

    private RemainingTimeBand GetRemainingTimeBand(float normalizedRemainingTime)
    {
        if (normalizedRemainingTime > 0.66f)
            return RemainingTimeBand.High;

        if (normalizedRemainingTime > 0.33f)
            return RemainingTimeBand.Medium;

        return RemainingTimeBand.Low;
    }

    private Color GetRemainingTimeColor(RemainingTimeBand remainingTimeBand)
    {
        return remainingTimeBand switch
        {
            RemainingTimeBand.High => _highRemainingTimeColor,
            RemainingTimeBand.Medium => _mediumRemainingTimeColor,
            RemainingTimeBand.Low => _lowRemainingTimeColor,
            _ => _highRemainingTimeColor
        };
    }
}
