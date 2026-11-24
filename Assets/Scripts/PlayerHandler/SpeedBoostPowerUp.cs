using UnityEngine;

// Przyśpieszenie: szybciej, nieśmiertelność i bez grawitacji (gracz nie spada między budynkami).
// Wrzuć na gracza (obok PowerUpManager).
public class SpeedBoostPowerUp : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1.8f;

    [Tooltip("Nieśmiertelność z mruganiem po końcu boosta – czas na wyjście z przeszkody, przez którą akurat leci")]
    [SerializeField] private float graceAfterBoost = 1f;

    private PowerUpManager powerUps;
    private PlayerMovement movement;
    private PlayerInvincibility invincibility;
    private PlayerLife life;

    private void Awake()
    {
        powerUps = GetComponent<PowerUpManager>();
        movement = GetComponent<PlayerMovement>();
        invincibility = GetComponent<PlayerInvincibility>();
        life = GetComponent<PlayerLife>();
    }

    private void OnEnable()
    {
        if (powerUps == null) return;
        powerUps.OnPowerUpStarted += HandleStarted;
        powerUps.OnPowerUpEnded += HandleEnded;
    }

    private void OnDisable()
    {
        if (powerUps == null) return;
        powerUps.OnPowerUpStarted -= HandleStarted;
        powerUps.OnPowerUpEnded -= HandleEnded;
    }

    private void HandleStarted(PowerUpType type, float duration)
    {
        if (type != PowerUpType.SpeedBoost) return;

        movement.StartBoost(speedMultiplier);
        invincibility.Activate(duration, false); // przez cały boost, bez mrugania
    }

    private void HandleEnded(PowerUpType type)
    {
        if (type != PowerUpType.SpeedBoost) return;

        movement.EndBoost();

        // po śmierci (power-upy kończą się wtedy automatycznie) nie dajemy nieśmiertelności
        if (life != null && life.IsDead) return;

        invincibility.Activate(graceAfterBoost, true);
    }
}