using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class LevelLoadoutEditorView : MonoBehaviour
{
    private struct ActiveDrag
    {
        public Ingredient Ingredient;
        public LevelLoadoutIngredientView Source;
        public LevelLoadoutIngredientView Preview;
        public LevelLoadoutInventorySlotView SourceSlotView;

        public bool IsFromSlot => Source != null;
        public bool IsFromInventoryPreview => Preview != null;
        public bool IsActive => Ingredient != null;
    }

    [SerializeField]
    private GameObject _panelRoot;

    [SerializeField]
    private TMP_Text _levelNameText;

    [SerializeField]
    private Image _levelSplashImage;

    [SerializeField]
    private Transform _doughSlotsRoot;

    [SerializeField]
    private Transform _fillingSlotsRoot;

    [SerializeField]
    private Transform _inventoryRoot;

    [SerializeField]
    private LevelLoadoutIngredientView _loadoutIngredientPrefab;

    [SerializeField]
    private LevelLoadoutInventorySlotView _inventorySlotPrefab;

    [SerializeField]
    private Button _startLevelButton;
    
    [SerializeField]
    private TextMeshProUGUI _startLevelText;

    [SerializeField]
    private LocalizedString _startLevelLocalizedString;

    [Inject]
    private readonly LevelLoadoutController _levelLoadoutController;

    [Inject]
    private readonly LevelSelector _levelSelector;

    [Inject]
    private readonly Inventory _inventory;

    [Inject]
    private readonly MoneyManager _moneyManager;

    private readonly List<LevelLoadoutIngredientView> _doughSlots = new();
    private readonly List<LevelLoadoutIngredientView> _fillingSlots = new();
    private readonly List<LevelLoadoutInventorySlotView> _inventoryItems = new();

    private Level _selectedLevel;
    private ActiveDrag _activeDrag;
    private void Awake()
    {
        if (_startLevelButton != null)
            _startLevelButton.onClick.AddListener(OnStartLevelClicked);

        if (_levelLoadoutController != null)
            _levelLoadoutController.LoadoutChanged += OnLoadoutChanged;

        if (_inventory != null)
            _inventory.Changed += OnInventoryChanged;

        if (_moneyManager != null)
            _moneyManager.MoneyChanged += OnMoneyChanged;

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_startLevelButton != null)
            _startLevelButton.onClick.RemoveListener(OnStartLevelClicked);

        if (_levelLoadoutController != null)
            _levelLoadoutController.LoadoutChanged -= OnLoadoutChanged;

        if (_inventory != null)
            _inventory.Changed -= OnInventoryChanged;

        if (_moneyManager != null)
            _moneyManager.MoneyChanged -= OnMoneyChanged;
    }

    public void Show(Level level)
    {
        _selectedLevel = level;
        SetVisible(level != null);
        Rebuild();
    }

    public bool TryBeginInventoryPreviewDrag(LevelLoadoutInventorySlotView sourceSlotView, Ingredient ingredient, PointerEventData eventData)
    {
        if (_selectedLevel == null || ingredient == null || _activeDrag.IsActive || _loadoutIngredientPrefab == null || sourceSlotView == null)
            return false;

        var previewSlot = Instantiate(_loadoutIngredientPrefab, transform);
        previewSlot.name = $"Dragged {ingredient.GetDisplayName()}";
        previewSlot.BindPreview(this, GetSlotType(ingredient), ingredient);
        previewSlot.UpdateDraggedPosition(eventData.position);
        previewSlot.SetDragState(true);

        _activeDrag = new ActiveDrag
        {
            Ingredient = ingredient,
            Preview = previewSlot,
            SourceSlotView = sourceSlotView
        };
        sourceSlotView.BeginPendingPreview();
        return true;
    }

    public void BeginSlotDrag(LevelLoadoutIngredientView source, Ingredient ingredient)
    {
        if (_selectedLevel == null || source == null || ingredient == null || _activeDrag.IsActive)
            return;

        _activeDrag = new ActiveDrag
        {
            Ingredient = ingredient,
            Source = source
        };
        source.SetDragState(true);
    }

    public void HandleDrag(PointerEventData eventData)
    {
        if (_activeDrag.Preview != null)
            _activeDrag.Preview.UpdateDraggedPosition(eventData.position);
        else if (_activeDrag.Source != null)
            _activeDrag.Source.UpdateDraggedPosition(eventData.position);
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!_activeDrag.IsActive)
            return;

        if (_activeDrag.IsFromInventoryPreview)
        {
            var added = TryDropPreviewIntoRoot(eventData);

            if (added)
                _activeDrag.SourceSlotView?.ConfirmPendingPreview();
            else
                _activeDrag.SourceSlotView?.CancelPendingPreview();

            if (_activeDrag.Preview != null)
                Destroy(_activeDrag.Preview.gameObject);
        }
        else if (_activeDrag.IsFromSlot)
        {
            var keptInLoadout = IsDroppedInsideCompatibleRoot(_activeDrag.Ingredient, eventData);

            if (_activeDrag.Source != null)
                _activeDrag.Source.SetDragState(false);

            if (!keptInLoadout && TryRemoveIngredient(_activeDrag.Ingredient) && _activeDrag.Source != null)
                Destroy(_activeDrag.Source.gameObject);
            else if (keptInLoadout)
                Rebuild();
        }

        _activeDrag = default;
    }

    private void OnStartLevelClicked()
    {
        if (_selectedLevel == null)
            return;

        _levelSelector.PlayLevel(_selectedLevel).Forget();
    }

    private void OnLoadoutChanged(Level level)
    {
        if (_selectedLevel == level)
            Rebuild();
    }

    private void OnInventoryChanged()
    {
        if (_selectedLevel != null)
            Rebuild();
    }

    private void OnMoneyChanged(MoneyChangedEvent _)
    {
        if (_selectedLevel != null)
            RefreshStartButton();
    }

    private void Rebuild()
    {
        RefreshHeader();
        RebuildSlots();
        RebuildProjectedInventory();
        RefreshStartButton();
    }

    private void RefreshHeader()
    {
        if (_levelNameText != null)
            _levelNameText.SetText(_selectedLevel != null ? _selectedLevel.Name.GetLocalizedString() : string.Empty);

        _levelSplashImage.sprite = _selectedLevel.SplashImage;
    }

    private void RebuildSlots()
    {
        ClearViews(_doughSlots);
        ClearViews(_fillingSlots);

        if (_selectedLevel == null || _loadoutIngredientPrefab == null)
            return;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);
        var missingLookup = _levelLoadoutController.GetMissingIngredients(_selectedLevel)
            .ToDictionary(entry => entry.Ingredient, entry => entry.IsMissing);

        BuildSlots(
            _doughSlotsRoot,
            _doughSlots,
            LevelLoadoutIngredientSlotType.Dough,
            loadout.Doughs.OrderBy(dough => dough.GetDisplayName()).Cast<Ingredient>().ToArray(),
            missingLookup);

        BuildSlots(
            _fillingSlotsRoot,
            _fillingSlots,
            LevelLoadoutIngredientSlotType.Filling,
            loadout.Fillings.OrderBy(filling => filling.GetDisplayName()).Cast<Ingredient>().ToArray(),
            missingLookup);
    }

    private void BuildSlots(
        Transform root,
        List<LevelLoadoutIngredientView> targetList,
        LevelLoadoutIngredientSlotType slotType,
        IReadOnlyList<Ingredient> assignedIngredients,
        IReadOnlyDictionary<Ingredient, bool> missingLookup)
    {
        if (root == null || assignedIngredients == null)
            return;

        for (var slotIndex = 0; slotIndex < assignedIngredients.Count; slotIndex++)
        {
            var slotView = Instantiate(_loadoutIngredientPrefab, root);
            var ingredient = assignedIngredients[slotIndex];
            var isMissing = ingredient != null && missingLookup.TryGetValue(ingredient, out var missing) && missing;
            slotView.name = $"{slotType} Slot {slotIndex}";
            slotView.Bind(this, slotType, ingredient, isMissing);
            targetList.Add(slotView);
        }
    }

    private void RebuildProjectedInventory()
    {
        ClearViews(_inventoryItems);

        if (_selectedLevel == null || _inventoryRoot == null || _inventorySlotPrefab == null)
            return;

        var projectedInventory = _levelLoadoutController.GetProjectedInventory(_selectedLevel);

        for (var index = 0; index < projectedInventory.Count; index++)
        {
            var itemView = Instantiate(_inventorySlotPrefab, _inventoryRoot);
            itemView.name = $"Projected Inventory Item {index}";
            itemView.Bind(this, projectedInventory[index]);
            _inventoryItems.Add(itemView);
        }
    }

    private void RefreshStartButton()
    {
        _startLevelButton.interactable = _selectedLevel != null && _levelSelector.CanPlayLevel(_selectedLevel);
        _startLevelText.text =
            _startLevelLocalizedString.GetLocalizedString(new { price = TextUtils.FormatAsMoney(_selectedLevel.PriceToPlay) });
    }

    private bool TryAddIngredient(Ingredient ingredient)
    {
        return ingredient switch
        {
            Dough dough => _levelLoadoutController.TryAddDough(_selectedLevel, dough),
            Filling filling => _levelLoadoutController.TryAddFilling(_selectedLevel, filling),
            _ => false
        };
    }

    private bool TryRemoveIngredient(Ingredient ingredient)
    {
        return ingredient switch
        {
            Dough dough => _levelLoadoutController.TryRemoveDough(_selectedLevel, dough),
            Filling filling => _levelLoadoutController.TryRemoveFilling(_selectedLevel, filling),
            _ => false
        };
    }

    private void SetVisible(bool isVisible)
    {
        var root = _panelRoot != null ? _panelRoot : gameObject;
        root.SetActive(isVisible);
    }

    private static LevelLoadoutIngredientSlotType GetSlotType(Ingredient ingredient)
    {
        return ingredient switch
        {
            Dough => LevelLoadoutIngredientSlotType.Dough,
            Filling => LevelLoadoutIngredientSlotType.Filling,
            _ => throw new System.ArgumentException("Unsupported ingredient type.", nameof(ingredient))
        };
    }

    private bool TryDropPreviewIntoRoot(PointerEventData eventData)
    {
        if (!TryGetDropRootType(eventData, out var rootType))
            return false;

        if (rootType != GetSlotType(_activeDrag.Ingredient))
            return false;

        if (!HasCapacity(rootType))
            return false;

        return TryAddIngredient(_activeDrag.Ingredient);
    }

    private bool IsDroppedInsideCompatibleRoot(Ingredient ingredient, PointerEventData eventData)
    {
        if (!TryGetDropRootType(eventData, out var rootType))
            return false;

        return rootType == GetSlotType(ingredient);
    }

    private bool TryGetDropRootType(PointerEventData eventData, out LevelLoadoutIngredientSlotType rootType)
    {
        if (IsInsideRoot(_doughSlotsRoot, eventData))
        {
            rootType = LevelLoadoutIngredientSlotType.Dough;
            return true;
        }

        if (IsInsideRoot(_fillingSlotsRoot, eventData))
        {
            rootType = LevelLoadoutIngredientSlotType.Filling;
            return true;
        }

        rootType = default;
        return false;
    }

    private bool HasCapacity(LevelLoadoutIngredientSlotType slotType)
    {
        if (_selectedLevel == null)
            return false;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);

        return slotType switch
        {
            LevelLoadoutIngredientSlotType.Dough => loadout.DoughCount < loadout.MaxDoughs,
            LevelLoadoutIngredientSlotType.Filling => loadout.FillingCount < loadout.MaxFillings,
            _ => false
        };
    }

    private static bool IsInsideRoot(Transform root, PointerEventData eventData)
    {
        if (root is not RectTransform rectTransform)
            return false;

        var eventCamera = eventData.pressEventCamera != null
            ? eventData.pressEventCamera
            : eventData.enterEventCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventCamera);
    }

    private static void ClearViews<T>(List<T> views) where T : Component
    {
        for (var index = 0; index < views.Count; index++)
        {
            if (views[index] != null)
                Destroy(views[index].gameObject);
        }

        views.Clear();
    }
}
