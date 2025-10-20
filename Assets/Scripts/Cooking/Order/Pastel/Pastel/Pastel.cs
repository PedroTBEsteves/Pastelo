using System.Collections.Generic;

public class Pastel
{
    private readonly Recipe _recipe;
    private readonly FriedLevel _friedLevel;

    public Pastel(Recipe recipe, FriedLevel friedLevel)
    {
        _recipe = recipe;
        _friedLevel = friedLevel;
    }

    public bool IsCorrectFor(Recipe recipe)
    {
        return _recipe.Equals(recipe) && _friedLevel == FriedLevel.Done;
    }
}
