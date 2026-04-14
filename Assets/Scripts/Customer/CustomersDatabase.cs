using System.Collections.Generic;
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

    public Customer GetRandomExcluding(HashSet<Customer> excludedCustomers)
    {
        if (excludedCustomers == null || excludedCustomers.Count == 0)
            return GetRandom();

        var availableCustomers = 0;

        foreach (var customer in _customers)
        {
            if (!excludedCustomers.Contains(customer))
                availableCustomers++;
        }

        if (availableCustomers == 0)
            return GetRandom();

        var targetIndex = Random.Range(0, availableCustomers);

        foreach (var customer in _customers)
        {
            if (excludedCustomers.Contains(customer))
                continue;

            if (targetIndex == 0)
                return customer;

            targetIndex--;
        }

        return GetRandom();
    }
}
