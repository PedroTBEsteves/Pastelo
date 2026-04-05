using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipService : MonoBehaviour, ITooltipService
{
    private const string DefaultTooltipViewResourcePath = "UI/Tooltip/TooltipView";

    private TooltipSettings _settings;
    private TooltipView _currentView;
    private TooltipTarget _currentTarget;

    [SerializeField]
    private Canvas _canvas;

    private RectTransform _canvasRectTransform;
    private TooltipView _defaultViewPrefab;

    public void Configure(TooltipSettings settings)
    {
        _settings = settings != null ? settings : TooltipSettings.CreateFallback();
        ResolveCanvas();
        EnsurePointerRaycasters();
    }

    public bool Show(TooltipTarget target, Vector2 screenPosition, Camera eventCamera)
    {
        if (target == null || (!target.HasTooltipContent && !target.HasLegacyViewOverride))
            return false;

        if (!ResolveCanvas())
            return false;

        var targetViewPrefab = ResolveViewPrefab(target);

        if (targetViewPrefab == null)
            return false;

        if (_currentTarget != target)
        {
            DestroyCurrentView();
            _currentTarget = target;
            _currentView = Instantiate(targetViewPrefab, _canvasRectTransform);
        }

        if (!TryConfigureCurrentView(target))
        {
            DestroyCurrentView();
            return false;
        }

        _currentView.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currentView.RectTransform);
        UpdatePosition(target, screenPosition, eventCamera);
        return true;
    }

    public void UpdatePosition(TooltipTarget target, Vector2 screenPosition, Camera eventCamera)
    {
        if (_currentTarget != target || _currentView == null || !ResolveCanvas())
            return;

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            eventCamera = null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                screenPosition + _settings.ScreenOffset,
                eventCamera,
                out var anchoredPosition))
        {
            return;
        }

        _currentView.RectTransform.anchoredPosition = ClampToCanvas(anchoredPosition, _currentView.RectTransform);
    }

    public void Hide(TooltipTarget target)
    {
        if (_currentTarget != target)
            return;

        DestroyCurrentView();
    }

    private Vector2 ClampToCanvas(Vector2 anchoredPosition, RectTransform tooltipRect)
    {
        var canvasSize = _canvasRectTransform.rect.size;
        var tooltipSize = tooltipRect.rect.size;
        var pivot = tooltipRect.pivot;

        var minX = -canvasSize.x * 0.5f + tooltipSize.x * pivot.x;
        var maxX = canvasSize.x * 0.5f - tooltipSize.x * (1f - pivot.x);
        var minY = -canvasSize.y * 0.5f + tooltipSize.y * pivot.y;
        var maxY = canvasSize.y * 0.5f - tooltipSize.y * (1f - pivot.y);

        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, minY, maxY);
        return anchoredPosition;
    }

    private TooltipView ResolveViewPrefab(TooltipTarget target)
    {
        var presenterViewPrefab = target.Presenter?.GetViewPrefab(target);

        if (presenterViewPrefab != null)
            return presenterViewPrefab;

        if (target.ViewPrefabOverride != null)
            return target.ViewPrefabOverride;

        if (_defaultViewPrefab == null)
            _defaultViewPrefab = Resources.Load<TooltipView>(DefaultTooltipViewResourcePath);

        return _defaultViewPrefab != null ? _defaultViewPrefab : CreateFallbackViewPrefab();
    }

    private bool TryConfigureCurrentView(TooltipTarget target)
    {
        if (_currentView == null)
            return false;

        var presenter = target.Presenter;

        if (presenter != null && presenter.Configure(_currentView, target))
            return true;

        return _currentView.Bind(target);
    }

    private TooltipView CreateFallbackViewPrefab()
    {
        var rootObject = new GameObject("TooltipView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(TooltipView));
        rootObject.transform.SetParent(transform, false);
        rootObject.SetActive(false);

        var rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.zero;
        rootRect.pivot = new Vector2(0f, 1f);

        var background = rootObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.85f);
        background.raycastTarget = false;

        var layoutGroup = rootObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(12, 12, 8, 8);
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        var contentSizeFitter = rootObject.GetComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(rootObject.transform, false);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 24f;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;

        var view = rootObject.GetComponent<TooltipView>();
        view.Initialize(text);
        _defaultViewPrefab = view;
        return _defaultViewPrefab;
    }

    private bool ResolveCanvas()
    {
        if (_canvas == null)
        {
            _canvas = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(canvas => canvas.isRootCanvas && canvas.gameObject.scene.IsValid())
                .OrderByDescending(GetCanvasPriority)
                .FirstOrDefault();
        }

        if (_canvas == null)
            return false;

        _canvasRectTransform = (RectTransform)_canvas.transform;
        return true;
    }

    private static int GetCanvasPriority(Canvas canvas)
    {
        var renderModePriority = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? 2 :
            canvas.renderMode == RenderMode.ScreenSpaceCamera ? 1 : 0;
        return renderModePriority * 10000 + canvas.sortingOrder;
    }

    private void EnsurePointerRaycasters()
    {
        var mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (mainCamera.GetComponent<Physics2DRaycaster>() == null)
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();

        if (mainCamera.GetComponent<PhysicsRaycaster>() == null)
            mainCamera.gameObject.AddComponent<PhysicsRaycaster>();
    }

    private void DestroyCurrentView()
    {
        if (_currentView != null)
            Destroy(_currentView.gameObject);

        _currentView = null;
        _currentTarget = null;
    }
}
