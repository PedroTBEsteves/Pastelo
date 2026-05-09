using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class CustomersInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene(Flag.Editable)]
    private CustomerServiceDialogue _customerServiceDialogueRef;

    [SerializeField, Scene(Flag.Editable)]
    private CustomerDeliveryDialogue _customerDeliveryDialogueRef;

    [SerializeField, Scene]
    private DialoguePresentationService _dialoguePresentationService;
    
    [SerializeField, Scene(Flag.Editable)]
    private CustomerPopUpDialogue _customerPopUpDialogueRef;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(typeof(CustomersDatabase))
            .AddScoped(_ => _dialoguePresentationService)
            .AddScoped(_ => _customerServiceDialogueRef, typeof(ICustomerServiceDialogue))
            .AddScoped(_ => _customerDeliveryDialogueRef, typeof(ICustomerDeliveryDialogue))
            .AddScoped(_ => _customerPopUpDialogueRef, typeof(ICustomerPopUpDialogue));
    }
}
