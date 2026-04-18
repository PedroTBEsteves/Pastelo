using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelLoadoutIngredientView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    private bool _isMissing;
    private bool _isPreview;

    public LevelLoadoutIngredientSlotType SlotType => _slotType;
    public Ingredient Ingredient => _ingredient;
    public bool HasIngredient => _ingredient != null;
    public bool IsPreview => _isPreview;

    public void Bind(LevelLoadoutEditorView editor, LevelLoadoutIngredientSlotType slotType, Ingredient ingredient, bool isMissing)
    {
        _editor = editor;
        _slotType = slotType;
        _ingredient = ingredient;
        _isMissing = isMissing;
        _isPreview = false;
        Refresh();
    }

    public void BindPreview(LevelLoadoutEditorView editor, LevelLoadoutIngredientSlotType slotType, Ingredient ingredient)
    {
        _editor = editor;
        _slotType = slotType;
        _ingredient = ingredient;
        _isMissing = false;
        _isPreview = true;
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_editor == null || _ingredient == null)
            return;

        if (_isPreview)
            return;

        _editor.BeginSlotDrag(this, _ingredient);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _editor?.HandleDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _editor?.EndDrag(eventData);
    }

    public void UpdateDraggedPosition(Vector2 screenPosition)
    {
        if (transform is RectTransform rectTransform)
            rectTransform.position = screenPosition;
        else
            transform.position = screenPosition;
    }

    public void SetDragState(bool isDragging)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = isDragging ? 0.5f : 1f;
        _canvasGroup.blocksRaycasts = !isDragging;
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
