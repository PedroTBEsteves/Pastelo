using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PastelCookingSettings", menuName = "Scriptable Objects/PastelCookingSettings")]
public class PastelCookingSettings : ScriptableObject
{
    [field: SerializeField]
    public float TimeToIncreaseFriedLevelInSeconds { get; private set; }

    [SerializeField]
    private PastelComboPattern[] _comboPatterns = Array.Empty<PastelComboPattern>();

    public IReadOnlyList<PastelComboPattern> ComboPatterns => _comboPatterns;
}

[Serializable]
public class PastelComboPattern
{
    [field: SerializeField]
    public float Value { get; private set; }

    [SerializeField]
    private PastelGenericIngredientType[] _slotTypes = Array.Empty<PastelGenericIngredientType>();

    public IReadOnlyList<PastelGenericIngredientType> SlotTypes => _slotTypes;
}

public enum PastelGenericIngredientType
{
    None = 0,
    Type1 = 1,
    Type2 = 2,
    Type3 = 3
}
