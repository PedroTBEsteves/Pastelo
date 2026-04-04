using System;
using UnityEngine;

public class StrikesController
{
    private int _strikes;
    private bool _isGameOver;

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
            EndGame();
    }

    public void EndGame()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        GameOver();
    }
}
