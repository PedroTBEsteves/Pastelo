using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class CustomersInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene(Flag.Editable)]
    private InterfaceRef<ICustomerServiceDialogue> _customerServiceDialogueRef;

    [SerializeField, Scene(Flag.Editable)]
    private InterfaceRef<ICustomerDeliveryDialogue> _customerDeliveryDialogueRef;

    [SerializeField, Scene]
    private DialoguePresentationService _dialoguePresentationService;
    
    [SerializeField, Scene(Flag.Editable)]
    private InterfaceRef<ICustomerPopUpDialogue> _customerPopUpDialogueRef;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(typeof(CustomersDatabase))
            .AddScoped(_ => _dialoguePresentationService)
            .AddScoped(_ => _customerServiceDialogueRef.Value, typeof(ICustomerServiceDialogue))
            .AddScoped(_ => _customerDeliveryDialogueRef.Value, typeof(ICustomerDeliveryDialogue))
            .AddScoped(_ => _customerPopUpDialogueRef.Value, typeof(ICustomerPopUpDialogue));
    }
}
