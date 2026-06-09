using System;

public sealed class LevelRunContext
{
    public LevelRunMode Mode { get; private set; } = LevelRunMode.Normal;
    public Level SelectedLevel { get; private set; }
    public Loadout CurrentLoadout { get; private set; }
    public bool HasActiveRun => SelectedLevel != null && CurrentLoadout != null;
    public bool IsArcade => Mode == LevelRunMode.Arcade;

    public event Action LoadoutChanged = delegate { };
    public event Action<Level> RunStarted = delegate { };
    public event Action RunCleared = delegate { };

    public void StartRun(LevelRunMode mode, Level level, Loadout loadout)
    {
        SelectedLevel = level ?? throw new ArgumentNullException(nameof(level));
        CurrentLoadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
        Mode = mode;
        RunStarted(level);
        LoadoutChanged();
    }

    public void NotifyLoadoutChanged()
    {
        if (!HasActiveRun)
            throw new InvalidOperationException("Cannot notify a loadout change without an active level run.");

        LoadoutChanged();
    }

    public void Clear()
    {
        SelectedLevel = null;
        CurrentLoadout = null;
        Mode = LevelRunMode.Normal;
        RunCleared();
    }
}
