using UnityEngine;

// Przeszkoda, którą tarcza może zbić. Chowa collidery i renderery,
// a przy ponownym włączeniu (segment z poola) wraca do stanu z prefaba.
public class BreakableObstacle : MonoBehaviour
{
    private Collider[] colliders;
    private Renderer[] renderers;
    private bool[] colliderDefaults;
    private bool[] rendererDefaults;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);

        colliderDefaults = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
            colliderDefaults[i] = colliders[i].enabled;

        rendererDefaults = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            rendererDefaults[i] = renderers[i].enabled;
    }

    // Segment z poola wraca z całą przeszkodą
    private void OnEnable()
    {
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = colliderDefaults[i];

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = rendererDefaults[i];
    }

    public void Break()
    {
        foreach (Collider c in colliders) c.enabled = false;
        foreach (Renderer r in renderers) r.enabled = false;
    }
}