using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class CustomersDatabase
{
    private readonly IReadOnlyList<Customer> _customers;

    public CustomersDatabase(LevelSelector levelSelector)
    {
        if (levelSelector == null)
            throw new ArgumentNullException(nameof(levelSelector));

        var selectedLevel = levelSelector.SelectedLevel;
        if (selectedLevel == null)
            throw new InvalidOperationException($"{nameof(CustomersDatabase)} requires a selected {nameof(Level)}.");

        _customers = selectedLevel.Customers.ToList();
    }

    public Customer GetRandom()
    {
        var size = _customers.Count;
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
