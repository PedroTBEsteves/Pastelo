using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableIngredient<TIngredient> : Draggable where TIngredient : Ingredient
{
    [SerializeField]
    public TIngredient Ingredient;

    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly Money _money;
    
    [Inject]
    private readonly IPopupTextService _popupTextService;
    
    protected sealed override void OnDrop(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPointy(eventData.position);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));
        
        if (!raycastHit 
            || !raycastHit.collider.TryGetComponent<OpenPastelDoughArea>(out var openPastelDoughArea))
        {
            Destroy(gameObject);
            return;
        }

        if (!_money.CanSpend(Ingredient.Cost))
        {
            Destroy(gameObject);
            _popupTextService.ShowError("Sem dinheiro suficiente!", transform.position);
            return;
        }

        if (TryAddToOpenDough(openPastelDoughArea))
        {
            _money.TrySpend(Ingredient.Cost);
        }
    }
    
    protected abstract bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea);
}
