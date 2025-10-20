using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "Dough", menuName = "Scriptable Objects/Dough")]
public class Dough : Ingredient
{
    [field: SerializeField]
    public Sprite OpenDoughSprite { get; private set; }
    
    [SerializeField]
    private SerializedDictionary<FriedLevel, Sprite> _closedPastelDoughSprites;

    public Sprite GetClosedDoughSprite(FriedLevel level) => _closedPastelDoughSprites[level];
}
