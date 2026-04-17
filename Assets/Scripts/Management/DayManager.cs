using System;
using Cysharp.Threading.Tasks;

public sealed class DayManager
{
    private readonly GameplayLoopFlowController _gameplayLoopFlowController;

    public DayManager(GameplayLoopFlowController gameplayLoopFlowController)
    {
        _gameplayLoopFlowController = gameplayLoopFlowController ?? throw new ArgumentNullException(nameof(gameplayLoopFlowController));
    }

    public int CurrentDay { get; private set; } = 1;

    public event Action DayFinished = delegate { };

    public void FinishDay()
    {
        CurrentDay++;
        DayFinished();
        _gameplayLoopFlowController.LoadDowntime().Forget();
    }
}
