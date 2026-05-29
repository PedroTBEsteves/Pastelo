using System;
using System.Collections.Generic;
using System.Linq;
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
        var loadout = _levelLoadoutController.GetLoadout(level);
        
        return loadout.DoughCount > 0 &&
               loadout.FillingCount > 0 && 
               _levelLoadoutController.CanConsumeLoadout(level) && 
               _moneyManager.CanSpend(level.PriceToPlay);
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

    public async UniTask<bool> StartConfiguredLevel(Level level, IEnumerable<Dough> doughs, IEnumerable<Filling> fillings)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        if (doughs == null)
            throw new ArgumentNullException(nameof(doughs));

        if (fillings == null)
            throw new ArgumentNullException(nameof(fillings));

        var configuredDoughs = doughs.Where(dough => dough != null).Distinct().ToArray();
        var configuredFillings = fillings.Where(filling => filling != null).Distinct().ToArray();

        if (configuredDoughs.Length == 0)
            throw new InvalidOperationException($"{nameof(StartConfiguredLevel)} requires at least one configured {nameof(Dough)}.");

        if (configuredFillings.Length == 0)
            throw new InvalidOperationException($"{nameof(StartConfiguredLevel)} requires at least one configured {nameof(Filling)}.");

        _levelLoadoutController.ReplaceLoadout(level, configuredDoughs, configuredFillings);

        SelectedLevel = level;
        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            SelectedLevel = null;
            return false;
        }

        LevelStarted(level);
        return true;
    }

    public void ClearSelectedLevel()
    {
        SelectedLevel = null;
    }
    
    public Loadout GetSelectedLevelLoadout() => _levelLoadoutController.GetLoadout(SelectedLevel);
}
