using System;
using Cysharp.Threading.Tasks;

public sealed class LevelSelector
{
    private readonly GameplayLoopFlowController _gameplayLoopFlowController;
    
    public LevelSelector(GameplayLoopFlowController gameplayLoopFlowController)
    {
        _gameplayLoopFlowController = gameplayLoopFlowController ?? throw new ArgumentNullException(nameof(gameplayLoopFlowController));
    }

    public Level SelectedLevel { get; private set; }
    
    public event Action<Level> LevelStarted = delegate { };

    public async UniTask PlayLevel(Level level)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        SelectedLevel = level;
        var loaded = await _gameplayLoopFlowController.LoadLevelGameplay();
        if (!loaded)
        {
            SelectedLevel = null;
        }

        LevelStarted(level);
    }

    public void ClearSelectedLevel()
    {
        SelectedLevel = null;
    }
}
