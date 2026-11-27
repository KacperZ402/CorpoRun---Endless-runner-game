using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Podpina jeden element UI sklepu pod ulepszenie power-upa.
// Wrzuć na kafelek danego power-upa (np. "Magnes") i przypisz teksty oraz przycisk.
public class PowerUpUpgradeItemUI : MonoBehaviour
{
    [SerializeField] private PowerUpUpgrades upgrades;
    [SerializeField] private PowerUpType type;

    [Header("UI (puste pola są pomijane)")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("Formaty tekstów")]
    [SerializeField] private string levelFormat = "Poziom {0}";
    [SerializeField] private string durationFormat = "{0:0.#} s → {1:0.#} s";
    [SerializeField] private string priceFormat = "{0}";

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(Buy);
    }

    private void OnEnable()
    {
        if (GameSaver.Instance != null)
            GameSaver.Instance.OnDataChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (GameSaver.Instance != null)
            GameSaver.Instance.OnDataChanged -= Refresh;
    }

    public void Refresh()
    {
        if (upgrades == null) return;

        int level = upgrades.GetLevel(type);

        if (levelText != null)
            levelText.text = string.Format(levelFormat, level);

        if (durationText != null)
            durationText.text = string.Format(durationFormat,
                upgrades.GetDuration(type, level),
                upgrades.GetDuration(type, level + 1));

        if (priceText != null)
            priceText.text = string.Format(priceFormat, upgrades.GetNextPrice(type));

        if (buyButton != null)
            buyButton.interactable = upgrades.CanAfford(type);
    }

    private void Buy()
    {
        if (upgrades != null && upgrades.TryBuy(type))
            Refresh();
    }
}