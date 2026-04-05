using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class CustomerQueueNotificationItem : MonoBehaviour, IPointerClickHandler
{
    private enum RemainingTimeBand
    {
        High,
        Medium,
        Low
    }

    [SerializeField]
    private Image _customerImage;

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

    [SerializeField]
    private TweenSettings _introTweenSettings;
    
    [SerializeField]
    private TweenSettings _exitTweenSettings;

    private CustomerWaitStatus _entry;
    private CameraController _cameraController;
    private Tween _remainingTimeColorTween;
    private RemainingTimeBand _currentRemainingTimeBand;
    private bool _remainingTimeBandInitialized;

    public void Initialize(CustomerWaitStatus entry, CameraController cameraController)
    {
        _entry = entry;
        _cameraController = cameraController;

        _customerImage.sprite = _entry.Customer.Icone;
        _customerImage.preserveAspect = true;
        Tween.Scale(transform, Vector3.zero, Vector3.one, _introTweenSettings);
        Refresh();
    }

    private void OnDestroy()
    {
        if (_remainingTimeColorTween.isAlive)
            _remainingTimeColorTween.Stop();
    }

    public void Remove()
    {
        Tween.Scale(transform, Vector3.zero, Vector3.one, _introTweenSettings)
            .OnComplete(this, target => Destroy(target.gameObject));
    }

    private void Update()
    {
        if (_entry == null)
            return;

        Refresh();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cameraController == null)
            return;

        _cameraController.GoImmediatelyToSection(CameraSection.Balcony);
    }

    private void Refresh()
    {
        var normalizedRemaining = _entry.NormalizedRemaining;
        _remainingTimeSlider.normalizedValue = normalizedRemaining;
        ApplyRemainingTimeBand(normalizedRemaining, _remainingTimeBandInitialized);
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
