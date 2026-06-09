using System;

public class LevelMoneyManager
{
    private readonly MoneyManager _moneyManager;
    private readonly LevelRunContext _runContext;
    private float _amount;
    private bool _hasTransferred;

    public LevelMoneyManager(
        MoneyManager moneyManager,
        LevelRunContext runContext,
        LevelFlowController levelFlowController,
        OrderController orderController)
    {
        _moneyManager = moneyManager;
        _runContext = runContext;
        levelFlowController.LevelEnded += TransferToMoneyManager;
        orderController.OrderSucceeded += GainOrderMoney;
    }
    
    public float Amount
    {
        get => _amount;
        private set
        {
            var previous = _amount;
            _amount = value;
            MoneyChanged(new MoneyChangedEvent(previous, value));
        }
    }

    public event Action<MoneyChangedEvent> MoneyChanged = delegate { };

    public void Gain(float amount)
    {
        Amount += amount;
    }

    private void TransferToMoneyManager()
    {
        if (_hasTransferred)
            return;

        _hasTransferred = true;

        if (_runContext.IsArcade)
            return;

        if (_amount <= 0f)
            return;

        _moneyManager.Gain(_amount);
    }
    
    private void GainOrderMoney(Order order) => Gain(order.GetValue());
}
