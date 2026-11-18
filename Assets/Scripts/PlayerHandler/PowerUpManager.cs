using UnityEngine;

public enum PowerUpType { Magnet } // kolejne (tarcza, mnożnik...) dopisujesz tutaj

// Trzyma aktywne power-upy i ich czasy. Wrzuć na gracza.
public class PowerUpManager : MonoBehaviour
{
    public event System.Action<PowerUpType, float> OnPowerUpStarted; // typ, czas trwania
    public event System.Action<PowerUpType> OnPowerUpEnded;

    private float[] remaining;
    private PlayerLife life;

    private void Awake()
    {
        remaining = new float[System.Enum.GetValues(typeof(PowerUpType)).Length];
        life = GetComponent<PlayerLife>();
    }

    private void OnEnable()
    {
        if (life != null) life.OnDied += DeactivateAll;
    }

    private void OnDisable()
    {
        if (life != null) life.OnDied -= DeactivateAll;
    }

    public void Activate(PowerUpType type, float duration)
    {
        int i = (int)type;

        // drugi taki sam power-up odświeża czas, a nie go sumuje
        remaining[i] = Mathf.Max(remaining[i], duration);
        OnPowerUpStarted?.Invoke(type, remaining[i]);
    }

    public bool IsActive(PowerUpType type) => remaining[(int)type] > 0f;
    public float GetRemaining(PowerUpType type) => remaining[(int)type];

    public void DeactivateAll()
    {
        for (int i = 0; i < remaining.Length; i++)
        {
            if (remaining[i] <= 0f) continue;
            remaining[i] = 0f;
            OnPowerUpEnded?.Invoke((PowerUpType)i);
        }
    }

    private void Update()
    {
        for (int i = 0; i < remaining.Length; i++)
        {
            if (remaining[i] <= 0f) continue;

            remaining[i] -= Time.deltaTime;
            if (remaining[i] <= 0f)
            {
                remaining[i] = 0f;
                OnPowerUpEnded?.Invoke((PowerUpType)i);
            }
        }
    }
}