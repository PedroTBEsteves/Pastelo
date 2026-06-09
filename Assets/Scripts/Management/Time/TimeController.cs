using UnityEngine;

public class TimeController
{
    public TimeController(LevelFlowController levelFlowController, StrikesController strikesController)
    {
        levelFlowController.LevelEnded += Pause;
        strikesController.GameOver += Pause;
    }
    
    public bool Running { get; private set; } = true;

    public void Resume() => Running = true;
    public void Pause() => Running = false;
}
