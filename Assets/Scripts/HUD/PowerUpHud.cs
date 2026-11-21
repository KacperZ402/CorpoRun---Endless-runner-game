using UnityEngine;

// Pokazuje ikonki aktywnych power-upów z licznikiem czasu.
// Wrzuć na kontener, w którym siedzą sloty (kontener musi być zawsze aktywny).
public class PowerUpHUD : MonoBehaviour
{
    [SerializeField] private PowerUpManager powerUps;

    private PowerUpSlot[] slots; // indeks = typ power-upa

    private void Awake()
    {
        slots = new PowerUpSlot[System.Enum.GetValues(typeof(PowerUpType)).Length];

        foreach (PowerUpSlot slot in GetComponentsInChildren<PowerUpSlot>(true))
        {
            slots[(int)slot.type] = slot;
            slot.Hide();
        }
    }

    private void OnEnable()
    {
        if (powerUps == null) return;

        powerUps.OnPowerUpStarted += HandleStarted;
        powerUps.OnPowerUpEnded += HandleEnded;

        // HUD mógł być wyłączony (np. ekran Game Over) i przegapić eventy – wyrównujemy stan
        SyncWithManager();
    }


    private void OnDisable()
    {
        if (powerUps == null) return;
        powerUps.OnPowerUpStarted -= HandleStarted;
        powerUps.OnPowerUpEnded -= HandleEnded;
    }

    private void HandleStarted(PowerUpType type, float duration)
    {
        PowerUpSlot slot = slots[(int)type];
        if (slot != null) slot.Show(duration);
    }

    private void HandleEnded(PowerUpType type)
    {
        PowerUpSlot slot = slots[(int)type];
        if (slot != null) slot.Hide();
    }

    private void Update()
    {
        if (powerUps == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            PowerUpSlot slot = slots[i];
            if (slot == null || !slot.IsVisible) continue;

            float remaining = powerUps.GetRemaining((PowerUpType)i);
            if (remaining <= 0f)
            {
                slot.Hide(); // koniec, nawet jeśli event nie doszedł
                continue;
            }

            slot.SetRemaining(remaining);
        }
    }
    private void SyncWithManager()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            PowerUpSlot slot = slots[i];
            if (slot == null) continue;

            float remaining = powerUps.GetRemaining((PowerUpType)i);
            if (remaining <= 0f)
                slot.Hide();
            else if (!slot.IsVisible)
                slot.Show(remaining); // power-up złapany, kiedy HUD był ukryty
        }
    }
}