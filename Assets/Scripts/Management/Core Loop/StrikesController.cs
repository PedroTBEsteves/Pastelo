using System;
using UnityEngine;

public class StrikesController
{
    private int _strikes;

    public int RemainingStrikes => Mathf.Max(0, StrikesToFail - _strikes);
    public int StrikesToFail { get; }

    public StrikesController(OrderLoopSettings settings)
    {
        StrikesToFail = settings.StrikesToFail;
    }

    public event Action<int> RemainingStrikesChanged = delegate { };
    public event Action GameOver = delegate { };

    public void Strike()
    {
        if (_strikes >= StrikesToFail)
            return;

        _strikes++;
        RemainingStrikesChanged(RemainingStrikes);
        
        if (_strikes >= StrikesToFail)
            GameOver();
    }
}
