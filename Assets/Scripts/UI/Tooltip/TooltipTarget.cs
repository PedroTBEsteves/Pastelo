using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TooltipTarget : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    private const float MovementThresholdSqr = 0.01f;

    [SerializeField]
    private LocalizedStringTable _localizationTable;

    [SerializeField]
    private string _localizationKey;

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
    private string _runtimeTextOverride;
    private bool _loggedMissingTable;
    private bool _loggedMissingKey;
    private bool _loggedMissingTableForLocale;
    private bool _loggedMissingEntry;

    public string Text => GetText();
    public TooltipView ViewPrefabOverride => _viewPrefabOverride;
    public ITooltipPresenter Presenter => _presenter as ITooltipPresenter;
    public bool HasTooltipContent => Presenter != null || !string.IsNullOrWhiteSpace(Text);
    public bool HasLegacyViewOverride => _viewPrefabOverride != null;

    public void Configure(string text = null, TooltipView viewPrefabOverride = null, MonoBehaviour presenter = null)
    {
        _runtimeTextOverride = text;
        _viewPrefabOverride = viewPrefabOverride;
        _presenter = presenter;
    }

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

    private void OnValidate()
    {
        ResetLocalizationLogState();
    }

    private string GetText()
    {
        if (!string.IsNullOrWhiteSpace(_runtimeTextOverride))
            return _runtimeTextOverride;

        return ResolveLocalizedText();
    }

    private string ResolveLocalizedText()
    {
        if (_localizationTable == null)
        {
            LogMissingTable();
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(_localizationKey))
        {
            LogMissingKey();
            return string.Empty;
        }

        var table = _localizationTable.GetTable();

        if (table == null)
        {
            LogMissingTableForLocale();
            return string.Empty;
        }

        var entry = table.GetEntry(_localizationKey);

        if (entry == null)
        {
            LogMissingEntry();
            return string.Empty;
        }

        return entry.IsSmart ? entry.GetLocalizedString(new {amount = 1}) : entry.GetLocalizedString();
    }

    private void ResetLocalizationLogState()
    {
        _loggedMissingTable = false;
        _loggedMissingKey = false;
        _loggedMissingTableForLocale = false;
        _loggedMissingEntry = false;
    }

    private void LogMissingTable()
    {
        if (_loggedMissingTable)
            return;

        _loggedMissingTable = true;
        Debug.LogError($"{nameof(TooltipTarget)} on '{name}' is missing a localization table reference.", this);
    }

    private void LogMissingKey()
    {
        if (_loggedMissingKey)
            return;

        _loggedMissingKey = true;
        Debug.LogError($"{nameof(TooltipTarget)} on '{name}' is missing a localization key.", this);
    }

    private void LogMissingTableForLocale()
    {
        if (_loggedMissingTableForLocale)
            return;

        _loggedMissingTableForLocale = true;
        Debug.LogError(
            $"{nameof(TooltipTarget)} on '{name}' could not resolve a String Table for locale '{LocalizationSettings.SelectedLocale?.Identifier.Code}'.",
            this);
    }

    private void LogMissingEntry()
    {
        if (_loggedMissingEntry)
            return;

        _loggedMissingEntry = true;
        Debug.LogError(
            $"{nameof(TooltipTarget)} on '{name}' could not find localization key '{_localizationKey}' in the selected table.",
            this);
    }
}
