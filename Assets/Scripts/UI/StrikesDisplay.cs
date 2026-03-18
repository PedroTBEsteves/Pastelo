using System.Collections.Generic;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class StrikesDisplay : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private HorizontalLayoutGroup _layoutGroup;

    [SerializeField]
    private float _spacing = 12f;

    [SerializeField]
    private float _labelFontSize = 30f;
    
    [SerializeField]
    private GameObject _iconPrefab;

    [Inject]
    private readonly StrikesController _strikesController;

    private readonly List<GameObject> _icons = new();


    private void Awake()
    {
        ConfigureLayout();
        BuildIcons();
        UpdateIcons(_strikesController.RemainingStrikes);

        _strikesController.RemainingStrikesChanged += UpdateIcons;
    }

    private void OnDestroy()
    {
        if (_strikesController == null)
            return;

        _strikesController.RemainingStrikesChanged -= UpdateIcons;
    }

    private void ConfigureLayout()
    {
        _layoutGroup.childAlignment = TextAnchor.MiddleRight;
        _layoutGroup.spacing = _spacing;
        _layoutGroup.childControlWidth = false;
        _layoutGroup.childControlHeight = false;
        _layoutGroup.childForceExpandWidth = false;
        _layoutGroup.childForceExpandHeight = false;
        _layoutGroup.childScaleWidth = false;
        _layoutGroup.childScaleHeight = false;
    }

    private void BuildIcons()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        _icons.Clear();

        for (var index = 0; index < _strikesController.StrikesToFail; index++)
            _icons.Add(CreateIcon());
    }

    private GameObject CreateIcon()
    {
        return Instantiate(_iconPrefab, _layoutGroup.transform);
    }

    private void UpdateIcons(int remainingStrikes)
    {
        for (var index = 0; index < _icons.Count; index++)
            _icons[index].SetActive(index < remainingStrikes);
    }
}
