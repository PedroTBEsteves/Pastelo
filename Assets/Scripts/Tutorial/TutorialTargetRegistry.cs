using System;
using System.Collections.Generic;
using System.Linq;

public class TutorialTargetRegistry
{
    private readonly List<TutorialTarget> _targets = new();

    public event Action TargetsChanged = delegate { };

    public void Register(TutorialTarget target)
    {
        if (target == null || _targets.Contains(target))
            return;

        _targets.Add(target);
        TargetsChanged();
    }

    public void Unregister(TutorialTarget target)
    {
        if (target == null || !_targets.Remove(target))
            return;

        TargetsChanged();
    }

    public bool TryGet(TutorialTargetId id, object context, out TutorialTarget target)
    {
        target = _targets.LastOrDefault(candidate => candidate != null && candidate.Matches(id, context));

        if (target != null)
            return true;

        if (context != null)
            return false;

        target = _targets.LastOrDefault(candidate => candidate != null && candidate.Matches(id, null));
        return target != null;
    }
}
