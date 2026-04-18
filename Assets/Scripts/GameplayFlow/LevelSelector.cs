using System;
using Cysharp.Threading.Tasks;

public sealed class LevelSelector
{
    private readonly GameplayLoopFlowController _gameplayLoopFlowController;
    private readonly LevelLoadoutController _levelLoadoutController;
    private readonly MoneyManager _moneyManager;
    
    public LevelSelector(GameplayLoopFlowController gameplayLoopFlowController, LevelLoadoutController levelLoadoutController, MoneyManager moneyManager)
    {
        _gameplayLoopFlowController = gameplayLoopFlowController ?? throw new ArgumentNullException(nameof(gameplayLoopFlowController));
        _levelLoadoutController = levelLoadoutController ?? throw new ArgumentNullException(nameof(levelLoadoutController));
        _moneyManager = moneyManager;
    }

    public Level SelectedLevel { get; private set; }
    
    public event Action<Level> LevelStarted = delegate { };

    public bool CanPlayLevel(Level level)
    {
        return _levelLoadoutController.CanConsumeLoadout(level) && _moneyManager.CanSpend(level.PriceToPlay);
    }

    public async UniTask PlayLevel(Level level)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        if (!CanPlayLevel(level))
            return;

        SelectedLevel = level;
        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            SelectedLevel = null;
            return;
        }

        _levelLoadoutController.ConsumeLoadout(level);
        _moneyManager.TrySpend(level.PriceToPlay);
        LevelStarted(level);
    }

    public void ClearSelectedLevel()
    {
        SelectedLevel = null;
    }
    
    public Loadout GetSelectedLevelLoadout() => _levelLoadoutController.GetLoadout(SelectedLevel);
}
