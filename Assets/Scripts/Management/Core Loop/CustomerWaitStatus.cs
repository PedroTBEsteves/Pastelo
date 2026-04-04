using UnityEngine;

public class CustomerWaitStatus
{
    private static int _nextId = 1;
    private readonly float _waitTime;
    private float _elapsedTime;

    public CustomerWaitStatus(Customer customer, float waitTime)
    {
        Id = _nextId++;
        Customer = customer;
        _waitTime = waitTime;
    }

    public int Id { get; }
    public Customer Customer { get; }
    public float WaitTime => _waitTime;
    public float ElapsedTime => _elapsedTime;
    public float RemainingTime => Mathf.Max(0f, _waitTime - _elapsedTime);
    public float NormalizedRemaining => _waitTime <= 0f
        ? 0f
        : Mathf.Clamp01(RemainingTime / _waitTime);

    public void Tick(float deltaTime)
    {
        _elapsedTime += deltaTime;
    }
    
    public bool IsExpired() => _elapsedTime >= _waitTime;
}
