using UnityEngine;

[CreateAssetMenu(fileName = "OrderLoopSettings", menuName = "Scriptable Objects/OrderLoopSettings")]
public class OrderLoopSettings : ScriptableObject
{
    [field: Header("Customers Queue")]
    [field: SerializeField]
    public float QueueWaitTimeLimit { get; private set; }
    
    [field: SerializeField]
    public float MinCustomerArrivalTime { get; private set; }
    
    [field: SerializeField]
    public float MaxCustomerArrivalTime { get; private set; }
    
    [field: Header("Customers Queue")]
    [field: SerializeField]
    public float OrderCompletionTimeLimit { get; private set; }
    
    [field: Header("Fail State")]
    [field: SerializeField]
    public int StrikesToFail { get; private set; }
}
