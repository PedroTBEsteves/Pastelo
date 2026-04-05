using System.Collections.Generic;
using System.Linq;

public static class PastelComboPricer
{
    private const int MaxDistinctIngredientTypes = 3;

    public static float GetValue(Pastel pastel, PastelCookingSettings settings)
    {
        if (pastel == null)
            return 0f;

        return TryGetMatchingComboPattern(pastel.FillingSlots, settings, out var comboPattern)
            ? comboPattern.Value
            : 0f;
    }

    public static bool HasMatchingCombo(IReadOnlyList<Filling> fillingSlots, PastelCookingSettings settings)
    {
        return TryGetMatchingComboPattern(fillingSlots, settings, out _);
    }

    private static bool TryGetMatchingComboPattern(
        IReadOnlyList<Filling> fillingSlots,
        PastelCookingSettings settings,
        out PastelComboPattern matchingComboPattern)
    {
        matchingComboPattern = null;

        if (settings == null || fillingSlots == null)
            return false;

        var ingredientStats = BuildIngredientStats(fillingSlots);
        if (ingredientStats.Count > MaxDistinctIngredientTypes)
            return false;

        var genericTypesByIngredient = AssignGenericTypes(ingredientStats);
        foreach (var comboPattern in settings.ComboPatterns)
        {
            if (comboPattern == null)
                continue;

            var slotTypes = comboPattern.SlotTypes;
            if (slotTypes == null || slotTypes.Count != fillingSlots.Count)
                continue;

            if (!MatchesPattern(fillingSlots, slotTypes, genericTypesByIngredient))
                continue;

            matchingComboPattern = comboPattern;
            return true;
        }

        return false;
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
