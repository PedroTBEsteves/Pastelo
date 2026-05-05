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

    [SerializeField]
    private Transform[] _ingredientSlotRoots = System.Array.Empty<Transform>();

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
            Source = source,
            SourceSlot = GetAssignedSlot(source)
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
        SyncSlotAssignments();
        ClearViews(_loadoutSlotViews);

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
            _slotAssignments[slotIndex].View = null;

        if (_selectedLevel == null || _loadoutIngredientPrefab == null)
            return;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);
        var missingLookup = _levelLoadoutController.GetMissingIngredients(_selectedLevel)
            .ToDictionary(entry => entry.Ingredient, entry => entry.IsMissing);

        var assignedIngredients = new HashSet<Ingredient>();
        var loadoutIngredients = loadout.Doughs
            .OrderBy(dough => dough.GetDisplayName())
            .Cast<Ingredient>()
            .Concat(loadout.Fillings.OrderBy(filling => filling.GetDisplayName()))
            .ToArray();
        var loadoutIngredientLookup = new HashSet<Ingredient>(loadoutIngredients);

        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            var assignment = _slotAssignments[slotIndex];
            if (assignment.Ingredient == null)
                continue;

            if (!loadoutIngredientLookup.Contains(assignment.Ingredient) || !assignedIngredients.Add(assignment.Ingredient))
                assignment.Ingredient = null;
        }

        for (var ingredientIndex = 0; ingredientIndex < loadoutIngredients.Length; ingredientIndex++)
        {
            var ingredient = loadoutIngredients[ingredientIndex];
            if (ingredient == null || assignedIngredients.Contains(ingredient))
                continue;

            var emptySlot = GetFirstEmptySlot();
            if (emptySlot == null)
                break;

            emptySlot.Ingredient = ingredient;
            assignedIngredients.Add(ingredient);
        }

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

        if (slot.Ingredient != null)
            return false;

        if (!HasCapacity(_activeDrag.Ingredient))
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
        SyncSlotAssignments();

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

    private bool HasCapacity(Ingredient ingredient)
    {
        if (_selectedLevel == null)
            return false;

        var loadout = _levelLoadoutController.GetLoadout(_selectedLevel);

        return ingredient switch
        {
            Dough => loadout.DoughCount < loadout.MaxDoughs,
            Filling => loadout.FillingCount < loadout.MaxFillings,
            _ => false
        };
    }

    private void SyncSlotAssignments()
    {
        var configuredRoots = GetConfiguredSlotRoots();

        for (var index = _slotAssignments.Count - 1; index >= 0; index--)
        {
            if (!_slotAssignments[index].Root || !configuredRoots.Contains(_slotAssignments[index].Root))
                _slotAssignments.RemoveAt(index);
        }

        for (var index = 0; index < configuredRoots.Count; index++)
        {
            var root = configuredRoots[index];
            if (GetAssignedSlot(root) == null)
                _slotAssignments.Add(new SlotAssignment { Root = root });
        }
    }

    private IReadOnlyList<Transform> GetConfiguredSlotRoots()
    {
        return _ingredientSlotRoots == null
            ? System.Array.Empty<Transform>()
            : _ingredientSlotRoots.Where(root => root != null).Distinct().ToArray();
    }

    private SlotAssignment GetFirstEmptySlot()
    {
        for (var slotIndex = 0; slotIndex < _slotAssignments.Count; slotIndex++)
        {
            if (_slotAssignments[slotIndex].Root != null && _slotAssignments[slotIndex].Ingredient == null)
                return _slotAssignments[slotIndex];
        }

        return null;
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
