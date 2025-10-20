using UnityEngine;

public abstract class Filling : Ingredient
{
    [field: SerializeField]
    public string PluralName { get; private set; }
}