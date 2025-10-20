using Reflex.Core;
using UnityEngine;

public class CookingInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(Resources.Load("Settings/Cooking/RecipeGeneratorSettings"))
            .AddScoped(typeof(RecipeGenerator))
            .AddSingleton(Resources.Load("Settings/Cooking/PastelCookingSettings"))
            .AddScoped(typeof(DeliverySequence));
    }
}
