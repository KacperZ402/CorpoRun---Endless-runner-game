using CorpoRun.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Wybór jednej z kilku opcji (np. Low / Medium / High).
    public class SegmentedUI : MonoBehaviour
    {
        [System.Serializable] public class IntEvent : UnityEvent<int> { }

        public Button[] options;
        public IntEvent onValueChanged = new IntEvent();

        int selected;

        public int Selected => selected;

        void Awake()
        {
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                options[i].onClick.AddListener(() => Select(index, true));
            }
        }

        public void Select(int index, bool notify)
        {
            if (options == null || options.Length == 0) return;
            selected = Mathf.Clamp(index, 0, options.Length - 1);
            for (int i = 0; i < options.Length; i++)
            {
                bool on = i == selected;
                options[i].targetGraphic.color = on ? ShopColors.Gold : new Color(1f, 1f, 1f, 0f);
                TMP_Text label = options[i].GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.color = on ? ShopColors.Ink : ShopColors.Hex("C9D2DA");
            }
            if (notify) onValueChanged.Invoke(selected);
        }
    }
}
