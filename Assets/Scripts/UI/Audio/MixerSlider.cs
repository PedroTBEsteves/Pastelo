using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerSlider : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Slider _slider;

    [SerializeField]
    private AudioMixerGroup _group;

    private void Awake()
    {
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
        _group.audioMixer.GetFloat($"{_group.name}Volume", out var volume);
        _slider.value = Mathf.Pow(10, volume / 20);
    }

    private void OnSliderValueChanged(float value)
    {
        var volume = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;
        
        _group.audioMixer.SetFloat($"{_group.name}Volume", volume);
    }
}
