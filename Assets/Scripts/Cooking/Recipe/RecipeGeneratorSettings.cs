using UnityEngine;

[CreateAssetMenu(fileName = "RecipeGeneratorSettings", menuName = "Scriptable Objects/RecipeSettings")]
public class RecipeGeneratorSettings : ScriptableObject
{
    [field: SerializeField]
    public int MinFillings { get; private set; }
    
    [field: SerializeField]
    public int MaxFillingsInclusive { get; private set; }
    
    [field: SerializeField, Range(0f, 1f)]
    public float NoMainProbability { get; private set; } = 0.1f;

    [field: SerializeField, Range(0f, 1f)]
    public float MaxMainPercentage { get; private set; } = 0.3f;
}
