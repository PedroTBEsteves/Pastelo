using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DoughsArea : IngredientsArea<Dough, DraggableDoughSource>
{
    [SerializeField]
    private DraggableDoughSource[] _sources = Array.Empty<DraggableDoughSource>();

    protected override IReadOnlyList<DraggableDoughSource> Sources => _sources;
}
