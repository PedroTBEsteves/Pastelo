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
    private TextMeshProUGUI _moneyGainedText;

    [SerializeField]
    private TextMeshProUGUI _successfulOrdersText;

    [SerializeField]
    private TextMeshProUGUI _failedOrdersText;

    [SerializeField]
    private TextMeshProUGUI _burntPastelsText;

    [SerializeField]
    private TextMeshProUGUI _queueAbandonmentsText;

    [SerializeField]
    private TextMeshProUGUI _postServiceAbandonmentsText;

    [SerializeField]
    private TextMeshProUGUI _ordersMissingIngredientsText;

    [SerializeField]
    private TweenSettings _countTweenSettings = new(1f, Ease.OutQuad, useUnscaledTime: true);

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
        SetAllTextsToZero();
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
        SetAllTextsToZero();

        var snapshot = LevelResultsSnapshot.Capture(_levelMoneyManager, _levelPerformanceTracker);

        _countSequence = Sequence.Create(useUnscaledTime: _countTweenSettings.useUnscaledTime)
            .Group(Tween.Custom(0f, snapshot.MoneyGained, _countTweenSettings, SetMoney))
            .Group(Tween.Custom(0f, snapshot.SuccessfulOrdersCount, _countTweenSettings, value => SetCount(_successfulOrdersText, value)))
            .Group(Tween.Custom(0f, snapshot.FailedOrdersCount, _countTweenSettings, value => SetCount(_failedOrdersText, value)))
            .Group(Tween.Custom(0f, snapshot.BurntPastelsCount, _countTweenSettings, value => SetCount(_burntPastelsText, value)))
            .Group(Tween.Custom(0f, snapshot.QueueAbandonmentsCount, _countTweenSettings, value => SetCount(_queueAbandonmentsText, value)))
            .Group(Tween.Custom(0f, snapshot.PostServiceAbandonmentsCount, _countTweenSettings, value => SetCount(_postServiceAbandonmentsText, value)))
            .Group(Tween.Custom(0f, snapshot.OrdersMissingIngredientsCount, _countTweenSettings, value => SetCount(_ordersMissingIngredientsText, value)))
            .OnComplete(this, screen => screen.ApplySnapshot(snapshot));
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

    private void SetAllTextsToZero()
    {
        SetMoney(0f);
        SetCount(_successfulOrdersText, 0f);
        SetCount(_failedOrdersText, 0f);
        SetCount(_burntPastelsText, 0f);
        SetCount(_queueAbandonmentsText, 0f);
        SetCount(_postServiceAbandonmentsText, 0f);
        SetCount(_ordersMissingIngredientsText, 0f);
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
