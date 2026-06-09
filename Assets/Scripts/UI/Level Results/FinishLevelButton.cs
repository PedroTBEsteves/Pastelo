using Reflex.Attributes;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FinishLevelButton : ValidatedMonoBehaviour
{
    private const string MenuSceneName = "Menu";

    [Inject]
    private readonly DayManager _dayManager;

    [Inject]
    private readonly LevelRunContext _runContext;

    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_runContext.IsArcade)
        {
            SceneManager.LoadScene(MenuSceneName);
            return;
        }

        _dayManager.FinishDay();
    }
}
