using UnityEngine;
using UnityEngine.UI;

// Jedna ikonka power-upa z okrągłym licznikiem czasu.
public class PowerUpSlot : MonoBehaviour
{
    public PowerUpType type;

    [SerializeField] private Image ring;   // Image Type = Filled, Fill Method = Radial 360
    [SerializeField] private Image icon;

    [Tooltip("Ile sekund przed końcem ikonka zaczyna migać (0 = bez migania)")]
    [SerializeField] private float warnTime = 2f;
    [SerializeField] private float blinkSpeed = 10f;

    private float duration;

    public bool IsVisible => gameObject.activeSelf;

    public void Show(float totalDuration)
    {
        duration = totalDuration;
        gameObject.SetActive(true);

        if (ring != null) ring.fillAmount = 1f;
        SetIconAlpha(1f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetRemaining(float remaining)
    {
        if (ring != null)
            ring.fillAmount = duration > 0f ? remaining / duration : 0f;

        // miganie pod koniec, żeby gracz wiedział, że power-up zaraz zniknie
        if (warnTime > 0f && remaining < warnTime)
            SetIconAlpha(0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed)));
    }

    private void SetIconAlpha(float alpha)
    {
        if (icon == null) return;

        Color c = icon.color;
        c.a = alpha;
        icon.color = c;
    }
}