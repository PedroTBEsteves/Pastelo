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
        _tutorialEvents.PublishFillingAdded(filling.Ingredient);
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
        var availableSlotPositions = GetSlotPositions(filling.transform.position.z)
            .Where(slotPosition => _fillings.All(existing => existing.transform.position != slotPosition))
            .ToList();

        if (availableSlotPositions.Count == 0)
            return;

        var closestSlotPosition = availableSlotPositions
            .OrderBy(slotPosition => (filling.transform.position - slotPosition).sqrMagnitude)
            .First();

        filling.transform.SetParent(transform, true);
        filling.transform.position = closestSlotPosition;
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
