using Cysharp.Threading.Tasks;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DowntimeButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [Inject]
    private GameplayLoopFlowController _gameplayLoopFlowController;

    private void Awake()
    {
        _button.onClick.AddListener(() => _gameplayLoopFlowController.LoadDowntime().Forget());
    }
}
