using System;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DowntimeButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [Inject]
    private GameplayLoopFlowController _gameplayLoopFlowController;
    
    [Inject]
    private readonly LevelRunContext _levelRunContext;

    private void Awake()
    {
        switch (_levelRunContext.Mode)
        {
            case LevelRunMode.Normal:
                _button.onClick.AddListener(() => _gameplayLoopFlowController.LoadDowntime().Forget());
                break;
            case LevelRunMode.Arcade:
                _button.onClick.AddListener(() => SceneManager.LoadScene(0));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
