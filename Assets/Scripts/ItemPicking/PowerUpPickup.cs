using UnityEngine;

// Znajdźka power-upa – po zebraniu włącza dany power-up na określony czas.
public class PowerUpPickup : PickupItem
{
    [SerializeField] private PowerUpType type = PowerUpType.Magnet;
    [SerializeField] private float duration = 10f;

    protected override void OnCollect(GameObject player)
    {
        PowerUpManager manager = player.GetComponentInParent<PowerUpManager>();
        if (manager != null)
            manager.Activate(type, duration);
    }
}