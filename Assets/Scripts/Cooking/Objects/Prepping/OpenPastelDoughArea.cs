using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenPastelDoughArea : ValidatedMonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField, Child]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private BoxCollider2D _ingredientsArea;

    [SerializeField]
    private BoxCollider2D _closeDragStartArea;

    [SerializeField]
    private BoxCollider2D _closeDragReferenceArea;

    [SerializeField]
    private SpriteRenderer _closeDragFillRenderer;

    [SerializeField, Range(0f, 1f)]
    private float _closeDragReleaseThresholdNormalized = 0.5f;

    [SerializeField]
    private DraggableClosedPastel _closedPastelPrefab;
    
    [SerializeField]
    private SpriteRenderer _comboIndicatorRenderer;

    [SerializeField]
    private Transform _ingredientsRoot;
    
    [Inject]
    private readonly PastelCookingSettings _pastelCookingSettings;
    
    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly RecipeGeneratorSettings _recipeGeneratorSettings;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    private OpenPastelDough _pastel;
    private Dough _currentDough;
    
    private List<DraggableFilling> _fillings = new List<DraggableFilling>();

    private TutorialTarget _tutorialTarget;
    private bool _isCloseDragActive;
    private int _activeCloseDragPointerId = int.MinValue;
    private float _closeDragProgressNormalized;
    
    public int FillingsCount => _fillings.Count;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.ClosePastelArea);
        _tutorialTargetRegistry.Register(_tutorialTarget);
        ResetCloseDragVisual();
        RefreshComboIndicator();
    }

    private void OnDestroy()
    {
        CancelCloseDrag();
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }
    
    public bool TryOpenDough(Dough dough)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.UseDough, dough))
            return false;

        if (_pastel != null)
            return false;
        
        _spriteRenderer.sprite = dough.OpenDoughSprite;
        _currentDough = dough;
        _pastel = new OpenPastelDough(dough, _recipeGeneratorSettings.MaxFillingsInclusive);
        _tutorialEvents.PublishDoughOpened(dough);
        ResetCloseDragVisual();
        RefreshComboIndicator();

        return true;
    }

    public bool TryAddFilling(DraggableFilling filling)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.AddFilling, filling.Ingredient))
            return false;

        if (_pastel == null)
            return false;

        if (!_pastel.TryAddFilling(filling.Ingredient))
            return false;

        SnapFillingToClosestAvailableSlot(filling);
        _fillings.Add(filling);
        UpdateFillingsSortingOrder();
        RefreshComboIndicator();
        _tutorialEvents.PublishFillingAdded(filling.Ingredient);
        return true;
    }

    public bool TryRepositionFilling(DraggableFilling filling, Vector3 dropPosition)
    {
        if (_pastel == null || !_fillings.Contains(filling))
            return false;

        var slotPositions = GetSlotPositions(filling.GetSlotPosition().z).ToList();
        var occupiedFillingsBySlot = GetOccupiedFillingsBySlot(slotPositions);
        var currentSlotIndex = GetClosestSlotIndex(slotPositions, filling.GetSlotPosition());
        var targetSlotIndex = GetClosestSlotIndex(slotPositions, dropPosition);

        if (currentSlotIndex < 0 || targetSlotIndex < 0)
            return false;

        var hasAvailableSlot = occupiedFillingsBySlot.Count < slotPositions.Count;

        if (!hasAvailableSlot
            && occupiedFillingsBySlot.TryGetValue(targetSlotIndex, out var targetFilling)
            && targetFilling != null
            && targetFilling != filling)
        {
            SetFillingToSlot(targetFilling, slotPositions[currentSlotIndex]);
            SetFillingToSlot(filling, slotPositions[targetSlotIndex]);
        }
        else
        {
            SnapFillingToClosestAvailableSlot(filling, dropPosition, slotPositions);
        }

        _fillings.Remove(filling);
        _fillings.Add(filling);
        UpdateFillingsSortingOrder();
        RefreshComboIndicator();
        return true;
    }

    public bool RemoveFilling(DraggableFilling filling)
    {
        if (filling == null)
            return false;

        var removedFromList = _fillings.Remove(filling);
        if (!removedFromList)
            return false;

        _pastel?.TryRemoveFilling(filling.Ingredient);
        UpdateFillingsSortingOrder();
        RefreshComboIndicator();
        return true;
    }

    private void Close()
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.ClosePastel))
            return;

        if (_pastel == null)
            return;

        var closedPastel = _pastel.Close(_pastelCookingSettings, BuildFillingSlots());
        _pastel = null;
        _currentDough = null;
        _spriteRenderer.sprite = null;
        ResetCloseDragVisual();
        RefreshComboIndicator();
        var draggableClosedPastel = Instantiate(_closedPastelPrefab, transform.position + Vector3.forward, Quaternion.identity, transform.parent);
        draggableClosedPastel.Initialize(closedPastel);
        foreach (var fillings in _fillings)
            Destroy(fillings.gameObject);
        _fillings.Clear();
        _tutorialEvents.PublishPastelClosed(draggableClosedPastel);
        //_cameraController.GoToNextSection();
    }

    private bool CanStartCloseDrag(PointerEventData eventData)
    {
        if (_pastel == null)
            return false;

        if (!_interactionGate.CanInteract(TutorialInteractionType.ClosePastel))
            return false;

        if (_closeDragStartArea == null || _closeDragReferenceArea == null)
            return false;

        var pointerWorldPosition = GetPointerWorldPosition(eventData);
        pointerWorldPosition.z = _closeDragStartArea.bounds.center.z;

        return _closeDragStartArea.bounds.Contains(pointerWorldPosition);
    }

    private Vector3 GetPointerWorldPosition(PointerEventData eventData)
    {
        return _cameraController.ScreenToWorldPoint(eventData.position);
    }

    private float GetCloseDragProgress(PointerEventData eventData)
    {
        if (_closeDragReferenceArea == null)
            return 0f;

        var bounds = _closeDragReferenceArea.bounds;
        var width = bounds.size.x;
        if (width <= Mathf.Epsilon)
            return 0f;

        return Mathf.Clamp01(Mathf.InverseLerp(bounds.min.x, bounds.max.x, GetPointerWorldPosition(eventData).x));
    }

    private void StartCloseDrag(PointerEventData eventData)
    {
        _isCloseDragActive = true;
        _activeCloseDragPointerId = eventData.pointerId;
        UpdateCloseDragProgress(GetCloseDragProgress(eventData));
    }

    private void UpdateCloseDragProgress(float progressNormalized)
    {
        _closeDragProgressNormalized = Mathf.Clamp01(progressNormalized);
        UpdateCloseDragVisual();
    }

    private void TryCompleteCloseDrag()
    {
        if (!_isCloseDragActive)
            return;

        var releaseThreshold = Mathf.Clamp01(_closeDragReleaseThresholdNormalized);
        var shouldClose = _closeDragProgressNormalized >= 1f
                          || _closeDragProgressNormalized >= releaseThreshold;

        CancelCloseDrag();

        if (shouldClose)
            Close();
    }

    private void CancelCloseDrag()
    {
        _isCloseDragActive = false;
        _activeCloseDragPointerId = int.MinValue;
        _closeDragProgressNormalized = 0f;
        ResetCloseDragVisual();
    }

    private void ResetCloseDragVisual()
    {
        if (_currentDough != null)
            _spriteRenderer.sprite = _currentDough.OpenDoughSprite;

        if (_closeDragFillRenderer == null)
            return;

        _closeDragFillRenderer.enabled = false;
        _closeDragFillRenderer.sprite = null;
        _closeDragFillRenderer.drawMode = SpriteDrawMode.Simple;
    }

    private void UpdateCloseDragVisual()
    {
        if (_currentDough == null || _spriteRenderer == null || _closeDragFillRenderer == null)
            return;

        var frameCount = _currentDough.GetCloseDragFrameCount();
        if (_closeDragProgressNormalized <= 0f || frameCount <= 0)
        {
            ResetCloseDragVisual();
            return;
        }

        var frameIndex = Mathf.Clamp(Mathf.FloorToInt(_closeDragProgressNormalized * frameCount), 0, frameCount - 1);
        _spriteRenderer.sprite = _currentDough.GetCloseDragBaseLayerFrame(frameIndex);
        _closeDragFillRenderer.sprite = _currentDough.GetCloseDragCoverLayerFrame(frameIndex);
        _closeDragFillRenderer.enabled = _closeDragFillRenderer.sprite != null;
        _closeDragFillRenderer.drawMode = SpriteDrawMode.Simple;
    }

    private void SnapFillingToClosestAvailableSlot(DraggableFilling filling)
    {
        SnapFillingToClosestAvailableSlot(filling, filling.transform.position);
    }

    private void SnapFillingToClosestAvailableSlot(DraggableFilling filling, Vector3 targetPosition)
    {
        SnapFillingToClosestAvailableSlot(filling, targetPosition, GetSlotPositions(filling.transform.position.z).ToList());
    }

    private void SnapFillingToClosestAvailableSlot(DraggableFilling filling, Vector3 targetPosition, IReadOnlyList<Vector3> slotPositions)
    {
        var occupiedSlotIndices = GetOccupiedSlotIndices(slotPositions, filling);
        var closestAvailableSlotIndex = GetClosestAvailableSlotIndex(slotPositions, occupiedSlotIndices, targetPosition);

        if (closestAvailableSlotIndex < 0)
            return;

        SetFillingToSlot(filling, slotPositions[closestAvailableSlotIndex]);
    }

    private void RefreshComboIndicator()
    {
        if (_comboIndicatorRenderer == null)
            return;

        _comboIndicatorRenderer.enabled = _pastel != null
                                         && PastelComboPricer.HasMatchingCombo(BuildFillingSlots(), _pastelCookingSettings);
    }

    private void UpdateFillingsSortingOrder()
    {
        for (var index = 0; index < _fillings.Count; index++)
        {
            if (_fillings[index] == null)
                continue;

            _fillings[index].SetSortingOrder(index + 1);
        }
    }

    private Dictionary<int, DraggableFilling> GetOccupiedFillingsBySlot(IReadOnlyList<Vector3> slotPositions)
    {
        var occupiedFillingsBySlot = new Dictionary<int, DraggableFilling>();

        foreach (var existingFilling in _fillings)
        {
            if (existingFilling == null)
                continue;

            var closestSlotIndex = GetClosestSlotIndex(slotPositions, existingFilling.GetSlotPosition());
            if (closestSlotIndex < 0 || occupiedFillingsBySlot.ContainsKey(closestSlotIndex))
                continue;

            occupiedFillingsBySlot.Add(closestSlotIndex, existingFilling);
        }

        return occupiedFillingsBySlot;
    }

    private void SetFillingToSlot(DraggableFilling filling, Vector3 slotPosition)
    {
        filling.transform.SetParent(_ingredientsRoot, true);
        filling.transform.position = slotPosition;
        filling.SetSlotPosition(slotPosition);
    }

    private HashSet<int> GetOccupiedSlotIndices(IReadOnlyList<Vector3> slotPositions, DraggableFilling fillingToIgnore)
    {
        var occupiedSlotIndices = new HashSet<int>();

        foreach (var existingFilling in _fillings)
        {
            if (existingFilling == null || existingFilling == fillingToIgnore)
                continue;

            var closestSlotIndex = GetClosestSlotIndex(slotPositions, existingFilling.GetSlotPosition());
            if (closestSlotIndex >= 0)
                occupiedSlotIndices.Add(closestSlotIndex);
        }

        return occupiedSlotIndices;
    }

    private int GetClosestAvailableSlotIndex(IReadOnlyList<Vector3> slotPositions, HashSet<int> occupiedSlotIndices, Vector3 fillingPosition)
    {
        var closestAvailableSlotIndex = -1;
        var closestDistance = float.MaxValue;

        for (var slotIndex = 0; slotIndex < slotPositions.Count; slotIndex++)
        {
            if (occupiedSlotIndices.Contains(slotIndex))
                continue;

            var distance = (fillingPosition - slotPositions[slotIndex]).sqrMagnitude;
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestAvailableSlotIndex = slotIndex;
        }

        return closestAvailableSlotIndex;
    }

    private int GetClosestSlotIndex(IReadOnlyList<Vector3> slotPositions, Vector3 fillingPosition)
    {
        var closestSlotIndex = -1;
        var closestDistance = float.MaxValue;

        for (var slotIndex = 0; slotIndex < slotPositions.Count; slotIndex++)
        {
            var distance = ((Vector2)fillingPosition - (Vector2)slotPositions[slotIndex]).sqrMagnitude;
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestSlotIndex = slotIndex;
        }

        return closestSlotIndex;
    }

    private IReadOnlyList<Filling> BuildFillingSlots()
    {
        var slotPositions = GetSlotPositions(0f).ToList();
        var fillingSlots = new Filling[slotPositions.Count];

        foreach (var filling in _fillings)
        {
            if (filling == null)
                continue;

            var slotIndex = GetClosestSlotIndex(slotPositions, filling.GetSlotPosition());
            if (slotIndex < 0 || slotIndex >= fillingSlots.Length)
                continue;

            fillingSlots[slotIndex] = filling.Ingredient;
        }

        return fillingSlots;
    }

    private IEnumerable<Vector3> GetSlotPositions(float zPosition)
    {
        var maxFillings = Mathf.Max(1, _recipeGeneratorSettings.MaxFillingsInclusive);
        var columns = Mathf.CeilToInt(Mathf.Sqrt(maxFillings));
        var rows = Mathf.CeilToInt(maxFillings / (float)columns);

        var size = _ingredientsArea.bounds.size;
        var min = _ingredientsArea.bounds.min;
        var cellWidth = size.x / columns;
        var cellHeight = size.y / rows;

        for (var index = 0; index < maxFillings; index++)
        {
            var row = index / columns;
            var column = index % columns;

            yield return new Vector3(
                min.x + (column + 0.5f) * cellWidth,
                min.y + (row + 0.5f) * cellHeight,
                zPosition);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanStartCloseDrag(eventData))
            return;

        StartCloseDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isCloseDragActive || eventData.pointerId != _activeCloseDragPointerId)
            return;

        UpdateCloseDragProgress(GetCloseDragProgress(eventData));

        if (_closeDragProgressNormalized >= 1f)
            TryCompleteCloseDrag();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isCloseDragActive || eventData.pointerId != _activeCloseDragPointerId)
            return;

        UpdateCloseDragProgress(GetCloseDragProgress(eventData));
        TryCompleteCloseDrag();
    }
}
