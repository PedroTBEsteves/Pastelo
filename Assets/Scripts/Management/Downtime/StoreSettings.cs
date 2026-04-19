using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreSettings", menuName = "Scriptable Objects/StoreSettings")]
public class StoreSettings : ScriptableObject
{
    [SerializeField]
    private Ingredient[] _randomPool = Array.Empty<Ingredient>();

    [SerializeField]
    private FixedStoreIngredientEntry[] _fixedSequence = Array.Empty<FixedStoreIngredientEntry>();

    [field: SerializeField, Min(0)]
    public int RandomIngredientsCount { get; private set; } = 2;

    [field: SerializeField, Min(0)]
    public int RandomItemMinStock { get; private set; } = 1;

    [field: SerializeField, Min(0)]
    public int RandomItemMaxStock { get; private set; } = 3;

    [field: SerializeField, Min(0)]
    public int FixedIngredientsCount { get; private set; } = 4;

    public IReadOnlyList<Ingredient> RandomPool => _randomPool;
    public IReadOnlyList<FixedStoreIngredientEntry> FixedSequence => _fixedSequence;

    public int GetNormalizedRandomItemMinStock()
    {
        return Mathf.Max(0, RandomItemMinStock);
    }

    public int GetNormalizedRandomItemMaxStock()
    {
        return Mathf.Max(GetNormalizedRandomItemMinStock(), RandomItemMaxStock);
    }
}

[Serializable]
public struct FixedStoreIngredientEntry
{
    [SerializeField]
    private Ingredient _ingredient;

    [SerializeField, Min(1)]
    private int _durationInDays;

    public Ingredient Ingredient => _ingredient;
    public int DurationInDays => Mathf.Max(1, _durationInDays);
}
