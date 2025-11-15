public class Order
{
    private readonly float _timeLimit;
    private float _elapsedTime;
    
    public Order(int number, Customer customer, Recipe recipe, float timeLimit)
    {
        Number = number;
        Customer = customer;
        Recipe = recipe;
        _timeLimit = timeLimit;
    }
    
    public int Number { get; }
    public Customer Customer { get; }
    public Recipe Recipe { get; }
    public float RemainingTime => _timeLimit - _elapsedTime;
    public float NormalizedRemainingTime => RemainingTime / _timeLimit;

    public void Tick(float deltaTime)
    {
        _elapsedTime += deltaTime;
    }

    public bool IsExpired() => _elapsedTime >= _timeLimit;
    
    public override string ToString() => $"{Recipe} para {Customer.name}";
}
