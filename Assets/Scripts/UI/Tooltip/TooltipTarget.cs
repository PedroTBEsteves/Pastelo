using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTarget : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    private const float MovementThresholdSqr = 0.01f;

    [SerializeField, TextArea]
    private string _text;

    [SerializeField]
    private TooltipView _viewPrefabOverride;

    [SerializeField]
    private MonoBehaviour _presenter;

    [Inject]
    private readonly TooltipSettings _tooltipSettings;

    [Inject]
    private readonly ITooltipService _tooltipService;

    private bool _isHovered;
    private bool _isTooltipVisible;
    private float _hoverStartTime;
    private Vector2 _lastPointerPosition;
    private Camera _lastEventCamera;

    public string Text => _text;
    public TooltipView ViewPrefabOverride => _viewPrefabOverride;
    public ITooltipPresenter Presenter => _presenter as ITooltipPresenter;
    public bool HasTooltipContent => Presenter != null || !string.IsNullOrWhiteSpace(_text);
    public bool HasLegacyViewOverride => _viewPrefabOverride != null;

    private void Update()
    {
        if (!_isHovered || _isTooltipVisible || (!HasTooltipContent && !HasLegacyViewOverride) || _tooltipSettings == null || _tooltipService == null)
            return;

        if (Time.unscaledTime - _hoverStartTime < _tooltipSettings.HoverDelay)
            return;

        _isTooltipVisible = _tooltipService.Show(this, _lastPointerPosition, _lastEventCamera);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _isTooltipVisible = false;
        _hoverStartTime = Time.unscaledTime;
        CachePointerData(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        var previousPosition = _lastPointerPosition;
        CachePointerData(eventData);

        if (_isTooltipVisible)
        {
            _tooltipService.UpdatePosition(this, _lastPointerPosition, _lastEventCamera);
            return;
        }

        if ((_lastPointerPosition - previousPosition).sqrMagnitude > MovementThresholdSqr)
            _hoverStartTime = Time.unscaledTime;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _isTooltipVisible = false;
        _tooltipService?.Hide(this);
    }

    private void OnDisable()
    {
        if (!_isHovered && !_isTooltipVisible)
            return;

        _isHovered = false;
        _isTooltipVisible = false;
        _tooltipService?.Hide(this);
    }

    private void CachePointerData(PointerEventData eventData)
    {
        _lastPointerPosition = eventData.position;
        _lastEventCamera = eventData.enterEventCamera
            ?? eventData.pressEventCamera
            ?? eventData.pointerCurrentRaycast.module?.eventCamera;
    }
}
