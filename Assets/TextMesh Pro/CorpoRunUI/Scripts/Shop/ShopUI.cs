using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CorpoRun.Shop
{
    public static class ShopColors
    {
        public static readonly Color Gold = Hex("F2B632");
        public static readonly Color GoldShadow = Hex("A9761A");
        public static readonly Color Ink = Hex("141418");
        public static readonly Color Grey = Hex("A8B4C0");
        public static readonly Color Dim = Hex("8A93A8");
        public static readonly Color Light = Hex("E6EAF0");
        public static readonly Color Panel = new Color(0.078f, 0.094f, 0.141f, 0.92f);
        public static readonly Color PanelBorder = new Color(1f, 1f, 1f, 0.12f);
        public static readonly Color PipEmpty = new Color(1f, 1f, 1f, 0.14f);
        public static readonly Color PipNext = new Color(0.949f, 0.714f, 0.196f, 0.35f);
        public static readonly Color ButtonOff = new Color(1f, 1f, 1f, 0.08f);
        public static readonly Color ButtonEquipped = new Color(1f, 1f, 1f, 0.12f);
        public static readonly Color ShadowOff = new Color(0f, 0f, 0f, 0.3f);
        public static readonly Color MaxBg = new Color(0.949f, 0.714f, 0.196f, 0.14f);
        public static readonly Color TagOwned = new Color(1f, 1f, 1f, 0.14f);

        public static Color Rarity(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare: return Hex("5BB0FF");
                case SkinRarity.Epic: return Hex("C49BFF");
                case SkinRarity.Legendary: return Hex("F2B632");
                default: return Hex("A8B4C0");
            }
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }

        public static Color Alpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }

    public static class ShopFormat
    {
        static readonly NumberFormatInfo Numbers = new NumberFormatInfo { NumberGroupSeparator = " ", NumberDecimalSeparator = "." };

        // 1000 -> "1 000"
        public static string Number(float n) => n.ToString(Mathf.Approximately(n, Mathf.Round(n)) ? "#,0" : "#,0.#", Numbers);

        public static string Value(float v, string unit) => string.IsNullOrEmpty(unit) ? Number(v) : Number(v) + " " + unit;
    }

    // Sklep: zakładki Ulepszenia i Skiny. Otwórz przez Open(), zamyka przycisk wstecz.
    public class ShopUI : MonoBehaviour
    {
        [System.Serializable] public class SkinEvent : UnityEvent<string> { }

        [Header("Dane")]
        public ShopCatalog catalog;

        [Header("Nagłówek")]
        public Button backButton;
        public Button upgradesTab;
        public Button skinsTab;
        public TMP_Text coinsText;

        [Header("Ulepszenia")]
        public GameObject upgradesPage;
        public Transform upgradeList;
        public UpgradeRowUI upgradeRowTemplate;

        [Header("Skiny")]
        public GameObject skinsPage;
        public Transform skinGrid;
        public SkinCardUI skinCardTemplate;
        public Image previewGlow;
        public Image previewFigure;
        public TMP_Text previewName;
        public Image previewRarityBackground;
        public TMP_Text previewRarity;
        public TMP_Text previewDescription;
        public Button previewButton;
        public Shadow previewButtonShadow;
        public TMP_Text previewButtonLabel;
        public GameObject previewPrice;
        public Image previewPriceCoin;
        public TMP_Text previewPriceText;
        public TMP_Text previewHint;

        [Header("Komunikat")]
        public CanvasGroup toast;
        public TMP_Text toastText;

        [Header("Zdarzenia")]
        public UnityEvent onClosed = new UnityEvent();
        public SkinEvent onSkinEquipped = new SkinEvent();

        readonly List<UpgradeRowUI> rows = new List<UpgradeRowUI>();
        readonly List<SkinCardUI> cards = new List<SkinCardUI>();
        bool showSkins;
        int picked = -1;
        Coroutine toastRoutine;

        void Awake()
        {
            backButton.onClick.AddListener(Close);
            upgradesTab.onClick.AddListener(() => ShowTab(false));
            skinsTab.onClick.AddListener(() => ShowTab(true));
            previewButton.onClick.AddListener(OnPreviewButton);
            if (toast != null) toast.alpha = 0f;
        }

        void OnEnable()
        {
            ShopSave.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            ShopSave.Changed -= Refresh;
            if (toast != null) toast.alpha = 0f;
            toastRoutine = null;
        }

        public void Open() => gameObject.SetActive(true);

        public void OpenSkins()
        {
            showSkins = true;
            Open();
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            onClosed.Invoke();
        }

        public void ShowTab(bool skins)
        {
            showSkins = skins;
            Refresh();
        }

        public void Refresh()
        {
            if (catalog == null) return;
            int coins = ShopSave.Coins;
            coinsText.text = ShopFormat.Number(coins);
            StyleTab(upgradesTab, !showSkins);
            StyleTab(skinsTab, showSkins);
            upgradesPage.SetActive(!showSkins);
            skinsPage.SetActive(showSkins);
            RefreshUpgrades(coins);
            RefreshSkins(coins);
        }

        [ContextMenu("Podgląd: Ulepszenia")]
        void PreviewUpgrades() => ShowTab(false);

        [ContextMenu("Podgląd: Skiny")]
        void PreviewSkins() => ShowTab(true);

        [ContextMenu("Test: +5000 monet")]
        void AddTestCoins()
        {
            ShopSave.AddCoins(5000);
            Refresh();
        }

        [ContextMenu("Test: wyczyść zapis sklepu")]
        void ResetTestSave()
        {
            ShopSave.ResetForTesting(catalog);
            picked = -1;
            Refresh();
        }

        void RefreshUpgrades(int coins)
        {
            List<PowerUpDef> defs = catalog.powerUps;
            Fill(rows, upgradeRowTemplate, upgradeList, defs.Count);
            for (int i = 0; i < defs.Count; i++)
            {
                PowerUpDef def = defs[i];
                int level = Mathf.Min(ShopSave.GetLevel(def.id), def.MaxLevel);
                rows[i].Bind(def, level, catalog.UpgradeCost(def, level), coins, () => Upgrade(def));
            }
        }

        void Upgrade(PowerUpDef def)
        {
            int level = Mathf.Min(ShopSave.GetLevel(def.id), def.MaxLevel);
            int cost = catalog.UpgradeCost(def, level);
            if (cost < 0 || !ShopSave.TrySpend(cost)) return;
            ShopSave.SetLevel(def.id, level + 1);
            ShowToast(def.displayName + " upgraded to level " + (level + 1) + "!");
        }

        void RefreshSkins(int coins)
        {
            List<SkinDef> skins = catalog.skins;
            Fill(cards, skinCardTemplate, skinGrid, skins.Count);
            if (skins.Count == 0) return;
            string equipped = ShopSave.EquippedSkin;
            if (picked < 0 || picked >= skins.Count) picked = Mathf.Max(0, skins.FindIndex(x => x.id == equipped));
            for (int i = 0; i < skins.Count; i++)
            {
                int index = i;
                SkinDef skin = skins[i];
                bool owned = catalog.IsOwned(skin);
                cards[i].Bind(skin, owned, skin.id == equipped, !owned && coins >= skin.cost, i == picked, () => Pick(index));
            }
            BindPreview(skins[picked], coins, equipped);
        }

        void Pick(int index)
        {
            picked = index;
            Refresh();
        }

        void BindPreview(SkinDef skin, int coins, string equipped)
        {
            bool owned = catalog.IsOwned(skin);
            bool isEquipped = skin.id == equipped;
            bool afford = !owned && coins >= skin.cost;
            bool canAct = !isEquipped && (owned || afford);
            Color rarity = ShopColors.Rarity(skin.rarity);

            previewFigure.sprite = skin.preview;
            previewFigure.enabled = skin.preview != null;
            previewGlow.color = ShopColors.Alpha(rarity, 0.35f);
            previewName.text = skin.displayName;
            previewRarity.text = skin.rarity.ToString().ToUpperInvariant();
            previewRarity.color = rarity;
            previewRarityBackground.color = ShopColors.Alpha(rarity, 0.16f);
            previewDescription.text = skin.description;

            previewButtonLabel.text = isEquipped ? "EQUIPPED" : owned ? "EQUIP" : afford ? "BUY" : "NEED";
            previewPrice.SetActive(!owned);
            previewPriceText.text = ShopFormat.Number(skin.cost);
            previewPriceCoin.color = new Color(1f, 1f, 1f, afford ? 1f : 0.45f);
            previewButton.targetGraphic.color = canAct ? ShopColors.Gold : isEquipped ? ShopColors.ButtonEquipped : ShopColors.ButtonOff;
            previewButtonShadow.effectColor = canAct ? ShopColors.GoldShadow : ShopColors.ShadowOff;
            Color ink = canAct ? ShopColors.Ink : isEquipped ? ShopColors.Light : ShopColors.Dim;
            previewButtonLabel.color = ink;
            previewPriceText.color = ink;
            previewButton.interactable = canAct;
            previewHint.text = isEquipped ? "This is your current runner"
                : !owned && !afford ? "Collect " + ShopFormat.Number(skin.cost - coins) + " more coins to unlock" : "";
        }

        void OnPreviewButton()
        {
            if (catalog == null || picked < 0 || picked >= catalog.skins.Count) return;
            SkinDef skin = catalog.skins[picked];
            if (ShopSave.EquippedSkin == skin.id) return;
            bool owned = catalog.IsOwned(skin);
            if (!owned && !ShopSave.TrySpend(skin.cost)) return;
            if (!owned) ShopSave.SetOwned(skin.id);
            ShopSave.Equip(skin.id);
            ShowToast(owned ? skin.displayName + " equipped" : skin.displayName + " unlocked!");
            onSkinEquipped.Invoke(skin.id);
        }

        void ShowToast(string message)
        {
            if (toast == null || !isActiveAndEnabled) return;
            toastText.text = message;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine());
        }

        IEnumerator ToastRoutine()
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.15f)
            {
                toast.alpha = t;
                yield return null;
            }
            toast.alpha = 1f;
            yield return new WaitForSecondsRealtime(1.6f);
            for (float t = 1f; t > 0f; t -= Time.unscaledDeltaTime / 0.25f)
            {
                toast.alpha = t;
                yield return null;
            }
            toast.alpha = 0f;
            toastRoutine = null;
        }

        static void StyleTab(Button tab, bool active)
        {
            Graphic background = tab.targetGraphic;
            if (background != null) background.color = active ? ShopColors.Gold : new Color(1f, 1f, 1f, 0f);
            Color ink = active ? ShopColors.Ink : Color.white;
            foreach (Graphic g in tab.GetComponentsInChildren<Graphic>(true))
                if (g != background) g.color = ink;
        }

        // Używa istniejących dzieci (wiersze i karty z budowania), brakujące klonuje z szablonu.
        static void Fill<T>(List<T> items, T template, Transform parent, int count) where T : Component
        {
            items.Clear();
            for (int i = 0; i < count; i++)
            {
                T item = i < parent.childCount ? parent.GetChild(i).GetComponent<T>() : null;
                if (item == null) item = Instantiate(template, parent);
                item.gameObject.SetActive(true);
                items.Add(item);
            }
            for (int i = count; i < parent.childCount; i++)
                parent.GetChild(i).gameObject.SetActive(false);
        }
    }
}
