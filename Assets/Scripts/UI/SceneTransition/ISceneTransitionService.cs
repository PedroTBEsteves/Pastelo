using System.Threading.Tasks;

public interface ISceneTransitionService
{
    bool IsTransitioning { get; }
    Task<bool> TryLoadSceneAsync(int sceneIndex);
}
