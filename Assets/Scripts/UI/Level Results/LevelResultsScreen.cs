using System;
using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelResultsScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject _screen;

    [SerializeField]
    private TextMeshProUGUI _moneyGainedLegend;
    
    [SerializeField]
    private TextMeshProUGUI _moneyGainedText;

    [SerializeField]
    private TextMeshProUGUI _successfulOrdersLegend;
    
    [SerializeField]
    private TextMeshProUGUI _successfulOrdersText;

    [SerializeField]
    private TextMeshProUGUI _failedOrdersLegend;
    
    [SerializeField]
    private TextMeshProUGUI _failedOrdersText;
    
    [SerializeField]
    private TextMeshProUGUI _ordersMissingIngredientsLegend;
    
    [SerializeField]
    private TextMeshProUGUI _ordersMissingIngredientsText;

    [SerializeField]
    private TextMeshProUGUI _queueAbandonmentsLegend;
    
    [SerializeField]
    private TextMeshProUGUI _queueAbandonmentsText;

    [SerializeField]
    private TextMeshProUGUI _postServiceAbandonmentsLegend;
    
    [SerializeField]
    private TextMeshProUGUI _postServiceAbandonmentsText;
    
    [SerializeField]
    private TextMeshProUGUI _burntPastelsLegend;
    
    [SerializeField]
    private TextMeshProUGUI _burntPastelsText;

    [SerializeField]
    private TweenSettings _countTweenSettings = new(1f, Ease.OutQuad, useUnscaledTime: true);
    
    [SerializeField]
    private TweenSettings _legendTweenSettings = new(1f, Ease.OutQuad, useUnscaledTime: true);

    [Inject]
    private readonly LevelFlowController _levelFlowController;

    [Inject]
    private readonly LevelMoneyManager _levelMoneyManager;

    [Inject]
    private readonly LevelPerformanceTracker _levelPerformanceTracker;

    private Sequence _countSequence;

    private void Awake()
    {
        _levelFlowController.LevelEnded += OnLevelEnded;
        HideScreen();
    }

    private void OnDestroy()
    {
        if (_levelFlowController != null)
            _levelFlowController.LevelEnded -= OnLevelEnded;

        StopCountSequence();
    }

    private void OnLevelEnded()
    {
        StopCountSequence();
        ShowScreen();
        EmptyAllTexts();

        var snapshot = LevelResultsSnapshot.Capture(_levelMoneyManager, _levelPerformanceTracker);

        _countSequence = Sequence.Create(useUnscaledTime: _countTweenSettings.useUnscaledTime)
            .Chain(TextAnimationSequence(_moneyGainedLegend, snapshot.MoneyGained, SetMoney))
            .Chain(TextAnimationSequence(_successfulOrdersLegend, snapshot.SuccessfulOrdersCount, value => SetCount(_successfulOrdersText, value)))
            .Chain(TextAnimationSequence(_failedOrdersLegend, snapshot.FailedOrdersCount, value => SetCount(_failedOrdersText, value)))
            .Chain(TextAnimationSequence(_ordersMissingIngredientsLegend, snapshot.OrdersMissingIngredientsCount, value => SetCount(_ordersMissingIngredientsText, value)))
            .Chain(TextAnimationSequence(_queueAbandonmentsLegend, snapshot.QueueAbandonmentsCount, value => SetCount(_queueAbandonmentsText, value)))
            .Chain(TextAnimationSequence(_postServiceAbandonmentsLegend, snapshot.PostServiceAbandonmentsCount, value => SetCount(_postServiceAbandonmentsText, value)))
            .Chain(TextAnimationSequence(_burntPastelsLegend, snapshot.BurntPastelsCount, value => SetCount(_burntPastelsText, value)))
            .OnComplete(this, screen => screen.ApplySnapshot(snapshot));
    }

    private Sequence TextAnimationSequence(TextMeshProUGUI legendText, float initialValue, Action<float> onValueChange)
    {
        return Sequence.Create(useUnscaledTime: _countTweenSettings.useUnscaledTime)
            .Chain(LegendAnimationTween(legendText))
            .Chain(Tween.Custom(0f, initialValue, _countTweenSettings, onValueChange));
    }
    
    private Tween LegendAnimationTween(TextMeshProUGUI legendText)
    {
        legendText.maxVisibleCharacters = 0;
        return Tween.TextMaxVisibleCharacters(legendText, legendText.text.Length, _legendTweenSettings);
    }

    private void ApplySnapshot(LevelResultsSnapshot snapshot)
    {
        SetMoney(snapshot.MoneyGained);
        SetCount(_successfulOrdersText, snapshot.SuccessfulOrdersCount);
        SetCount(_failedOrdersText, snapshot.FailedOrdersCount);
        SetCount(_burntPastelsText, snapshot.BurntPastelsCount);
        SetCount(_queueAbandonmentsText, snapshot.QueueAbandonmentsCount);
        SetCount(_postServiceAbandonmentsText, snapshot.PostServiceAbandonmentsCount);
        SetCount(_ordersMissingIngredientsText, snapshot.OrdersMissingIngredientsCount);
    }

    private void EmptyAllTexts()
    {
        EmptyText(_moneyGainedText);
        EmptyText(_successfulOrdersText);
        EmptyText(_failedOrdersText);
        EmptyText(_burntPastelsText);
        EmptyText(_queueAbandonmentsText);
        EmptyText(_postServiceAbandonmentsText);
        EmptyText(_ordersMissingIngredientsText);
    }

    private void EmptyText(TextMeshProUGUI text)
    {
        text.SetText(string.Empty);
    }
    
    private void SetMoney(float value)
    {
        _moneyGainedText.SetText(TextUtils.FormatAsMoney(value));
    }

    private static void SetCount(TextMeshProUGUI text, float value)
    {
        text.SetText(Mathf.FloorToInt(value).ToString());
    }

    private void HideScreen()
    {
        if (_screen != null)
            _screen.SetActive(false);
    }

    private void ShowScreen()
    {
        if (_screen != null)
            _screen.SetActive(true);
    }

    private void StopCountSequence()
    {
        if (_countSequence.isAlive)
            _countSequence.Stop();
    }

    private readonly struct LevelResultsSnapshot
    {
        public LevelResultsSnapshot(
            float moneyGained,
            int successfulOrdersCount,
            int failedOrdersCount,
            int burntPastelsCount,
            int queueAbandonmentsCount,
            int postServiceAbandonmentsCount,
            int ordersMissingIngredientsCount)
        {
            MoneyGained = moneyGained;
            SuccessfulOrdersCount = successfulOrdersCount;
            FailedOrdersCount = failedOrdersCount;
            BurntPastelsCount = burntPastelsCount;
            QueueAbandonmentsCount = queueAbandonmentsCount;
            PostServiceAbandonmentsCount = postServiceAbandonmentsCount;
            OrdersMissingIngredientsCount = ordersMissingIngredientsCount;
        }

        public float MoneyGained { get; }
        public int SuccessfulOrdersCount { get; }
        public int FailedOrdersCount { get; }
        public int BurntPastelsCount { get; }
        public int QueueAbandonmentsCount { get; }
        public int PostServiceAbandonmentsCount { get; }
        public int OrdersMissingIngredientsCount { get; }

        public static LevelResultsSnapshot Capture(
            LevelMoneyManager levelMoneyManager,
            LevelPerformanceTracker levelPerformanceTracker)
        {
            return new LevelResultsSnapshot(
                levelMoneyManager.Amount,
                levelPerformanceTracker.SuccessfulOrdersCount,
                levelPerformanceTracker.FailedOrdersCount,
                levelPerformanceTracker.BurntPastelsCount,
                levelPerformanceTracker.QueueAbandonmentsCount,
                levelPerformanceTracker.PostServiceAbandonmentsCount,
                levelPerformanceTracker.OrdersMissingIngredientsCount);
        }
    }
}
