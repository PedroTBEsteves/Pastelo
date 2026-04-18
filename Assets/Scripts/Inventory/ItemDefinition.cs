using UnityEngine;
using UnityEngine.Localization;

public abstract class ItemDefinition : ScriptableObject
{
    [field: SerializeField, Header("Info")]
    public LocalizedString Name { get; private set; }
    
    [field: SerializeField]
    public LocalizedString Description { get; private set; }
    
    [field: SerializeField]
    public Sprite Icon { get; private set; }
    
    [field: SerializeField, Header("Store")]
    public float BuyPrice { get; private set; }
    
    [field: SerializeField]
    public float SellPrice { get; private set; }
    
    [field: SerializeField, Header("Inventory")]
    public int MaxStack { get; private set; }

    public virtual string GetDisplayName() => Name.GetLocalizedString();
}
