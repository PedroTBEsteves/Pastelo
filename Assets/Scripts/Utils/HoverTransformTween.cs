using KBCore.Refs;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class HoverTransformTween : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TweenSettings _transformTweenSettings;

    [SerializeField]
    private bool _useScale = true;

    [SerializeField]
    private Vector3 _hoverScale = Vector3.one;

    [SerializeField]
    private bool _usePosition;

    [SerializeField]
    private Vector3 _hoverLocalPosition;

    [SerializeField]
    private bool _useRotation;

    [SerializeField]
    private Vector3 _hoverLocalEulerAngles;

    [SerializeField]
    private bool _isTweenEnabled = true;

    private Sequence _activeSequence;
    private Vector3 _initialLocalScale;
    private Vector3 _initialLocalPosition;
    private Vector3 _initialLocalEulerAngles;
    private bool _isPointerOver;

    public bool IsTweenEnabled => _isTweenEnabled;

    private void Awake()
    {
        _initialLocalScale = transform.localScale;
        _initialLocalPosition = transform.localPosition;
        _initialLocalEulerAngles = transform.localEulerAngles;
    }

    private void OnEnable()
    {
        if (!_isTweenEnabled)
            return;

        RefreshPointerOverState();

        if (_isPointerOver)
            PlayHoverStateTween();
    }

    private void OnDisable()
    {
        StopActiveSequence();
        ApplyInitialState();
    }

    private void OnDestroy()
    {
        StopActiveSequence();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;

        if (!_isTweenEnabled)
            return;

        PlayHoverStateTween();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;

        if (!_isTweenEnabled)
        {
            ApplyInitialState();
            return;
        }

        PlayInitialStateTween();
    }

    public void SetTweenEnabled(bool enabled)
    {
        if (_isTweenEnabled == enabled)
            return;

        _isTweenEnabled = enabled;

        if (_isTweenEnabled)
        {
            RefreshPointerOverState();

            if (_isPointerOver)
                PlayHoverStateTween();

            return;
        }

        StopActiveSequence();
        ApplyInitialState();
    }

    public void EnableTween()
    {
        SetTweenEnabled(true);
    }

    public void DisableTween()
    {
        SetTweenEnabled(false);
    }

    private void PlayHoverStateTween()
    {
        if (!_isTweenEnabled || !HasAnyEnabledTransform())
            return;

        StopActiveSequence();

        var sequence = Sequence.Create(useUnscaledTime: _transformTweenSettings.useUnscaledTime);

        if (_useScale)
            sequence.Group(Tween.Scale(transform, transform.localScale, _hoverScale, _transformTweenSettings));

        if (_usePosition)
            sequence.Group(Tween.LocalPosition(transform, transform.localPosition, _hoverLocalPosition, _transformTweenSettings));

        if (_useRotation)
            sequence.Group(Tween.LocalRotation(transform, transform.localEulerAngles, _hoverLocalEulerAngles, _transformTweenSettings));

        _activeSequence = sequence;
    }

    private void PlayInitialStateTween()
    {
        if (!_isTweenEnabled || !HasAnyEnabledTransform())
            return;

        StopActiveSequence();

        var sequence = Sequence.Create(useUnscaledTime: _transformTweenSettings.useUnscaledTime);

        if (_useScale)
            sequence.Group(Tween.Scale(transform, transform.localScale, _initialLocalScale, _transformTweenSettings));

        if (_usePosition)
            sequence.Group(Tween.LocalPosition(transform, transform.localPosition, _initialLocalPosition, _transformTweenSettings));

        if (_useRotation)
            sequence.Group(Tween.LocalRotation(transform, transform.localEulerAngles, _initialLocalEulerAngles, _transformTweenSettings));

        _activeSequence = sequence;
    }

    private bool HasAnyEnabledTransform() => _useScale || _usePosition || _useRotation;

    private void StopActiveSequence()
    {
        if (_activeSequence.isAlive)
            _activeSequence.Stop();
    }

    private void ApplyInitialState()
    {
        if (_useScale)
            transform.localScale = _initialLocalScale;

        if (_usePosition)
            transform.localPosition = _initialLocalPosition;

        if (_useRotation)
            transform.localEulerAngles = _initialLocalEulerAngles;
    }

    private void RefreshPointerOverState()
    {
        _isPointerOver = IsPointerCurrentlyOverThisObject();
    }

    private bool IsPointerCurrentlyOverThisObject()
    {
        if (EventSystem.current == null)
            return false;

        var pointerPosition = GetCurrentPointerPosition();
        if (pointerPosition == null)
            return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition.Value
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (var result in raycastResults)
        {
            var resultObject = result.gameObject;
            if (resultObject == null)
                continue;

            if (resultObject == gameObject || resultObject.transform.IsChildOf(transform))
                return true;

            var hoverHandler = ExecuteEvents.GetEventHandler<IPointerEnterHandler>(resultObject);
            if (hoverHandler == gameObject)
                return true;
        }

        return false;
    }

    private Vector2? GetCurrentPointerPosition()
    {
        if (Pointer.current != null)
            return Pointer.current.position.ReadValue();

        return Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2?)Input.mousePosition;
    }
}
