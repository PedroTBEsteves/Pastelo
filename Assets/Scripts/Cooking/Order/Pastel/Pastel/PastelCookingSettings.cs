using UnityEngine;

[CreateAssetMenu(fileName = "PastelCookingSettings", menuName = "Scriptable Objects/PastelCookingSettings")]
public class PastelCookingSettings : ScriptableObject
{
    [field: SerializeField]
    public float TimeToIncreaseFriedLevelInSeconds { get; private set; }
}
