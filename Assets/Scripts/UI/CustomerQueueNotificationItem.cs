using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class CustomerQueueNotificationItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image _customerImage;

    [SerializeField]
    private Slider _remainingTimeSlider;

    private CustomerWaitStatus _entry;
    private CameraController _cameraController;

    public void Initialize(CustomerWaitStatus entry, CameraController cameraController)
    {
        _entry = entry;
        _cameraController = cameraController;

        _customerImage.sprite = _entry.Customer.Sprite;
        _customerImage.preserveAspect = true;
        Refresh();
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
        _remainingTimeSlider.normalizedValue = _entry.NormalizedRemaining;
    }
}
