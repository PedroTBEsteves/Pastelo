using UnityEngine;

[CreateAssetMenu(fileName = "MoneySettings", menuName = "Scriptable Objects/MoneySettings")]
public class MoneySettings : ScriptableObject
{
    [field:  SerializeField]
    public float InitialAmount { get; private set; }
}
