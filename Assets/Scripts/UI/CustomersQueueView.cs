using PrimeTween;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(HorizontalLayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class CustomersQueueView : MonoBehaviour
{
    private enum NotificationSide
    {
        Left,
        Right,
    }

    [SerializeField]
    private Vector2 _leftVisibleAnchoredPosition = new(48f, 0f);

    [SerializeField]
    private Vector2 _rightVisibleAnchoredPosition = new(-48f, 0f);

    [SerializeField]
    private CustomerQueueNotificationItem _itemPrefab;

    [SerializeField]
    [Min(0)]
    private int _maxVisibleIndicators;

    [Inject]
    private readonly CustomerQueue _customerQueue;

    [Inject]
    private readonly CameraController _cameraController;

    private RectTransform _rectTransform;
    private Image _image;
    private AudioSource _audioSource;
    private HorizontalLayoutGroup _layoutGroup;
    private readonly Dictionary<int, CustomerQueueNotificationItem> _itemsById = new();
    private NotificationSide _currentSide;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _image = GetComponent<Image>();
        _audioSource = GetComponent<AudioSource>();
        _layoutGroup = GetComponent<HorizontalLayoutGroup>();
        _customerQueue.CustomerArrived += OnCustomerArrived;
        _customerQueue.QueueEntryAdded += OnQueueEntryAdded;
        _customerQueue.QueueEntryRemoved += OnQueueEntryRemoved;
        _cameraController.CurrentSectionChanged += OnCurrentSectionChanged;

        _currentSide = GetSideForBalcony();
        //ApplySide(_currentSide);

        foreach (var entry in _customerQueue.Entries)
        {
            if (!TryAddEntry(entry))
                break;
        }
    }

    private void OnDestroy()
    {
        if (_customerQueue != null)
        {
            _customerQueue.CustomerArrived -= OnCustomerArrived;
            _customerQueue.QueueEntryAdded -= OnQueueEntryAdded;
            _customerQueue.QueueEntryRemoved -= OnQueueEntryRemoved;
        }

        if (_cameraController != null)
            _cameraController.CurrentSectionChanged -= OnCurrentSectionChanged;
    }

    private void OnCustomerArrived(Customer _)
    {
        PlayNotificationSound();
    }

    private void OnQueueEntryAdded(CustomerWaitStatus entry)
    {
        TryAddEntry(entry);
    }

    private void OnQueueEntryRemoved(CustomerWaitStatus entry, CustomerQueueEntryRemovedReason _)
    {
        if (!_itemsById.Remove(entry.Id, out var item) || item == null)
            return;

        item.Remove();
        TryAddNextMissingEntry();
    }

    private void OnCurrentSectionChanged(CameraSection _)
    {
        _currentSide = GetSideForBalcony();
        //ApplySide(_currentSide);
    }

    private NotificationSide GetSideForBalcony() => _cameraController.GetDirectionToSection(CameraSection.Balcony) > 0
        ? NotificationSide.Right
        : NotificationSide.Left;

    private void ApplySide(NotificationSide side)
    {
        var anchor = side == NotificationSide.Left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        var pivot = side == NotificationSide.Left ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);

        _rectTransform.anchorMin = anchor;
        _rectTransform.anchorMax = anchor;
        _rectTransform.pivot = pivot;
        _rectTransform.anchoredPosition = GetVisiblePosition(side);
        _layoutGroup.childAlignment = side == NotificationSide.Left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
    }

    private Vector2 GetVisiblePosition(NotificationSide side) => side == NotificationSide.Left
        ? _leftVisibleAnchoredPosition
        : _rightVisibleAnchoredPosition;

    private void PlayNotificationSound()
    {
        _audioSource.Stop();
        _audioSource.Play();
    }

    private bool TryAddEntry(CustomerWaitStatus entry)
    {
        if (_itemsById.ContainsKey(entry.Id) || _itemPrefab == null)
            return false;

        if (!CanCreateMoreIndicators())
            return false;

        var item = Instantiate(_itemPrefab, transform);
        item.name = $"Queue Item {entry.Id}";
        item.Initialize(entry, _cameraController);

        _itemsById.Add(entry.Id, item);
        return true;
    }

    private void TryAddNextMissingEntry()
    {
        if (!CanCreateMoreIndicators())
            return;

        foreach (var entry in _customerQueue.Entries)
        {
            if (TryAddEntry(entry))
                return;
        }
    }

    private bool CanCreateMoreIndicators() => _maxVisibleIndicators <= 0 || _itemsById.Count < _maxVisibleIndicators;
}
