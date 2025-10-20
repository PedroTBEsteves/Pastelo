using UnityEngine;

public abstract class Ingredient : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field:  SerializeField]
    public float Cost { get; private set; }
    
    [field: SerializeField]
    public float Value { get; private set; }

    [field: SerializeField]
    public Sprite Icon { get; private set; }
}
