using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public sealed class LevelSelector
{
    private readonly GameplayLoopFlowController _gameplayLoopFlowController;
    private readonly LevelLoadoutController _levelLoadoutController;
    private readonly MoneyManager _moneyManager;
    private readonly IngredientsStorageSettings _ingredientsStorageSettings;
    private readonly LevelRunContext _runContext;
    
    public LevelSelector(
        GameplayLoopFlowController gameplayLoopFlowController,
        LevelLoadoutController levelLoadoutController,
        MoneyManager moneyManager,
        IngredientsStorageSettings ingredientsStorageSettings,
        LevelRunContext runContext)
    {
        _gameplayLoopFlowController = gameplayLoopFlowController ?? throw new ArgumentNullException(nameof(gameplayLoopFlowController));
        _levelLoadoutController = levelLoadoutController ?? throw new ArgumentNullException(nameof(levelLoadoutController));
        _moneyManager = moneyManager ?? throw new ArgumentNullException(nameof(moneyManager));
        _ingredientsStorageSettings = ingredientsStorageSettings ?? throw new ArgumentNullException(nameof(ingredientsStorageSettings));
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
    }

    public Level SelectedLevel => _runContext.SelectedLevel;
    
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

        _runContext.StartRun(LevelRunMode.Normal, level, _levelLoadoutController.GetLoadout(level));
        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            _runContext.Clear();
            return;
        }

        _levelLoadoutController.ConsumeLoadout(level);
        _moneyManager.TrySpend(level.PriceToPlay);
        LevelStarted(level);
    }

    public async UniTask<bool> PlayArcadeLevel(Level level)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        var arcadeLoadout = CreateArcadeLoadout();
        _runContext.StartRun(LevelRunMode.Arcade, level, arcadeLoadout);

        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            _runContext.Clear();
            return false;
        }

        LevelStarted(level);
        return true;
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

        _runContext.StartRun(LevelRunMode.Normal, level, _levelLoadoutController.GetLoadout(level));
        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            _runContext.Clear();
            return false;
        }

        LevelStarted(level);
        return true;
    }

    public void ClearSelectedLevel()
    {
        _runContext.Clear();
    }
    
    public Loadout GetSelectedLevelLoadout()
    {
        if (!_runContext.HasActiveRun)
            throw new InvalidOperationException($"{nameof(LevelSelector)} requires an active level run.");

        return _runContext.CurrentLoadout;
    }

    private Loadout CreateArcadeLoadout()
    {
        var loadout = new Loadout(
            _levelLoadoutController.MaxDoughUpgradeSize,
            _levelLoadoutController.MaxFillingUpgradeSize);

        foreach (var dough in _ingredientsStorageSettings.StartingDoughs.Where(dough => dough != null).Distinct())
            loadout.AddDough(dough);

        foreach (var filling in _ingredientsStorageSettings.StartingFillings.Where(filling => filling != null).Distinct())
            loadout.AddFilling(filling);

        if (loadout.DoughCount == 0)
            throw new InvalidOperationException($"{nameof(PlayArcadeLevel)} requires at least one starting {nameof(Dough)} in {nameof(IngredientsStorageSettings)}.");

        if (loadout.FillingCount == 0)
            throw new InvalidOperationException($"{nameof(PlayArcadeLevel)} requires at least one starting {nameof(Filling)} in {nameof(IngredientsStorageSettings)}.");

        return loadout;
    }
}
