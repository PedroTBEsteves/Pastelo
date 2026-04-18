using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientsStorageSettings", menuName = "Scriptable Objects/IngredientsStorageSettings")]
public class IngredientsStorageSettings : ScriptableObject
{
    [SerializeField]
    private Dough[] _startingDoughs;
    
    [SerializeField]
    private Filling[] _startingFillings;
    
    [SerializeField]
    private Ingredient[] _ingredients;
    
    [SerializeField]
    private float[]  _prices;
    
    public IEnumerable<Dough> StartingDoughs => _startingDoughs;
    public IEnumerable<Filling> StartingFillings => _startingFillings;
    public IEnumerable<Ingredient> Ingredients => _ingredients;
    public IReadOnlyList<float> Prices => _prices;
}
