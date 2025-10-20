using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderController : ITickable
{
    private readonly float _orderCompletionTimeLimit;
    private readonly RecipeGenerator _recipeGenerator;
    private readonly StrikesController _strikesController;
    private readonly Money _money;
    private readonly ICustomerPopUpDialogue _customerPopUpDialogues;
    private readonly List<Order> _activeOrders = new();
    private readonly List<Order> _expiredOrders = new();

    private int _currentOrderNumber;

    public OrderController(OrderLoopSettings orderLoopSettings, RecipeGenerator recipeGenerator, StrikesController strikesController, Money money, ICustomerPopUpDialogue customerPopUpDialogues)
    {
        _recipeGenerator = recipeGenerator;
        _strikesController = strikesController;
        _money = money;
        _customerPopUpDialogues = customerPopUpDialogues;
        _orderCompletionTimeLimit = orderLoopSettings.OrderCompletionTimeLimit;
    }
    
    public event Action<Order> OrderAccepted = delegate { };
    public event Action<Order> OrderStarted = delegate { };
    public event Action<Order> OrderExpired = delegate { };
    public event Action<Order> OrderFailed = delegate { };
    public event Action<Order> OrderSucceeded = delegate { };

    public Order AcceptOrder(Customer customer)
    {
        var recipe = _recipeGenerator.Generate();
        _currentOrderNumber++;
        var order = new Order(_currentOrderNumber, customer, recipe, _orderCompletionTimeLimit);
        OrderAccepted(order);
        Debug.Log($"{order} aceita!");

        return order;
    }

    public void StartOrder(Order order)
    {
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
        }
        else
        {
            OrderFailed(order);
        }
        
        _money.Gain(order.Recipe.Value);
    }
    
    public void Tick(float deltaTime)
    {
        foreach (var order in _activeOrders)
        {
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
                .ChainCallback(_strikesController.Strike);
        }
        
        _expiredOrders.Clear();
    }
}
