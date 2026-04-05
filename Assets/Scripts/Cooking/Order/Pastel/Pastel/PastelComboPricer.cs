using System.Collections.Generic;
using System.Linq;

public static class PastelComboPricer
{
    private const int MaxDistinctIngredientTypes = 3;

    public static float GetValue(Pastel pastel, PastelCookingSettings settings)
    {
        if (pastel == null || settings == null || pastel.FillingSlots == null)
            return 0f;

        var ingredientStats = BuildIngredientStats(pastel.FillingSlots);
        if (ingredientStats.Count > MaxDistinctIngredientTypes)
            return 0f;

        var genericTypesByIngredient = AssignGenericTypes(ingredientStats);
        foreach (var comboPattern in settings.ComboPatterns)
        {
            if (comboPattern == null)
                continue;

            var slotTypes = comboPattern.SlotTypes;
            if (slotTypes == null || slotTypes.Count != pastel.FillingSlots.Count)
                continue;

            if (MatchesPattern(pastel.FillingSlots, slotTypes, genericTypesByIngredient))
                return comboPattern.Value;
        }

        return 0f;
    }

    private static Dictionary<Filling, IngredientStats> BuildIngredientStats(IReadOnlyList<Filling> fillingSlots)
    {
        var ingredientStats = new Dictionary<Filling, IngredientStats>();

        for (var slotIndex = 0; slotIndex < fillingSlots.Count; slotIndex++)
        {
            var filling = fillingSlots[slotIndex];
            if (filling == null)
                continue;

            if (!ingredientStats.TryGetValue(filling, out var stats))
            {
                stats = new IngredientStats(slotIndex);
                ingredientStats.Add(filling, stats);
            }

            stats.Count++;
        }

        return ingredientStats;
    }

    private static Dictionary<Filling, PastelGenericIngredientType> AssignGenericTypes(
        Dictionary<Filling, IngredientStats> ingredientStats)
    {
        var genericTypesByIngredient = new Dictionary<Filling, PastelGenericIngredientType>(ingredientStats.Count);
        var orderedIngredients = ingredientStats
            .OrderByDescending(pair => pair.Value.Count)
            .ThenBy(pair => pair.Value.FirstSlotIndex)
            .ToList();

        for (var index = 0; index < orderedIngredients.Count; index++)
            genericTypesByIngredient[orderedIngredients[index].Key] = (PastelGenericIngredientType)(index + 1);

        return genericTypesByIngredient;
    }

    private static bool MatchesPattern(
        IReadOnlyList<Filling> fillingSlots,
        IReadOnlyList<PastelGenericIngredientType> expectedSlotTypes,
        IReadOnlyDictionary<Filling, PastelGenericIngredientType> genericTypesByIngredient)
    {
        for (var slotIndex = 0; slotIndex < fillingSlots.Count; slotIndex++)
        {
            var filling = fillingSlots[slotIndex];
            var actualType = filling == null
                ? PastelGenericIngredientType.None
                : genericTypesByIngredient[filling];

            if (expectedSlotTypes[slotIndex] != actualType)
                return false;
        }

        return true;
    }

    private sealed class IngredientStats
    {
        public IngredientStats(int firstSlotIndex)
        {
            FirstSlotIndex = firstSlotIndex;
        }

        public int Count { get; set; }
        public int FirstSlotIndex { get; }
    }
}
