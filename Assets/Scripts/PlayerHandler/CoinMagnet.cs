using System.Collections.Generic;
using UnityEngine;

// Magnes: dopóki power-up jest aktywny, przyciąga monety ze wszystkich trzech torów przed graczem.
// Wrzuć na gracza (obok PowerUpManager).
public class CoinMagnet : MonoBehaviour
{
    [SerializeField] private float rangeAhead = 15f;   // jak daleko przed graczem łapie
    [SerializeField] private float rangeBehind = 1f;
    [SerializeField] private float heightRange = 4f;   // w górę i w dół od gracza
    [SerializeField] private float checkInterval = 0.05f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f); // gdzie lecą monety

    private PowerUpManager powerUps;
    private PlayerMovement movement;
    private float timer;

    private void Awake()
    {
        powerUps = GetComponent<PowerUpManager>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (powerUps == null || !powerUps.IsActive(PowerUpType.Magnet)) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;

        Vector3 p = transform.position;

        // szerokość obejmuje wszystkie trzy tory, niezależnie od tego, na którym pasie jest gracz
        float halfWidth = (movement != null ? movement.laneDistance : 3f) * 2f + 1f;

        IReadOnlyList<PickupItem> items = PickupItem.ActiveMagnetic;
        for (int i = 0; i < items.Count; i++)
        {
            PickupItem item = items[i];
            if (item.IsAttracting) continue;

            Vector3 d = item.transform.position - p;
            if (d.z < -rangeBehind || d.z > rangeAhead) continue;
            if (Mathf.Abs(d.x) > halfWidth || Mathf.Abs(d.y) > heightRange) continue;

            item.StartAttraction(transform, targetOffset);
        }
    }
}