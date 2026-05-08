using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DraggableUI))]
public class LevelLoadoutIngredientView : MonoBehaviour
{
    [SerializeField]
    private LevelLoadoutIngredientSlotType _slotType;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private Color _defaultIconColor = Color.white;

    [SerializeField]
    private Color _missingIconColor = Color.red;

    [SerializeField]
    private CanvasGroup _canvasGroup;

    private LevelLoadoutEditorView _editor;
    private Ingredient _ingredient;
    private DraggableUI _draggableUI;
    private bool _isMissing;
    private bool _isPreview;

    public LevelLoadoutIngredientSlotType SlotType => _slotType;
    public Ingredient Ingredient => _ingredient;
    public bool HasIngredient => _ingredient != null;
    public bool IsPreview => _isPreview;

    private void Awake()
    {
        _draggableUI = GetComponent<DraggableUI>();
        if (_draggableUI == null)
        {
            Debug.LogError($"{nameof(LevelLoadoutIngredientView)} on '{name}' requires {nameof(DraggableUI)}.", this);
            return;
        }

        _draggableUI.AddCanDragHandler(CanBeginDrag);
        _draggableUI.Held += OnHeld;
        _draggableUI.Dragged += OnDragged;
        _draggableUI.Dropped += OnDropped;
    }

    private void OnDestroy()
    {
        if (_draggableUI == null)
            return;

        _draggableUI.RemoveCanDragHandler(CanBeginDrag);
        _draggableUI.Held -= OnHeld;
        _draggableUI.Dragged -= OnDragged;
        _draggableUI.Dropped -= OnDropped;
    }

    public void Bind(LevelLoadoutEditorView editor, LevelLoadoutIngredientSlotType slotType, Ingredient ingredient, bool isMissing)
    {
        _editor = editor;
        _slotType = slotType;
        _ingredient = ingredient;
        _isMissing = isMissing;
        _isPreview = false;
        _draggableUI?.Configure(editor != null ? editor.InputConfiguration : null);
        Refresh();
    }

    public void BindPreview(LevelLoadoutEditorView editor, LevelLoadoutIngredientSlotType slotType, Ingredient ingredient)
    {
        _editor = editor;
        _slotType = slotType;
        _ingredient = ingredient;
        _isMissing = false;
        _isPreview = true;
        _draggableUI?.Configure(editor != null ? editor.InputConfiguration : null);
        Refresh();
    }

    public bool Accepts(Ingredient ingredient)
    {
        return ingredient != null && ingredient switch
        {
            Dough when _slotType == LevelLoadoutIngredientSlotType.Dough => true,
            Filling when _slotType == LevelLoadoutIngredientSlotType.Filling => true,
            _ => false
        };
    }

    public void BeginExternalDrag(PointerEventData eventData)
    {
        _draggableUI?.BeginExternalDrag(eventData);
    }

    private bool CanBeginDrag()
    {
        return _editor != null && _ingredient != null;
    }

    private void OnHeld(PointerEventData eventData)
    {
        if (_isPreview)
            return;

        _editor?.HandleLoadoutIngredientHeld(this, _ingredient, eventData);
    }

    private void OnDragged(Vector2 screenPosition)
    {
        _editor?.HandleDragInput(screenPosition);
    }

    private void OnDropped(PointerEventData eventData)
    {
        _editor?.HandleEndDragInput(eventData);
    }

    public void UpdateDraggedPosition(Vector2 screenPosition)
    {
        if (transform is RectTransform rectTransform)
            rectTransform.position = screenPosition;
        else
            transform.position = screenPosition;
    }

    public void SetDragState(bool isDragging, bool blockRaycasts = false)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = isDragging ? 0.5f : 1f;
        _canvasGroup.blocksRaycasts = !isDragging || blockRaycasts;
    }

    private void Refresh()
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = _ingredient != null ? _ingredient.Icon : null;
            _iconImage.color = _ingredient != null && _isMissing
                ? _missingIconColor
                : _defaultIconColor;
            _iconImage.enabled = _ingredient != null;
        }

        SetDragState(_isPreview);
    }
}
