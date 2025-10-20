using UnityEngine;

[CreateAssetMenu(fileName = "Customer", menuName = "Scriptable Objects/Customer")]
public class Customer : ScriptableObject
{
    [field: SerializeField]
    public Sprite Sprite {get; private set;}
}
