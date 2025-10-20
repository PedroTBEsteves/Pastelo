using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class CustomersInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private InterfaceRef<ICustomerDialogue> _customerDialogueRef;
    
    [SerializeField, Scene]
    private InterfaceRef<ICustomerPopUpDialogue> _customerPopUpDialogueRef;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(typeof(CustomersDatabase))
            .AddScoped(_ => _customerDialogueRef.Value, typeof(ICustomerDialogue))
            .AddScoped(_ => _customerPopUpDialogueRef.Value, typeof(ICustomerPopUpDialogue));
    }
}
