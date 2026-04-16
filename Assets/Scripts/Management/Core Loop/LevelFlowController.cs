using System;

public class LevelFlowController
{
    public bool IsLevelEnded { get; private set; }

    public event Action LevelEnded = delegate { };

    public void EndLevel()
    {
        if (IsLevelEnded)
            return;

        IsLevelEnded = true;
        LevelEnded();
    }
}
