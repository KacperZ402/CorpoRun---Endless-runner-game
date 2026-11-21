using UnityEngine;

// Tarcza: dopóki power-up jest aktywny, pierwsze uderzenie w przeszkodę ją zbija zamiast zabić gracza.
// Wrzuć na gracza (obiekt z Rigidbody – obok PlayerLife i PowerUpManager).
public class ShieldPowerUp : MonoBehaviour
{
    [Tooltip("Krótka nieśmiertelność po zbiciu przeszkody – żeby wyhamowanie nie zostało uznane za śmierć")]
    [SerializeField] private float graceTime = 0.5f;

    private PowerUpManager powerUps;
    private PlayerInvincibility invincibility;
    private int obstacleLayer;

    private void Awake()
    {
        powerUps = GetComponent<PowerUpManager>();
        invincibility = GetComponent<PlayerInvincibility>();
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (powerUps == null || !powerUps.IsActive(PowerUpType.Shield)) return;
        if (collision.collider.gameObject.layer != obstacleLayer) return;

        // tarcza działa tylko raz
        powerUps.Deactivate(PowerUpType.Shield);

        BreakableObstacle obstacle = collision.collider.GetComponentInParent<BreakableObstacle>();
        if (obstacle != null)
            obstacle.Break();

        // chwila nieśmiertelności: PlayerLife nie uzna wyhamowania za śmierć,
        // a gracz przejdzie przez przeszkodę, nawet jeśli nie da się jej zbić
        if (invincibility != null)
            invincibility.Activate(graceTime);
    }
}