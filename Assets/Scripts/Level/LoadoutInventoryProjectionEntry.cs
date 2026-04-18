public readonly struct LoadoutInventoryProjectionEntry
{
    public LoadoutInventoryProjectionEntry(Ingredient ingredient, int availableQuantity)
    {
        Ingredient = ingredient;
        AvailableQuantity = availableQuantity;
    }

    public Ingredient Ingredient { get; }
    public int AvailableQuantity { get; }
}
