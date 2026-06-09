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
    private sealed class SlotAssignment
    {
        public Transform Root;
        public LevelLoadoutIngredientSlotType SlotType;
        public Ingredient Ingredient;
        public LevelLoadoutIngredientView View;
    }

    private struct ActiveDrag
    {
        public Ingredient Ingredient;
        public LevelLoadoutIngredientView Source;
        public LevelLoadoutIngredientView Preview;
        public LevelLoadoutInventorySlotView SourceSlotView;
        public SlotAssignment SourceSlot;

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

    [SerializeField, HideInInspector]
    private Transform[] _ingredientSlotRoots = System.Array.Empty<Transform>();

    [SerializeField]
    private Transform _doughSlotRoot;

    [SerializeField]
    private Transform _fillingSlotRoot;

    [SerializeField]
    private Transform _loadoutSlotPrefab;

    [SerializeField]
    private Transform _inventoryRoot;

    [SerializeField]
    private LevelLoadoutIngredientView _loadoutIngredientPrefab;

    [SerializeField]
    private Transform _preferencesRoot;

    [SerializeField]
    private LevelPreferenceView _levelPreferencePrefab;

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

    private readonly List<SlotAssignment> _slotAssignments = new();
    private readonly List<LevelLoadoutIngredientView> _loadoutSlotViews = new();
    private readonly List<LevelLoadoutInventorySlotView> _inventoryItems = new();
    private readonly List<LevelPreferenceView> _preferenceItems = new();
    private readonly List<Transform> _instantiatedSlotRoots = new();

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

    public void HandleInventoryHeld(LevelLoadoutInventorySlotView sourceSlotView, Ingredient ingredient, int availableQuantity, PointerEventData eventData)
    {
        if (availableQuantity <= 0 || !CanAddIngredient(ingredient))
            return;

        TryBeginInventoryPreviewDrag(sourceSlotView, ingredient, eventData);
    }

    public bool CanAddIngredient(Ingredient ingredient)
    {
        if (_selectedLevel == null || ingredient == null)
            return false;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);
        return !IsInLoadout(loadout, ingredient) && HasCapacity(loadout, ingredient);
    }

    public void HandleLoadoutIngredientHeld(LevelLoadoutIngredientView source, Ingredient ingredient, PointerEventData eventData)
    {
        BeginSlotDrag(source, ingredient);
        HandleDrag(eventData.position);
    }

    public void HandleDragInput(Vector2 screenPosition)
    {
        HandleDrag(screenPosition);
    }

    public void HandleEndDragInput(PointerEventData eventData)
    {
        EndDrag(eventData);
    }

    private void TryBeginInventoryPreviewDrag(LevelLoadoutInventorySlotView sourceSlotView, Ingredient ingredient, PointerEventData eventData)
    {
        if (_selectedLevel == null || ingredient == null || _activeDrag.IsActive || _loadoutIngredientPrefab == null || sourceSlotView == null)
            return;

        if (!CanAddIngredient(ingredient))
            return;

        var previewSlot = Instantiate(_loadoutIngredientPrefab, transform);
        previewSlot.name = $"Dragged {ingredient.GetDisplayName()}";
        previewSlot.BindPreview(this, GetSlotType(ingredient), ingredient);
        previewSlot.UpdateDraggedPosition(eventData.position);
        previewSlot.SetDragState(true, sourceSlotView.IsUsingClickGesture);

        _activeDrag = new ActiveDrag
        {
            Ingredient = ingredient,
            Preview = previewSlot,
            SourceSlotView = sourceSlotView
        };
        sourceSlotView.BeginPendingPreview();

        if (sourceSlotView.IsUsingClickGesture)
        {
            sourceSlotView.CancelDragInput();
            previewSlot.BeginExternalDrag(eventData);
        }
    }

    private void BeginSlotDrag(LevelLoadoutIngredientView source, Ingredient ingredient)
    {
        if (_selectedLevel == null || source == null || ingredient == null || _activeDrag.IsActive)
            return;

        _activeDrag = new ActiveDrag
        {
            Ingredient = ingredient,
            Source = source,
            SourceSlot = GetAssignedSlot(source)
        };
        source.SetDragState(true, source.IsUsingClickGesture);
    }

    private void HandleDrag(Vector2 screenPosition)
    {
        if (_activeDrag.Preview != null)
            _activeDrag.Preview.UpdateDraggedPosition(screenPosition);
        else if (_activeDrag.Source != null)
            _activeDrag.Source.UpdateDraggedPosition(screenPosition);
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!_activeDrag.IsActive)
            return;

        if (_activeDrag.IsFromInventoryPreview)
        {
            var added = TryDropPreviewIntoSlot(eventData);

            if (added)
                _activeDrag.SourceSlotView?.ConfirmPendingPreview();
            else
                _activeDrag.SourceSlotView?.CancelPendingPreview();

            if (_activeDrag.Preview != null)
                Destroy(_activeDrag.Preview.gameObject);
        }
        else if (_activeDrag.IsFromSlot)
        {
            if (_activeDrag.Source != null)
                _activeDrag.Source.SetDragState(false);

            var keptInLoadout = TryMoveExistingIngredient(eventData);

            if (!keptInLoadout && TryRemoveIngredient(_activeDrag.Ingredient) && _activeDrag.Source != null)
            {
                ClearSlotAssignment(_activeDrag.SourceSlot);
                Destroy(_activeDrag.Source.gameObject);
            }
            else
            {
                Rebuild();
            }
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
        RebuildPreferences();
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
        var previousAssignments = SnapshotAssignments();
        ClearInstantiatedSlots();

        if (_selectedLevel == null || _loadoutIngredientPrefab == null)
            return;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);
        if (!TryBuildSlotAssignments(loadout))
            return;

        var missingLookup = _levelLoadoutController.GetMissingIngredients(_selectedLevel)
            .ToDictionary(entry => entry.Ingredient, entry => entry.IsMissing);
        RestoreSlotAssignments(previousAssignments, loadout);
        AssignIngredientsToSlots(loadout.Doughs.OrderBy(dough => dough.GetDisplayName()), LevelLoadoutIngredientSlotType.Dough);
        AssignIngredientsToSlots(loadout.Fillings.OrderBy(filling => filling.GetDisplayName()), LevelLoadoutIngredientSlotType.Filling);

        BuildAssignedSlots(missingLookup);
    }

    private void BuildAssignedSlots(IReadOnlyDictionary<Ingredient, bool> missingLookup)
    {
        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            var assignment = _slotAssignments[slotIndex];
            var ingredient = assignment.Ingredient;
            if (assignment.Root == null || ingredient == null)
                continue;

            var slotView = Instantiate(_loadoutIngredientPrefab, assignment.Root);
            var isMissing = ingredient != null && missingLookup.TryGetValue(ingredient, out var missing) && missing;
            slotView.name = $"Loadout Slot {slotIndex}";
            slotView.Bind(this, GetSlotType(ingredient), ingredient, isMissing);
            ResetSlotViewTransform(slotView);
            assignment.View = slotView;
            _loadoutSlotViews.Add(slotView);
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

    private void RebuildPreferences()
    {
        ClearViews(_preferenceItems);

        if (_selectedLevel == null || _preferencesRoot == null || _levelPreferencePrefab == null)
            return;

        foreach (var dough in _selectedLevel.PreferredDoughs
                     .Where(dough => dough != null)
                     .OrderBy(dough => dough.GetDisplayName()))
        {
            var itemView = Instantiate(_levelPreferencePrefab, _preferencesRoot);
            itemView.name = $"Preferred Dough {dough.GetDisplayName()}";
            itemView.Bind(dough);
            _preferenceItems.Add(itemView);
        }

        foreach (var filling in _selectedLevel.PreferredFillings
                     .Where(filling => filling != null)
                     .OrderBy(filling => filling.GetDisplayName()))
        {
            var itemView = Instantiate(_levelPreferencePrefab, _preferencesRoot);
            itemView.name = $"Preferred Filling {filling.GetDisplayName()}";
            itemView.Bind(filling);
            _preferenceItems.Add(itemView);
        }

        foreach (var fillingTag in _selectedLevel.PreferredFillingTags
                     .Where(fillingTag => fillingTag != null)
                     .OrderBy(fillingTag => fillingTag.Name.GetLocalizedString()))
        {
            var itemView = Instantiate(_levelPreferencePrefab, _preferencesRoot);
            itemView.name = $"Preferred Tag {fillingTag.Name.GetLocalizedString()}";
            itemView.Bind(fillingTag);
            _preferenceItems.Add(itemView);
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

    private bool TryDropPreviewIntoSlot(PointerEventData eventData)
    {
        if (!TryGetDropSlot(eventData, out var slot))
            return false;

        if (!AcceptsIngredient(slot, _activeDrag.Ingredient))
            return false;

        if (slot.Ingredient != null)
            return false;

        if (!CanAddIngredient(_activeDrag.Ingredient))
            return false;

        slot.Ingredient = _activeDrag.Ingredient;

        if (!TryAddIngredient(_activeDrag.Ingredient))
        {
            slot.Ingredient = null;
            return false;
        }

        return true;
    }

    private bool TryMoveExistingIngredient(PointerEventData eventData)
    {
        if (!TryGetDropSlot(eventData, out var targetSlot))
            return false;

        if (!AcceptsIngredient(targetSlot, _activeDrag.Ingredient))
            return false;

        if (targetSlot == _activeDrag.SourceSlot)
            return true;

        if (targetSlot.Ingredient != null)
            return true;

        ClearSlotAssignment(_activeDrag.SourceSlot);
        targetSlot.Ingredient = _activeDrag.Ingredient;
        return true;
    }

    private bool TryGetDropSlot(PointerEventData eventData, out SlotAssignment slot)
    {
        for (var slotIndex = _slotAssignments.Count - 1; slotIndex >= 0; slotIndex--)
        {
            var assignment = _slotAssignments[slotIndex];
            if (IsInsideRoot(assignment.Root, eventData))
            {
                slot = assignment;
                return true;
            }
        }

        slot = null;
        return false;
    }

    private static bool HasCapacity(Loadout loadout, Ingredient ingredient)
    {
        if (loadout == null)
            return false;

        return ingredient switch
        {
            Dough => loadout.DoughCount < loadout.MaxDoughs,
            Filling => loadout.FillingCount < loadout.MaxFillings,
            _ => false
        };
    }

    private static bool IsInLoadout(Loadout loadout, Ingredient ingredient)
    {
        if (loadout == null)
            return false;

        return ingredient switch
        {
            Dough dough => loadout.Doughs.Contains(dough),
            Filling filling => loadout.Fillings.Contains(filling),
            _ => false
        };
    }

    private bool TryBuildSlotAssignments(Loadout loadout)
    {
        if (loadout == null)
            return false;

        if (_doughSlotRoot == null)
        {
            Debug.LogError($"{nameof(LevelLoadoutEditorView)} on '{name}' is missing {nameof(_doughSlotRoot)}.", this);
            return false;
        }

        if (_fillingSlotRoot == null)
        {
            Debug.LogError($"{nameof(LevelLoadoutEditorView)} on '{name}' is missing {nameof(_fillingSlotRoot)}.", this);
            return false;
        }

        if (_loadoutSlotPrefab == null)
        {
            Debug.LogError($"{nameof(LevelLoadoutEditorView)} on '{name}' is missing {nameof(_loadoutSlotPrefab)}.", this);
            return false;
        }

        _slotAssignments.Clear();
        CreateSlots(_doughSlotRoot, loadout.MaxDoughs, LevelLoadoutIngredientSlotType.Dough);
        CreateSlots(_fillingSlotRoot, loadout.MaxFillings, LevelLoadoutIngredientSlotType.Filling);
        return true;
    }

    private void CreateSlots(Transform parent, int count, LevelLoadoutIngredientSlotType slotType)
    {
        for (var slotIndex = 0; slotIndex < count; slotIndex++)
        {
            var slotRoot = Instantiate(_loadoutSlotPrefab, parent, false);
            slotRoot.name = $"{slotType} Slot {slotIndex}";
            _instantiatedSlotRoots.Add(slotRoot);
            _slotAssignments.Add(new SlotAssignment
            {
                Root = slotRoot,
                SlotType = slotType
            });
        }
    }

    private void AssignIngredientsToSlots<TIngredient>(IEnumerable<TIngredient> ingredients, LevelLoadoutIngredientSlotType slotType)
        where TIngredient : Ingredient
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient == null || HasAssignedIngredient(ingredient))
                continue;

            var emptySlot = GetFirstEmptySlot(slotType);
            if (emptySlot == null)
                break;

            emptySlot.Ingredient = ingredient;
        }
    }

    private SlotAssignment GetFirstEmptySlot(LevelLoadoutIngredientSlotType slotType)
    {
        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            var slotAssignment = _slotAssignments[slotIndex];
            if (slotAssignment.Root != null &&
                slotAssignment.SlotType == slotType &&
                slotAssignment.Ingredient == null)
            {
                return slotAssignment;
            }
        }

        return null;
    }

    private List<Ingredient> SnapshotAssignments()
    {
        var assignments = new List<Ingredient>(_slotAssignments.Count);

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
            assignments.Add(_slotAssignments[slotIndex].Ingredient);

        return assignments;
    }

    private void RestoreSlotAssignments(IReadOnlyList<Ingredient> previousAssignments, Loadout loadout)
    {
        if (previousAssignments == null || loadout == null)
            return;

        var maxSlots = Mathf.Min(previousAssignments.Count, _slotAssignments.Count);
        for (var slotIndex = 0; slotIndex < maxSlots; slotIndex++)
        {
            var ingredient = previousAssignments[slotIndex];
            if (ingredient == null)
                continue;

            var slotAssignment = _slotAssignments[slotIndex];
            if (!AcceptsIngredient(slotAssignment, ingredient))
                continue;

            if (!IsInLoadout(loadout, ingredient))
                continue;

            slotAssignment.Ingredient = ingredient;
        }
    }

    private bool HasAssignedIngredient(Ingredient ingredient)
    {
        if (ingredient == null)
            return false;

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            if (_slotAssignments[slotIndex].Ingredient == ingredient)
                return true;
        }

        return false;
    }

    private SlotAssignment GetAssignedSlot(LevelLoadoutIngredientView view)
    {
        if (view == null)
            return null;

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            if (_slotAssignments[slotIndex].View == view)
                return _slotAssignments[slotIndex];
        }

        return null;
    }

    private SlotAssignment GetAssignedSlot(Transform root)
    {
        if (root == null)
            return null;

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            if (_slotAssignments[slotIndex].Root == root)
                return _slotAssignments[slotIndex];
        }

        return null;
    }

    private static void ClearSlotAssignment(SlotAssignment slot)
    {
        if (slot == null)
            return;

        slot.Ingredient = null;
        slot.View = null;
    }

    private void ClearInstantiatedSlots()
    {
        for (var index = 0; index < _instantiatedSlotRoots.Count; index++)
        {
            if (_instantiatedSlotRoots[index] != null)
                Destroy(_instantiatedSlotRoots[index].gameObject);
        }

        _instantiatedSlotRoots.Clear();
        _slotAssignments.Clear();
        _loadoutSlotViews.Clear();
    }

    private static bool AcceptsIngredient(SlotAssignment slot, Ingredient ingredient)
    {
        if (slot == null || ingredient == null)
            return false;

        return slot.SlotType == GetSlotType(ingredient);
    }

    private static void ResetSlotViewTransform(LevelLoadoutIngredientView slotView)
    {
        if (slotView == null)
            return;

        if (slotView.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
            return;
        }

        slotView.transform.localPosition = Vector3.zero;
        slotView.transform.localRotation = Quaternion.identity;
        slotView.transform.localScale = Vector3.one;
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
