using System;
using UnityEngine;

public class LevelFlowController : ITickable, IDisposable
{
    private readonly GameplayTutorialState _tutorialState;
    private readonly CustomerQueue _customerQueue;
    private readonly OrderController _orderController;
    private readonly StrikesController _strikesController;
    private readonly float _levelDurationSeconds;

    private float _elapsedTimeSeconds;
    private int _pendingCustomerFlows;
    private bool _hasTimeExpired;

    public LevelFlowController(
        LevelSelector levelSelector,
        GameplayTutorialState tutorialState,
        CustomerQueue customerQueue,
        OrderController orderController,
        StrikesController strikesController)
    {
        if (levelSelector == null)
            throw new ArgumentNullException(nameof(levelSelector));

        var selectedLevel = levelSelector.SelectedLevel;
        if (selectedLevel == null)
            throw new InvalidOperationException($"{nameof(LevelFlowController)} requires a selected {nameof(Level)}.");

        _tutorialState = tutorialState ?? throw new ArgumentNullException(nameof(tutorialState));
        _customerQueue = customerQueue ?? throw new ArgumentNullException(nameof(customerQueue));
        _orderController = orderController ?? throw new ArgumentNullException(nameof(orderController));
        _strikesController = strikesController ?? throw new ArgumentNullException(nameof(strikesController));

        _levelDurationSeconds = Mathf.Max(0f, selectedLevel.LevelDurationSeconds);

        _customerQueue.QueueEntryAdded += OnQueueEntryAdded;
        _customerQueue.CustomerFlowFinished += OnCustomerFlowFinished;
        _orderController.OrderFlowFinished += OnOrderFlowFinished;
        _strikesController.GameOver += OnGameOver;
    }

    public bool IsLevelEnded { get; private set; }
    public bool HasTimeExpired => _hasTimeExpired;
    public float ElapsedTimeSeconds => Mathf.Min(_elapsedTimeSeconds, _levelDurationSeconds);
    public float RemainingTimeSeconds => Mathf.Max(0f, _levelDurationSeconds - _elapsedTimeSeconds);

    public event Action LevelEnded = delegate { };
    public event Action<float> LevelTimeChanged = delegate { };
    public event Action LevelTimeExpired = delegate { };

    public void Tick(float deltaTime)
    {
        if (IsLevelEnded || _hasTimeExpired || _tutorialState.IsActive)
            return;

        _elapsedTimeSeconds += deltaTime;
        LevelTimeChanged(_elapsedTimeSeconds / _levelDurationSeconds);

        if (_elapsedTimeSeconds < _levelDurationSeconds)
            return;

        _elapsedTimeSeconds = _levelDurationSeconds;
        _hasTimeExpired = true;
        _customerQueue.StopArrivals();
        LevelTimeExpired();

        if (_pendingCustomerFlows == 0)
            EndLevel();
    }

    public void Dispose()
    {
        _customerQueue.QueueEntryAdded -= OnQueueEntryAdded;
        _customerQueue.CustomerFlowFinished -= OnCustomerFlowFinished;
        _orderController.OrderFlowFinished -= OnOrderFlowFinished;
        _strikesController.GameOver -= OnGameOver;
    }

    private void EndLevel()
    {
        if (IsLevelEnded)
            return;

        IsLevelEnded = true;
        LevelEnded();
    }

    private void OnQueueEntryAdded(CustomerWaitStatus _)
    {
        if (IsLevelEnded)
            throw new InvalidOperationException("Cannot register a customer flow after the level has ended.");

        _pendingCustomerFlows++;
    }

    private void OnCustomerFlowFinished(Customer _)
    {
        FinishCustomerFlow();
    }

    private void OnOrderFlowFinished(Order _)
    {
        FinishCustomerFlow();
    }

    private void FinishCustomerFlow()
    {
        if (_pendingCustomerFlows <= 0)
            throw new InvalidOperationException("Cannot finish a customer flow when none are pending.");

        _pendingCustomerFlows--;

        if (_hasTimeExpired && _pendingCustomerFlows == 0)
            EndLevel();
    }

    private void OnGameOver()
    {
        _customerQueue.StopArrivals();
        EndLevel();
    }
}
