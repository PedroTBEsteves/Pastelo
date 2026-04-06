using System;
using System.Collections;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocaleButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField, Self]
    private Image _image;
    
    [SerializeField]
    private Color _activeColor;
    
    [SerializeField]
    private Color _inactiveColor;

    [SerializeField]
    private string _localeCode;

    private Coroutine _refreshRoutine;
    private Coroutine _selectionRoutine;

    private void Awake()
    {
        _button.onClick.AddListener(SelectLocale);
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
        QueueVisualRefresh();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
        StopTrackedCoroutine(ref _refreshRoutine);
        StopTrackedCoroutine(ref _selectionRoutine);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(SelectLocale);
    }

    private void SelectLocale()
    {
        StopTrackedCoroutine(ref _selectionRoutine);
        _selectionRoutine = StartCoroutine(SelectLocaleWhenReady());
    }

    private IEnumerator SelectLocaleWhenReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = ResolveLocale();
        if (locale == null)
            yield break;

        if (!string.Equals(LocalizationSettings.SelectedLocale?.Identifier.Code, locale.Identifier.Code, StringComparison.OrdinalIgnoreCase))
            LocalizationSettings.SelectedLocale = locale;

        UpdateVisualState();
        _selectionRoutine = null;
    }

    private void HandleSelectedLocaleChanged(Locale _)
    {
        UpdateVisualState();
    }

    private void QueueVisualRefresh()
    {
        StopTrackedCoroutine(ref _refreshRoutine);
        _refreshRoutine = StartCoroutine(RefreshVisualStateWhenReady());
    }

    private IEnumerator RefreshVisualStateWhenReady()
    {
        UpdateVisualState();

        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            yield return LocalizationSettings.InitializationOperation;
            UpdateVisualState();
        }

        _refreshRoutine = null;
    }

    private Locale ResolveLocale()
    {
        if (string.IsNullOrWhiteSpace(_localeCode))
        {
            Debug.LogError($"{nameof(LocaleButton)} on '{name}' is missing a locale code.", this);
            return null;
        }

        var locale = LocalizationSettings.AvailableLocales?.GetLocale(_localeCode);
        if (locale == null)
            Debug.LogError($"{nameof(LocaleButton)} on '{name}' could not find locale '{_localeCode}'.", this);

        return locale;
    }

    private void UpdateVisualState()
    {
        var selectedLocale = LocalizationSettings.SelectedLocale;
        var isActive = selectedLocale != null &&
                       !string.IsNullOrWhiteSpace(_localeCode) &&
                       string.Equals(selectedLocale.Identifier.Code, _localeCode, StringComparison.OrdinalIgnoreCase);

        _image.color = isActive ? _activeColor : _inactiveColor;
    }

    private void StopTrackedCoroutine(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }
}
