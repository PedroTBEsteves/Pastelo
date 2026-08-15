using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class CustomerAnimationController : MonoBehaviour
{
    private enum CustomerAnimationMode
    {
        Queue,
        DeliverySlot,
    }

    [SerializeField]
    private CustomerAnimationMode _mode = CustomerAnimationMode.Queue;

    [SerializeField]
    private SpriteRenderer _customerSprite;

    [SerializeField]
    private SpriteRenderer _customerTransitionSprite;

    [SerializeField]
    private SpriteRenderer _iconSprite;

    [SerializeField]
    private SpriteRenderer _queuedCustomersIndicatorSprite;

    [SerializeField]
    private Vector3 _customerStartLocalOffset;

    [SerializeField]
    private TweenSettings _customerMoveTweenSettings;

    [SerializeField]
    private float _customerBobbingRange = 0.1f;

    [SerializeField]
    private TweenSettings _customerBobbingTweenSettings = new(1f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);

    [Inject]
    private readonly CustomerQueue _customerQueue;

    private Tween _customerMoveTween;
    private Tween _customerTransitionMoveTween;
    private Tween _customerBobbingTween;
    private Vector3 _customerIdleLocalPosition;
    private HoverTransformTween _customerHoverTween;
    private bool _isQueueCustomerVisible;
    private bool _isDialoguePlaying;

    public bool IsDialoguePlaying => _isDialoguePlaying;
    public bool IsVisible => _customerSprite != null && _customerSprite.sprite != null && _customerSprite.enabled;

    private void Awake()
    {
        _customerIdleLocalPosition = _customerSprite.transform.localPosition;
        _customerSprite.TryGetComponent(out _customerHoverTween);
        ResetTransitionSprite();
        SetRendererState(_customerSprite, _customerSprite.sprite, _customerIdleLocalPosition);
        _isQueueCustomerVisible = _customerSprite.sprite != null;
        SetCustomerHoverEnabled(false);
        StartCustomerBobbing();

        if (UsesQueueMode())
            SetQueuedCustomersIndicator(GetQueuedCustomersCount());
        else
            SetQueuedCustomersIndicator(0);
    }

    private void Start()
    {
        if (!UsesQueueMode())
            return;

        _customerQueue.CustomerArrived += OnCustomerArrived;
        _customerQueue.CustomerExpired += OnCustomerExpired;
        _customerQueue.CustomersCountChanged += OnCustomersCountChanged;
        SetQueuedCustomersIndicator(GetQueuedCustomersCount());
    }

    private void OnDestroy()
    {
        StopCustomerTweens();

        if (!UsesQueueMode() || _customerQueue == null)
            return;

        _customerQueue.CustomerArrived -= OnCustomerArrived;
        _customerQueue.CustomerExpired -= OnCustomerExpired;
        _customerQueue.CustomersCountChanged -= OnCustomersCountChanged;
    }

    public void ShowDialogueCustomer(Sprite sprite)
    {
        StopCustomerTweens();
        ResetTransitionSprite();
        SetRendererState(_customerSprite, sprite, _customerIdleLocalPosition);
        _isQueueCustomerVisible = false;
        _isDialoguePlaying = true;
        SetIconVisible(false);
        StartCustomerBobbing();
    }

    public void ShowNextCustomerAfterDialogue()
    {
        _isDialoguePlaying = false;
        AnimateNextCustomerAfterDialogue();
    }

    public void CompleteDialogue()
    {
        _isDialoguePlaying = false;
    }

    public void ShowDeliveryCustomer(Sprite sprite)
    {
        _isDialoguePlaying = false;
        SetQueuedCustomersIndicator(0);
        SetIconVisible(false);
        AnimateCustomerSwap(_customerSprite.sprite, sprite);
    }

    public void HideDeliveryCustomer()
    {
        _isDialoguePlaying = false;
        SetQueuedCustomersIndicator(0);
        SetIconVisible(false);
        AnimateCustomerExit();
    }

    private void OnCustomerArrived(Customer customer)
    {
        if (_isQueueCustomerVisible || _isDialoguePlaying)
            return;

        AnimateCustomerArrival(customer.Sprite);
    }

    private void OnCustomerExpired(Customer customer)
    {
        if (_isDialoguePlaying && !_isQueueCustomerVisible)
            return;

        if (!TryGetCurrentAndNextQueuedCustomers(out var currentCustomer, out var nextCustomer))
            return;

        if (currentCustomer != customer)
            return;

        if (nextCustomer != null)
        {
            AnimateCustomerSwap(currentCustomer.Sprite, nextCustomer.Sprite);
            return;
        }

        AnimateCustomerExit();
    }

    private void OnCustomersCountChanged(int count) => SetQueuedCustomersIndicator(count);

    private void AnimateNextCustomerAfterDialogue()
    {
        if (_customerQueue.TryPeek(out var nextCustomer))
        {
            AnimateCustomerSwap(_customerSprite.sprite, nextCustomer.Sprite);
            return;
        }

        AnimateCustomerExit();
    }

    private void AnimateCustomerArrival(Sprite sprite)
    {
        _customerHoverTween?.DisableTween();
        StopCustomerTweens();
        ResetTransitionSprite();
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        SetRendererState(_customerSprite, sprite, customerStartLocalPosition);
        _isQueueCustomerVisible = true;
        _customerMoveTween = Tween.LocalPosition(
                _customerSprite.transform,
                customerStartLocalPosition,
                _customerIdleLocalPosition,
                _customerMoveTweenSettings)
            .OnComplete(this, static controller =>
            {
                controller.StartCustomerBobbing();
                controller._customerHoverTween?.EnableTween();
                controller.SetIconVisible(true);
            });
    }

    private void AnimateCustomerExit()
    {
        if (_customerSprite.sprite == null)
        {
            SetCustomerHoverEnabled(false);
            ResetTransitionSprite();
            _isQueueCustomerVisible = false;
            return;
        }
        
        SetIconVisible(false);
        StopCustomerTweens();
        ResetTransitionSprite();
        SetRendererState(_customerSprite, _customerSprite.sprite, _customerIdleLocalPosition);
        _isQueueCustomerVisible = false;
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        _customerMoveTween = Tween.LocalPosition(
                _customerSprite.transform,
                _customerIdleLocalPosition,
                customerStartLocalPosition,
                _customerMoveTweenSettings)
            .OnComplete(this, static controller =>
            {
                controller.SetRendererState(controller._customerSprite, null, controller._customerIdleLocalPosition);
            });
    }

    private void AnimateCustomerSwap(Sprite outgoingSprite, Sprite incomingSprite)
    {
        if (incomingSprite == null)
        {
            AnimateCustomerExit();
            return;
        }

        if (outgoingSprite == null)
        {
            AnimateCustomerArrival(incomingSprite);
            return;
        }

        StopCustomerTweens();
        var customerStartLocalPosition = GetCustomerStartLocalPosition();
        SetRendererState(_customerTransitionSprite, outgoingSprite, _customerIdleLocalPosition);
        SetRendererState(_customerSprite, incomingSprite, customerStartLocalPosition);
        _isQueueCustomerVisible = true;
        SetIconVisible(false);
        
        _customerTransitionMoveTween = Tween.LocalPosition(
                _customerTransitionSprite.transform,
                _customerIdleLocalPosition,
                customerStartLocalPosition,
                _customerMoveTweenSettings)
            .OnComplete(this, static controller => controller.ResetTransitionSprite());

        _customerMoveTween = Tween.LocalPosition(
                _customerSprite.transform,
                customerStartLocalPosition,
                _customerIdleLocalPosition,
                _customerMoveTweenSettings)
            .OnComplete(this, static controller =>
            {
                controller.StartCustomerBobbing();
                controller.SetIconVisible(true);
            });
    }

    private void StopCustomerTweens()
    {
        SetCustomerHoverEnabled(false);

        if (_customerMoveTween.isAlive)
            _customerMoveTween.Stop();

        if (_customerTransitionMoveTween.isAlive)
            _customerTransitionMoveTween.Stop();

        StopCustomerBobbing(true);
    }

    private void StartCustomerBobbing()
    {
        if (_customerSprite.sprite == null || !_customerSprite.enabled || _customerBobbingRange <= 0f)
            return;

        StopCustomerBobbing(true);

        _customerBobbingTween = Tween.LocalPositionY(
            _customerSprite.transform,
            _customerIdleLocalPosition.y,
            _customerIdleLocalPosition.y + _customerBobbingRange,
            _customerBobbingTweenSettings);

        SetCustomerHoverEnabled(true);
    }

    private void StopCustomerBobbing(bool resetToIdlePosition)
    {
        if (_customerBobbingTween.isAlive)
            _customerBobbingTween.Stop();

        if (!resetToIdlePosition)
            return;

        var localPosition = _customerSprite.transform.localPosition;
        localPosition.y = _customerIdleLocalPosition.y;
        _customerSprite.transform.localPosition = localPosition;
    }

    private void ResetTransitionSprite() => SetRendererState(_customerTransitionSprite, null, GetCustomerStartLocalPosition());

    private void SetCustomerHoverEnabled(bool enabled)
    {
        if (_customerHoverTween == null)
            return;

        _customerHoverTween.SetTweenEnabled(enabled);
    }

    private void SetRendererState(SpriteRenderer renderer, Sprite sprite, Vector3 localPosition)
    {
        renderer.sprite = sprite;
        renderer.enabled = sprite != null;
        renderer.transform.localPosition = localPosition;
        renderer.transform.localScale =  Vector3.one;
    }

    private Vector3 GetCustomerStartLocalPosition() => _customerIdleLocalPosition + _customerStartLocalOffset;

    private void SetQueuedCustomersIndicator(int count)
    {
        if (_queuedCustomersIndicatorSprite != null)
            _queuedCustomersIndicatorSprite.enabled = count > 1;
    }

    private void SetIconVisible(bool visible)
    {
        if (_iconSprite != null)
            _iconSprite.gameObject.SetActive(visible);
    }

    private int GetQueuedCustomersCount()
    {
        var count = 0;

        foreach (var _ in _customerQueue.Entries)
            count++;

        return count;
    }

    private bool TryGetCurrentAndNextQueuedCustomers(out Customer currentCustomer, out Customer nextCustomer)
    {
        currentCustomer = null;
        nextCustomer = null;

        using var entries = _customerQueue.Entries.GetEnumerator();
        if (!entries.MoveNext())
            return false;

        currentCustomer = entries.Current.Customer;

        if (entries.MoveNext())
            nextCustomer = entries.Current.Customer;

        return true;
    }

    private bool UsesQueueMode() => _mode == CustomerAnimationMode.Queue;
}
