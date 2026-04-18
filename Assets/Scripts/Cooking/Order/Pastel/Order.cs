using System;
using System.Collections.Generic;

public class Order
{
    private readonly float _timeLimit;
    private float _elapsedTime;

    public Order(
        int number,
        Customer customer,
        Recipe recipe,
        float timeLimit,
        bool hadMissingIngredients = false,
        IReadOnlyList<Ingredient> missingIngredients = null)
    {
        Number = number;
        Customer = customer;
        Recipe = recipe;
        _timeLimit = timeLimit;
        HadMissingIngredients = hadMissingIngredients;
        MissingIngredients = missingIngredients ?? Array.Empty<Ingredient>();
    }

    public int Number { get; }
    public Customer Customer { get; }
    public Recipe Recipe { get; }
    public bool HadMissingIngredients { get; }
    public IReadOnlyList<Ingredient> MissingIngredients { get; }
    public float RemainingTime => _timeLimit - _elapsedTime;
    public float NormalizedRemainingTime => RemainingTime / _timeLimit;

    public void Tick(float deltaTime)
    {
        _elapsedTime += deltaTime;
    }

    public bool IsExpired() => _elapsedTime >= _timeLimit;

    public float GetValue() => Recipe?.Value ?? 0f;

    public override string ToString()
    {
        var orderDescription = HadMissingIngredients ? "pedido invalido" : Recipe?.ToString() ?? "pedido sem receita";
        return $"{orderDescription} para {Customer.name}";
    }
}
