public readonly struct LoadoutMissingIngredientEntry
{
    public LoadoutMissingIngredientEntry(Ingredient ingredient, bool isMissing)
    {
        Ingredient = ingredient;
        IsMissing = isMissing;
    }

    public Ingredient Ingredient { get; }
    public bool IsMissing { get; }
}
