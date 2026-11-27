using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.Shop
{
    // Jeden wiersz na zakładce Ulepszenia.
    public class UpgradeRowUI : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public TMP_Text levelText;
        public Image[] pips;
        public TMP_Text statText;
        public Button button;
        public Shadow buttonShadow;
        public TMP_Text buttonLabel;
        public GameObject price;
        public Image priceCoin;
        public TMP_Text priceText;

        public void Bind(PowerUpDef def, int level, int cost, int coins, Action onUpgrade)
        {
            bool max = cost < 0;
            bool afford = !max && coins >= cost;

            icon.sprite = def.icon;
            nameText.text = def.displayName;
            descriptionText.text = def.description;
            levelText.text = "LEVEL " + level + " / " + def.MaxLevel;

            for (int i = 0; i < pips.Length; i++)
            {
                pips[i].gameObject.SetActive(i < def.MaxLevel);
                pips[i].color = i < level ? ShopColors.Gold : (!max && i == level ? ShopColors.PipNext : ShopColors.PipEmpty);
            }

            string now = ShopFormat.Value(def.ValueAt(level), def.unit);
            string next = max ? "  ·  MAX" : "  →  " + ShopFormat.Value(def.ValueAt(level + 1), def.unit);
            statText.text = def.statLabel + "  <color=#FFFFFF>" + now + "</color><color=#F2B632>" + next + "</color>";

            buttonLabel.text = max ? "MAX LEVEL" : afford ? "UPGRADE" : "NOT ENOUGH COINS";
            price.SetActive(!max);
            if (!max) priceText.text = ShopFormat.Number(cost);

            Color ink = max ? ShopColors.Gold : afford ? ShopColors.Ink : ShopColors.Dim;
            button.targetGraphic.color = max ? ShopColors.MaxBg : afford ? ShopColors.Gold : ShopColors.ButtonOff;
            buttonShadow.effectColor = afford ? ShopColors.GoldShadow : max ? Color.clear : ShopColors.ShadowOff;
            buttonLabel.color = ink;
            priceText.color = ink;
            priceCoin.color = new Color(1f, 1f, 1f, afford ? 1f : 0.45f);

            button.interactable = afford;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onUpgrade());
        }
    }
}
