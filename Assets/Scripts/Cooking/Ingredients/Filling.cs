using UnityEngine;

[CreateAssetMenu(fileName = "Filling", menuName = "Scriptable Objects/Filling")]
public class Filling : Ingredient
{
    [field: SerializeField]
    public string PluralName { get; private set; }
}
