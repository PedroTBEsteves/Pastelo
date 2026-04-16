using UnityEngine;

[CreateAssetMenu(fileName = "RecipeGeneratorSettings", menuName = "Scriptable Objects/RecipeSettings")]
public class RecipeGeneratorSettings : ScriptableObject
{
    [field: SerializeField]
    public int MinFillings { get; private set; }
    
    [field: SerializeField]
    public int MaxFillingsInclusive { get; private set; }
}
