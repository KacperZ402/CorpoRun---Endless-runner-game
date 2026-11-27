using System.Collections;
using CorpoRun.Shop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CorpoRun.UI
{
    // Przełącznik wł./wył. z przesuwaną gałką.
    public class SwitchUI : MonoBehaviour
    {
        [System.Serializable] public class BoolEvent : UnityEvent<bool> { }

        public Button button;
        public Image background;
        public RectTransform knob;
        public float offX = 4f;
        public float onX = 36f;
        public BoolEvent onValueChanged = new BoolEvent();

        bool isOn = true;
        Coroutine slide;

        public bool IsOn => isOn;

        void Awake() => button.onClick.AddListener(() => Set(!isOn, true));

        public void Set(bool value, bool notify)
        {
            isOn = value;
            background.color = value ? ShopColors.Gold : new Color(1f, 1f, 1f, 0.18f);
            float x = value ? onX : offX;
            if (slide != null) StopCoroutine(slide);
            slide = null;
            // animacja tylko po kliknięciu gracza, przy wczytaniu ustawień gałka od razu na miejscu
            if (notify && Application.isPlaying && isActiveAndEnabled) slide = StartCoroutine(Slide(x));
            else knob.anchoredPosition = new Vector2(x, knob.anchoredPosition.y);
            if (notify) onValueChanged.Invoke(value);
        }

        IEnumerator Slide(float target)
        {
            float start = knob.anchoredPosition.x;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.12f)
            {
                knob.anchoredPosition = new Vector2(Mathf.Lerp(start, target, t * (2f - t)), knob.anchoredPosition.y);
                yield return null;
            }
            knob.anchoredPosition = new Vector2(target, knob.anchoredPosition.y);
            slide = null;
        }
    }
}
