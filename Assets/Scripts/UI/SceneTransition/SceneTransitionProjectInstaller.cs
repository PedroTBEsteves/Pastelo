using Reflex.Core;
using UnityEngine;

public class SceneTransitionProjectInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private SceneTransitionService _sceneTransitionServicePrefab;

    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        if (_sceneTransitionServicePrefab == null)
        {
            Debug.LogError($"{nameof(SceneTransitionProjectInstaller)} requires a {nameof(SceneTransitionService)} prefab.", this);
            return;
        }

        var service = Instantiate(_sceneTransitionServicePrefab);
        service.name = _sceneTransitionServicePrefab.name;
        DontDestroyOnLoad(service);

        containerBuilder.AddSingleton(_ => service, typeof(SceneTransitionService), typeof(ISceneTransitionService));
    }
}
