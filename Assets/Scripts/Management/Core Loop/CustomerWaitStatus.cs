using UnityEngine;

public class CustomerWaitStatus
{
    private readonly float _waitTime;
    private float _elapsedTime;

    public CustomerWaitStatus(Customer customer, float waitTime)
    {
        Customer = customer;
        _waitTime = waitTime;
    }

    public Customer Customer { get; }

    public void Tick(float deltaTime)
    {
        _elapsedTime += deltaTime;
    }
    
    public bool IsExpired() => _elapsedTime >= _waitTime;
}
