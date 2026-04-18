using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Scriptable Objects/Level", fileName = "Level")]
public class Level : ScriptableObject
{
    [field: SerializeField]
    public LocalizedString Name { get; private set; }

    [field: SerializeField]
    public LocalizedString Description { get; private set; }

    [SerializeField]
    private Customer[] _customers;
    
    [field: SerializeField]
    public int CustomersToServe { get; private set; }
    
    [field: SerializeField]
    public float PriceToPlay { get; private set; }

    public IEnumerable<Customer> Customers => _customers;
}
