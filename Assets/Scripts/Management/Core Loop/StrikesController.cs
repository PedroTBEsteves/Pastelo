using System;
using UnityEngine;

public class StrikesController
{
    private readonly int _strikesToFail;
    
    private int _strikes;

    public StrikesController(OrderLoopSettings settings)
    {
        _strikesToFail = settings.StrikesToFail;
    }

    public event Action GameOver = delegate { };

    public void Strike()
    {
        _strikes++;
        
        if (_strikes >= _strikesToFail)
            GameOver();
    }
}
