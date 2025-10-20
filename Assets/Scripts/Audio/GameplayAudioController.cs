using System;
using Reflex.Attributes;
using UnityEngine;

public class GameplayAudioController : MonoBehaviour
{
    [SerializeField]
    private AudioSource _gameplayAudio;

    [SerializeField]
    private AudioSource _gameOverAudio;
    
    [Inject]
    private readonly StrikesController _strikesController;

    private void Awake()
    {
        _strikesController.GameOver += () =>
        {
            _gameOverAudio.Play();
            _gameplayAudio.Stop();
        };
    }
}
