using UnityEngine;

public abstract class Ingredient : ItemDefinition
{
    [field: SerializeField]
    public string InternalName { get; private set; }

    [field: SerializeField]
    public string LocalizationKey { get; private set; }
    
    [field: SerializeField]
    public float Value { get; private set; }

    [field: SerializeField]
    public Sprite OrderIcon { get; private set; }
    
    [field: SerializeField]
    public Sprite SourceSprite { get; private set; }
    
    public abstract string GetName();

    public override string GetDisplayName() => GetName();
}
