using UnityEngine;

[CreateAssetMenu(fileName = "Filling", menuName = "Scriptable Objects/Filling")]
public class Filling : Ingredient
{
    public override string GetName() => Name.GetLocalizedString(new { amount = 1 });
}
