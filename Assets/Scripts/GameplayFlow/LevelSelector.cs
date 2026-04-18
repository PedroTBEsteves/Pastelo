using System;
using Cysharp.Threading.Tasks;

public sealed class LevelSelector
{
    private readonly GameplayLoopFlowController _gameplayLoopFlowController;
    private readonly LevelLoadoutController _levelLoadoutController;
    
    public LevelSelector(GameplayLoopFlowController gameplayLoopFlowController, LevelLoadoutController levelLoadoutController)
    {
        _gameplayLoopFlowController = gameplayLoopFlowController ?? throw new ArgumentNullException(nameof(gameplayLoopFlowController));
        _levelLoadoutController = levelLoadoutController ?? throw new ArgumentNullException(nameof(levelLoadoutController));
    }

    public Level SelectedLevel { get; private set; }
    
    public event Action<Level> LevelStarted = delegate { };

    public bool CanPlayLevel(Level level)
    {
        return _levelLoadoutController.CanConsumeLoadout(level);
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
        LevelStarted(level);
    }

    public void ClearSelectedLevel()
    {
        SelectedLevel = null;
    }
}
