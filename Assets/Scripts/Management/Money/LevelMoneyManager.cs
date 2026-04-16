using System;

public class LevelMoneyManager
{
    private readonly MoneyManager _moneyManager;
    private float _amount;
    private bool _hasTransferred;

    public LevelMoneyManager(MoneyManager moneyManager, LevelFlowController levelFlowController)
    {
        _moneyManager = moneyManager;
        levelFlowController.LevelEnded += TransferToMoneyManager;
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

        if (_amount <= 0f)
            return;

        _moneyManager.Gain(_amount);
    }
}
