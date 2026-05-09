using System;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DowntimeTutorialAutostart
{
    private const string DowntimeScenePath = "Assets/Scenes/Downtime.unity";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.path != DowntimeScenePath || !GameplayTutorialOptions.PeekShouldRunTutorial())
            return;

        RunAutostartAsync(scene).Forget();
    }

    private static async UniTaskVoid RunAutostartAsync(Scene scene)
    {
        await UniTask.Yield(PlayerLoopTiming.Update);

        if (!scene.IsValid() || !scene.isLoaded || !GameplayTutorialOptions.PeekShouldRunTutorial())
            return;

        Container container;
        try
        {
            container = scene.GetSceneContainer();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to resolve Downtime scene container for tutorial autostart.\n{exception}");
            return;
        }

        TutorialDowntimeAutostartSettings settings;
        LevelSelector levelSelector;
        LevelLoadoutController levelLoadoutController;
        MoneyManager moneyManager;

        try
        {
            settings = container.Resolve<TutorialDowntimeAutostartSettings>();
            levelSelector = container.Resolve<LevelSelector>();
            levelLoadoutController = container.Resolve<LevelLoadoutController>();
            moneyManager = container.Resolve<MoneyManager>();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to resolve Downtime tutorial autostart dependencies.\n{exception}");
            return;
        }

        if (settings == null)
        {
            Debug.LogError("Tutorial downtime autostart is enabled, but TutorialDowntimeAutostartSettings could not be loaded from Resources/Settings/Management.");
            return;
        }

        if (settings.Level == null)
        {
            Debug.LogError($"{nameof(TutorialDowntimeAutostartSettings)} requires a configured level.");
            return;
        }

        try
        {
            levelLoadoutController.ReplaceLoadout(settings.Level, settings.Doughs, settings.Fillings);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to apply tutorial downtime autostart loadout for level '{settings.Level.name}'.\n{exception}");
            return;
        }

        if (!levelSelector.CanPlayLevel(settings.Level))
        {
            var configuredDoughs = settings.Doughs.Count;
            var configuredFillings = settings.Fillings.Count;
            var hasEnoughMoney = moneyManager.CanSpend(settings.Level.PriceToPlay);
            var canConsumeLoadout = levelLoadoutController.CanConsumeLoadout(settings.Level);

            Debug.LogError(
                $"Tutorial downtime autostart could not start level '{settings.Level.name}'. " +
                $"Configured loadout: {configuredDoughs} dough(s), {configuredFillings} filling(s). " +
                $"Can consume loadout: {canConsumeLoadout}. Has enough money: {hasEnoughMoney}. " +
                $"Price to play: {settings.Level.PriceToPlay}.");
            return;
        }

        await levelSelector.PlayLevel(settings.Level);
    }
}
