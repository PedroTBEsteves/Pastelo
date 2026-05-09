using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FillingsArea : IngredientsArea<Filling, DraggableFillingSource>
{
    [SerializeField]
    private DraggableFillingSource[] _sources = Array.Empty<DraggableFillingSource>();

    protected override IReadOnlyList<DraggableFillingSource> Sources => _sources;
}
