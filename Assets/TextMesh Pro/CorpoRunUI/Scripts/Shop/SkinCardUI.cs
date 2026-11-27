using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CorpoRun.Shop
{
    // Jedna karta w siatce skinów.
    public class SkinCardUI : MonoBehaviour
    {
        public Button button;
        public Image border;
        public Image figure;
        public TMP_Text nameText;
        public TMP_Text rarityText;
        public GameObject badge;
        public Image badgeBackground;
        public TMP_Text badgeText;
        public Image priceCoin;
        public TMP_Text priceText;

        public void Bind(SkinDef skin, bool owned, bool equipped, bool afford, bool selected, Action onPick)
        {
            figure.sprite = skin.preview;
            figure.enabled = skin.preview != null;
            figure.color = new Color(1f, 1f, 1f, owned || afford ? 1f : 0.45f);
            nameText.text = skin.displayName;
            rarityText.text = skin.rarity.ToString().ToUpperInvariant();
            rarityText.color = ShopColors.Rarity(skin.rarity);
            border.color = selected ? ShopColors.Gold : ShopColors.PanelBorder;

            badge.SetActive(owned);
            badgeText.text = equipped ? "EQUIPPED" : "OWNED";
            badgeBackground.color = equipped ? ShopColors.Gold : ShopColors.TagOwned;
            badgeText.color = equipped ? ShopColors.Ink : ShopColors.Light;

            priceCoin.gameObject.SetActive(!owned);
            priceCoin.color = new Color(1f, 1f, 1f, afford ? 1f : 0.45f);
            priceText.text = owned ? "Owned" : ShopFormat.Number(skin.cost);
            priceText.color = owned ? ShopColors.Grey : afford ? ShopColors.Gold : ShopColors.Dim;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onPick());
        }
    }
}
