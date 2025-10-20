using UnityEngine;

public class CustomersDatabase
{
    private readonly Customer[] _customers;

    public CustomersDatabase()
    {
        _customers = Resources.LoadAll<Customer>("Customers");
    }

    public Customer GetRandom()
    {
        var size = _customers.Length;
        var index = Random.Range(0, size);
        return _customers[index];
    }
}
