using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly ApplicationController _applicationController;

    [SerializeField, Self]
    private Button _button;
    
    [SerializeField]
    private GameObject _pausePanel;

    private void Awake()
    {
        _button.onClick.AddListener(TogglePause);
        _pausePanel.gameObject.SetActive(false);
    }

    private void TogglePause()
    {
        var paused = _applicationController.IsPaused;
        
        if (paused)
            _applicationController.Resume();
        else
            _applicationController.Pause();
        
        _pausePanel.SetActive(!paused);
    }
}
