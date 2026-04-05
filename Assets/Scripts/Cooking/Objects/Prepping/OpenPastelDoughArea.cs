using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenPastelDoughArea : ValidatedMonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private BoxCollider2D _ingredientsArea;

    [SerializeField]
    private float _pressDuration;

    [SerializeField]
    private DraggableClosedPastel _closedPastelPrefab;
    
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

    private Tween _pressTween;
    
    private List<DraggableFilling> _fillings = new List<DraggableFilling>();

    private TutorialTarget _tutorialTarget;
    
    public int FillingsCount => _fillings.Count;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.ClosePastelArea);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }
    
    public bool TryOpenDough(Dough dough)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.UseDough, dough))
            return false;

        if (_pastel != null)
            return false;
        
        _spriteRenderer.sprite = dough.OpenDoughSprite;
        _pastel = new OpenPastelDough(dough, _recipeGeneratorSettings.MaxFillingsInclusive);
        _tutorialEvents.PublishDoughOpened(dough);

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
        return true;
    }

    private void Close()
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.ClosePastel))
            return;

        if (_pastel == null)
            return;
        
        var closedPastel = _pastel.Close(_pastelCookingSettings);
        _pastel = null;
        _spriteRenderer.sprite = null;
        var draggableClosedPastel = Instantiate(_closedPastelPrefab, transform.position + Vector3.forward, Quaternion.identity, transform.parent);
        draggableClosedPastel.Initialize(closedPastel);
        foreach (var fillings in _fillings)
            Destroy(fillings.gameObject);
        _fillings.Clear();
        _tutorialEvents.PublishPastelClosed(draggableClosedPastel);
        //_cameraController.GoToNextSection();
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
        filling.transform.SetParent(transform, true);
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
            var distance = (fillingPosition - slotPositions[slotIndex]).sqrMagnitude;
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestSlotIndex = slotIndex;
        }

        return closestSlotIndex;
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
        _pressTween = Tween.Delay(_pressDuration, Close);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pressTween.isAlive)
            _pressTween.Stop();
    }
}
