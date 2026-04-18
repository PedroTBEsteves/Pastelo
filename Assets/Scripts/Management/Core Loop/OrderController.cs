using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderController : ITickable
{
    private const float TutorialFreezeThresholdNormalized = 0.10f;

    private readonly float _orderCompletionTimeLimit;
    private readonly RecipeGenerator _recipeGenerator;
    private readonly LevelMoneyManager _levelMoneyManager;
    private readonly PastelCookingSettings _pastelCookingSettings;
    private readonly ICustomerPopUpDialogue _customerPopUpDialogues;
    private readonly GameplayTutorialState _tutorialState;
    private readonly List<Order> _activeOrders = new();
    private readonly List<Order> _expiredOrders = new();

    private int _currentOrderNumber;

    public OrderController(
        OrderLoopSettings orderLoopSettings,
        RecipeGenerator recipeGenerator,
        LevelMoneyManager levelMoneyManager,
        PastelCookingSettings pastelCookingSettings,
        ICustomerPopUpDialogue customerPopUpDialogues,
        GameplayTutorialState tutorialState)
    {
        _recipeGenerator = recipeGenerator;
        _levelMoneyManager = levelMoneyManager;
        _pastelCookingSettings = pastelCookingSettings;
        _customerPopUpDialogues = customerPopUpDialogues;
        _tutorialState = tutorialState;
        _orderCompletionTimeLimit = orderLoopSettings.OrderCompletionTimeLimit;
    }

    public event Action<Order> OrderAccepted = delegate { };
    public event Action<Order> OrderStarted = delegate { };
    public event Action<Order> OrderExpired = delegate { };
    public event Action<Order> OrderFailed = delegate { };
    public event Action<Order> OrderHadMissingIngredients = delegate { };
    public event Action<Order> OrderSucceeded = delegate { };
    public event Action<Order> OrderFlowFinished = delegate { };

    public Order AcceptOrder(Customer customer)
    {
        var generationResult = _recipeGenerator.Generate();
        _currentOrderNumber++;
        var order = new Order(
            _currentOrderNumber,
            customer,
            generationResult.Recipe,
            _orderCompletionTimeLimit,
            generationResult.FailedBecauseOfMissingLoadoutIngredients,
            generationResult.MissingIngredients);
        OrderAccepted(order);
        Debug.Log($"{order} aceita!");

        return order;
    }

    public void StartOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.HadMissingIngredients)
            throw new InvalidOperationException("Cannot start an order that failed recipe generation.");

        _activeOrders.Add(order);
        OrderStarted(order);
        Debug.Log($"{order} começou!");
    }

    public void DeliverOrder(Order order, Delivery delivery)
    {
        _activeOrders.Remove(order);

        if (delivery.IsCorrectFor(order))
        {
            OrderSucceeded(order);
            _levelMoneyManager.Gain(order.GetValue());
        }
        else
        {
            OrderFailed(order);
        }
    }

    public void Tick(float deltaTime)
    {
        foreach (var order in _activeOrders)
        {
            if (ShouldFreezeTutorialOrder(order))
                continue;

            order.Tick(deltaTime);
            if (order.IsExpired())
                _expiredOrders.Add(order);
        }

        foreach (var order in _expiredOrders)
        {
            _activeOrders.Remove(order);
            OrderExpired(order);
            Debug.Log($"{order} expirou!");
            _customerPopUpDialogues.CustomerOrderExpiredDialogue(order.Customer)
                .ChainCallback(() =>
                {
                    OrderFlowFinished(order);
                });
        }

        _expiredOrders.Clear();
    }

    public void FailOrderFromMissingIngredients(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (!order.HadMissingIngredients)
            throw new InvalidOperationException("Only orders missing loadout ingredients can use this flow.");

        OrderHadMissingIngredients(order);
        OrderFlowFinished(order);
    }

    public void FinishOrderFlow(Order order)
    {
        OrderFlowFinished(order);
    }

    private bool ShouldFreezeTutorialOrder(Order order)
    {
        return _tutorialState.IsActive
            && order == _tutorialState.TutorialOrder
            && order.NormalizedRemainingTime <= TutorialFreezeThresholdNormalized;
    }
}
