using System.Linq;
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
    private readonly LevelRunContext _runContext;

    [Inject]
    private readonly GameplayTutorialState _tutorialState;

    private DoughsArea _doughsArea;
    private FillingsArea _fillingsArea;

    private void Awake()
    {
        var doughsAreaPrefab = _runContext.IsArcade
            ? _levelLoadoutController.MaxDoughsAreaPrefab
            : _levelLoadoutController.CurrentDoughsAreaPrefab;
        var fillingsAreaPrefab = _runContext.IsArcade
            ? _levelLoadoutController.MaxFillingsAreaPrefab
            : _levelLoadoutController.CurrentFillingsAreaPrefab;

        _doughsArea = InstantiateArea<DoughsArea>(doughsAreaPrefab, _doughsAreaRoot);
        _fillingsArea = InstantiateArea<FillingsArea>(fillingsAreaPrefab, _fillingsAreaRoot);
        _runContext.LoadoutChanged += RefreshAreas;
        EnsureArcadeTutorialIngredientsAvailable();
        RefreshAreas();
    }

    private void OnDestroy()
    {
        if (_runContext != null)
            _runContext.LoadoutChanged -= RefreshAreas;
    }

    private void RefreshAreas()
    {
        var loadout = _levelSelector.GetSelectedLevelLoadout();
        _doughsArea.Configure(loadout.Doughs);
        _fillingsArea.Configure(loadout.Fillings);
    }

    private void EnsureArcadeTutorialIngredientsAvailable()
    {
        if (!_runContext.IsArcade)
            return;

        if (!_tutorialState.IsActive && !GameplayTutorialOptions.PeekShouldRunTutorial())
            return;

        var recipe = _tutorialState.TutorialRecipe;
        if (recipe == null)
            return;

        var loadout = _levelSelector.GetSelectedLevelLoadout();
        if (recipe.Dough != null && !loadout.Doughs.Contains(recipe.Dough) && !loadout.AddDough(recipe.Dough))
            Debug.LogError($"Arcade tutorial loadout does not have capacity for tutorial dough '{recipe.Dough.name}'.", this);

        foreach (var filling in recipe.Fillings.Keys)
        {
            if (filling == null)
                continue;

            if (loadout.Fillings.Contains(filling))
                continue;

            if (!loadout.AddFilling(filling))
                Debug.LogError($"Arcade tutorial loadout does not have capacity for tutorial filling '{filling.name}'.", this);
        }
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
