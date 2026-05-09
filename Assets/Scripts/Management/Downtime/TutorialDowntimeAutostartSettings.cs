using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialDowntimeAutostartSettings", menuName = "Scriptable Objects/TutorialDowntimeAutostartSettings")]
public sealed class TutorialDowntimeAutostartSettings : ScriptableObject
{
    [field: SerializeField]
    public Level Level { get; private set; }

    [SerializeField]
    private Dough[] _doughs = System.Array.Empty<Dough>();

    [SerializeField]
    private Filling[] _fillings = System.Array.Empty<Filling>();

    public IReadOnlyList<Dough> Doughs => _doughs;
    public IReadOnlyList<Filling> Fillings => _fillings;
}
