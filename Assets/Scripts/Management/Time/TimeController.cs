using UnityEngine;

public class TimeController
{
    public TimeController(StrikesController strikesController)
    {
        strikesController.GameOver += Pause;
    }
    
    public bool Running { get; private set; } = true;

    public void Resume() => Running = true;
    public void Pause() => Running = false;
}
