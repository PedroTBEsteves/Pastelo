using System.Collections.Generic;

public class Pastel
{
    private readonly Recipe _recipe;
    private readonly IReadOnlyList<Filling> _fillingSlots;
    private readonly FriedLevel _friedLevel;

    public Pastel(Recipe recipe, IReadOnlyList<Filling> fillingSlots, FriedLevel friedLevel)
    {
        _recipe = recipe;
        _fillingSlots = fillingSlots;
        _friedLevel = friedLevel;
    }

    public IReadOnlyList<Filling> FillingSlots => _fillingSlots;

    public bool IsCorrectFor(Recipe recipe)
    {
        return _recipe.Equals(recipe) && _friedLevel == FriedLevel.Done;
    }
}
