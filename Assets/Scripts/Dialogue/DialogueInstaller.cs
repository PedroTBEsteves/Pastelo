using Reflex.Core;
using UnityEngine;

public class DialogueInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private float _delayBetweenCharacters;

    [SerializeField]
    private int _audioFrequency;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var dialogueWriter = new DialogueWriter(
            _delayBetweenCharacters,
            _audioFrequency);

        containerBuilder.AddScoped(_ => dialogueWriter);
    }
}
