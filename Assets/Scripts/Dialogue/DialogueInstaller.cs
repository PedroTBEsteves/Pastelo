using Reflex.Core;
using UnityEngine;

public class DialogueInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private float _delayBetweenCharacters;
    
    [SerializeField]
    private AudioClip[] _audioClips;
    
    [SerializeField]
    private float _minPitch;
    
    [SerializeField]
    private float _maxPitch;

    [SerializeField]
    private int _audioFrequency;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var dialogueWriter = new DialogueWriter(
            _delayBetweenCharacters,
            _audioClips,
            _minPitch,
            _maxPitch,
            _audioFrequency);

        containerBuilder.AddScoped(_ => dialogueWriter);
    }
}
