using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;

public sealed class LevelLoadoutLoader : MonoBehaviour
{
    [SerializeField]
    private Transform _doughsAreaRoot;

    [SerializeField]
    private Transform _fillingsAreaRoot;

    [Inject]
    private readonly LevelSelector _levelSelector;

    [Inject]
    private readonly LevelLoadoutController _levelLoadoutController;

    [Inject]
    private readonly LoadoutSettings _loadoutSettings;

    private void Awake()
    {
        var loadout = _levelSelector.GetSelectedLevelLoadout();
        var doughsArea = InstantiateArea<DoughsArea>(
            _loadoutSettings.DoughLevels[_levelLoadoutController.CurrentDoughUpgradeLevelIndex].AreaPrefab,
            _doughsAreaRoot);
        var fillingsArea = InstantiateArea<FillingsArea>(
            _loadoutSettings.FillingLevels[_levelLoadoutController.CurrentFillingUpgradeLevelIndex].AreaPrefab,
            _fillingsAreaRoot);

        doughsArea.Configure(loadout.Doughs);
        fillingsArea.Configure(loadout.Fillings);
    }

    private TArea InstantiateArea<TArea>(GameObject prefab, Transform root)
        where TArea : MonoBehaviour
    {
        if (prefab == null)
            throw new System.InvalidOperationException($"{nameof(LevelLoadoutLoader)} on '{name}' is missing an area prefab for {typeof(TArea).Name}.");

        if (root == null)
            throw new System.InvalidOperationException($"{nameof(LevelLoadoutLoader)} on '{name}' is missing an area root for {typeof(TArea).Name}.");

        var instance = Instantiate(prefab, root);
        GameObjectInjector.InjectRecursive(instance, gameObject.scene.GetSceneContainer());

        if (!instance.TryGetComponent(out TArea area))
            throw new System.InvalidOperationException($"Area prefab '{prefab.name}' is missing a {typeof(TArea).Name} component.");

        return area;
    }
}
