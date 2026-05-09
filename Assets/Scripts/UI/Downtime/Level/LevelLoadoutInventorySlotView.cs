using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DraggableUI))]
public class LevelLoadoutInventorySlotView : MonoBehaviour
{
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _quantityText;

    [SerializeField]
    private GameObject _quantityRoot;

    [SerializeField]
    private Color _defaultIconColor = Color.white;

    [SerializeField]
    private Color _depletedIconColor = Color.gray;

    [SerializeField]
    private Color _cantAddIconColor = Color.red;

    private LevelLoadoutEditorView _editor;
    private LoadoutInventoryProjectionEntry _entry;
    private DraggableUI _draggableUI;
    private bool _hasPendingPreview;

    private void Awake()
    {
        _draggableUI = GetComponent<DraggableUI>();
        if (_draggableUI == null)
        {
            Debug.LogError($"{nameof(LevelLoadoutInventorySlotView)} on '{name}' requires {nameof(DraggableUI)}.", this);
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

    public void Bind(LevelLoadoutEditorView editor, LoadoutInventoryProjectionEntry entry)
    {
        _editor = editor;
        _entry = entry;
        _hasPendingPreview = false;
        _draggableUI?.Configure(editor != null ? editor.InputConfiguration : null);
        Refresh();
    }

    public void CancelDragInput()
    {
        _draggableUI?.CancelDrag();
    }

    private bool CanBeginDrag()
    {
        return _entry.Ingredient != null && _entry.AvailableQuantity > 0 && (_editor == null || _editor.CanAddIngredient(_entry.Ingredient));
    }

    private void OnHeld(PointerEventData eventData)
    {
        _editor?.HandleInventoryHeld(this, _entry.Ingredient, _entry.AvailableQuantity, eventData);
    }

    private void OnDragged(Vector2 screenPosition)
    {
        _editor?.HandleDragInput(screenPosition);
    }

    private void OnDropped(PointerEventData eventData)
    {
        _editor?.HandleEndDragInput(eventData);
    }

    public void BeginPendingPreview()
    {
        if (_hasPendingPreview)
            return;

        _hasPendingPreview = true;
        Refresh();
    }

    public void CancelPendingPreview()
    {
        if (!_hasPendingPreview)
            return;

        _hasPendingPreview = false;
        Refresh();
    }

    public void ConfirmPendingPreview()
    {
        if (!_hasPendingPreview)
            return;

        _hasPendingPreview = false;
        Refresh();
    }

    private void Refresh()
    {
        var displayedQuantity = GetDisplayedQuantity();
        var canAddIngredient = _editor == null || _editor.CanAddIngredient(_entry.Ingredient);

        if (_iconImage != null)
        {
            var color = displayedQuantity <= 0 ? 
                _depletedIconColor : 
                !canAddIngredient ? 
                    _cantAddIconColor : 
                    _defaultIconColor;
            _iconImage.sprite = _entry.Ingredient != null ? _entry.Ingredient.Icon : null;
            _iconImage.color = color;
        }

        if (_nameText != null)
            _nameText.SetText(_entry.Ingredient != null ? _entry.Ingredient.GetDisplayName() : string.Empty);

        if (_quantityRoot != null)
            _quantityRoot.SetActive(_entry.Ingredient != null);

        if (_quantityText != null)
            _quantityText.SetText(_entry.Ingredient != null ? displayedQuantity.ToString() : string.Empty);
    }

    private int GetDisplayedQuantity()
    {
        if (_entry.Ingredient == null)
            return 0;

        return Mathf.Max(0, _entry.AvailableQuantity - (_hasPendingPreview ? 1 : 0));
    }
}
