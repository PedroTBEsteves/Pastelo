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
    
    protected sealed override void OnDrop(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPointy(eventData.position);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));
        
        if (!raycastHit 
            || !raycastHit.collider.TryGetComponent<OpenPastelDoughArea>(out var openPastelDoughArea)
            || !_money.CanSpend(Ingredient.Cost))
        {
            Destroy(gameObject);
            return;
        }

        if (TryAddToOpenDough(openPastelDoughArea))
        {
            Debug.Log("opa");
            _money.TrySpend(Ingredient.Cost);
        }
    }
    
    protected abstract bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea);
}
