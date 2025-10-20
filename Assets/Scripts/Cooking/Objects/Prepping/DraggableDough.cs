public class DraggableDough : DraggableIngredient<Dough>
{
    protected override bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea)
    {
        Destroy(gameObject);
        
        return openPastelDoughArea.TryOpenDough(Ingredient);
    }
}
