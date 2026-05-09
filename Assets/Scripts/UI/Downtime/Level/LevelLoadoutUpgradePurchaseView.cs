using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public sealed class LevelLoadoutUpgradePurchaseView : MonoBehaviour
{
    [SerializeField]
    private Button _doughUpgradeButton;

    [SerializeField]
    private TextMeshProUGUI _doughSizeText;

    [SerializeField]
    private TextMeshProUGUI _doughUpgradeButtonText;

    [SerializeField]
    private Button _fillingUpgradeButton;

    [SerializeField]
    private TextMeshProUGUI _fillingSizeText;

    [SerializeField]
    private TextMeshProUGUI _fillingUpgradeButtonText;

    [SerializeField]
    private LocalizedString _upgradeButtonLocalizedString;

    [Inject]
    private readonly LevelLoadoutController _levelLoadoutController;

    [Inject]
    private readonly MoneyManager _moneyManager;

    private void Awake()
    {
        _doughUpgradeButton.onClick.AddListener(PurchaseDoughUpgrade);
        _fillingUpgradeButton.onClick.AddListener(PurchaseFillingUpgrade);
        _moneyManager.MoneyChanged += OnMoneyChanged;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (_doughUpgradeButton != null)
            _doughUpgradeButton.onClick.RemoveListener(PurchaseDoughUpgrade);

        if (_fillingUpgradeButton != null)
            _fillingUpgradeButton.onClick.RemoveListener(PurchaseFillingUpgrade);

        if (_moneyManager != null)
            _moneyManager.MoneyChanged -= OnMoneyChanged;

        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void PurchaseDoughUpgrade()
    {
        _levelLoadoutController.TryPurchaseDoughUpgrade();
        Refresh();
    }

    private void PurchaseFillingUpgrade()
    {
        _levelLoadoutController.TryPurchaseFillingUpgrade();
        Refresh();
    }

    private void OnMoneyChanged(MoneyChangedEvent _)
    {
        Refresh();
    }

    private void OnSelectedLocaleChanged(Locale _)
    {
        Refresh();
    }

    private void Refresh()
    {
        RefreshDoughUpgrade();
        RefreshFillingUpgrade();
    }

    private void RefreshDoughUpgrade()
    {
        var hasNextLevel = _levelLoadoutController.TryGetNextDoughUpgradeSize(out var nextSize);

        _doughSizeText.SetText(FormatSizeText(_levelLoadoutController.CurrentDoughUpgradeSize, hasNextLevel, nextSize));
        _doughUpgradeButtonText.SetText(GetDoughUpgradeButtonText());
        _doughUpgradeButton.gameObject.SetActive(hasNextLevel);
        _doughUpgradeButton.interactable = _levelLoadoutController.CanPurchaseDoughUpgrade();
    }

    private void RefreshFillingUpgrade()
    {
        var hasNextLevel = _levelLoadoutController.TryGetNextFillingUpgradeSize(out var nextSize);

        _fillingSizeText.SetText(FormatSizeText(_levelLoadoutController.CurrentFillingUpgradeSize, hasNextLevel, nextSize));
        _fillingUpgradeButtonText.SetText(GetFillingUpgradeButtonText());
        _fillingUpgradeButton.gameObject.SetActive(hasNextLevel);
        _fillingUpgradeButton.interactable = _levelLoadoutController.CanPurchaseFillingUpgrade();
    }

    private static string FormatSizeText(int currentSize, bool hasNextLevel, int nextSize)
    {
        return hasNextLevel ? $"{currentSize} -> {nextSize}" : currentSize.ToString();
    }

    private string GetDoughUpgradeButtonText()
    {
        return _upgradeButtonLocalizedString.GetLocalizedString(new
        {
            price = TextUtils.FormatAsMoney(_levelLoadoutController.GetDoughUpgradePrice())
        });
    }

    private string GetFillingUpgradeButtonText()
    {
        return _upgradeButtonLocalizedString.GetLocalizedString(new
        {
            price = TextUtils.FormatAsMoney(_levelLoadoutController.GetFillingUpgradePrice())
        });
    }
}
