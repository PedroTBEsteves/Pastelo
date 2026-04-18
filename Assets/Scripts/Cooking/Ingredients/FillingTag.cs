using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "IngredientTag", menuName = "Scriptable Objects/IngredientTag")]
public class FillingTag : ScriptableObject
{
    [field: SerializeField]
    public LocalizedString Name { get; private set; }
}
