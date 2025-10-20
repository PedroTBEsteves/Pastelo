using System;
using UnityEngine;

public class ApplicationController
{
    public ApplicationController()
    {
        Time.timeScale = 1;
    }
    
    public event Action Paused = delegate { };
    public event Action Resumed = delegate { };

    public bool IsPaused => Time.timeScale == 0;
    
    public void Pause()
    {
        Time.timeScale = 0;
        Paused();
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Resumed();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
