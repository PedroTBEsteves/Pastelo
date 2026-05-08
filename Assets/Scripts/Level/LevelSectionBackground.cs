using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class LevelSectionBackground : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private CameraSection _section;

    [Inject]
    private readonly LevelSelector _levelSelector;

    private void Awake()
    {
        var selectedLevel = _levelSelector.SelectedLevel;
        if (selectedLevel == null)
        {
            throw new InvalidOperationException(
                $"{nameof(LevelSectionBackground)} on '{name}' requires a selected {nameof(Level)}.");
        }

        _spriteRenderer.sprite = selectedLevel.GetSectionBackgroundSprite(_section);
    }
}
