using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PickupItem : MonoBehaviour
{
    [Header("Wizualia (Unoszenie)")]
    [SerializeField] private float bobbingAmplitude = 0.25f;
    [SerializeField] private float bobbingFrequency = 1.5f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Animacja Podniesienia")]
    [SerializeField] private float collectDuration = 0.25f;
    [SerializeField] private float flyUpDistance = 1.5f;
    [SerializeField] private AudioClip collectSound;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.8f;

    [Header("Magnes")]
    [Tooltip("Czy magnes przyciąga ten przedmiot (zaznacz na monetach)")]
    [SerializeField] private bool magnetic = false;
    [Tooltip("Ile sekund leci do gracza po złapaniu przez magnes")]
    [SerializeField] private float attractDuration = 0.25f;

    // Aktywne przedmioty, które przyciąga magnes – rejestrują się same w OnEnable/OnDisable
    private static readonly List<PickupItem> magneticItems = new List<PickupItem>();
    public static IReadOnlyList<PickupItem> ActiveMagnetic => magneticItems;

    private Vector3 basePosition;
    private Vector3 initialScale;
    private Collider itemCollider;
    private PoolableObject poolableObject;

    private bool isCollected = false;
    private bool basePositionInitialized = false;
    private int playerLayer;

    private Transform attractTarget;
    private Vector3 attractOffset;
    private Vector3 attractStart;
    private float attractTime;

    public bool IsAttracting => attractTarget != null;

    protected virtual void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemCollider.isTrigger = true;

        poolableObject = GetComponent<PoolableObject>();

        initialScale = transform.localScale;
        playerLayer = LayerMask.NameToLayer("Player");
    }

    protected virtual void OnEnable()
    {
        transform.localScale = initialScale;
        isCollected = false;
        basePositionInitialized = false; // Czekamy 1 klatkę, aż Spawner ustawi pozycję
        attractTarget = null;

        if (itemCollider != null)
            itemCollider.enabled = true;

        if (magnetic)
            magneticItems.Add(this);
    }

    protected virtual void OnDisable()
    {
        if (magnetic)
            magneticItems.Remove(this);

        attractTarget = null;
    }

    protected virtual void Update()
    {
        if (isCollected) return;

        // Lot do gracza po złapaniu przez magnes
        if (attractTarget != null)
        {
            attractTime += Time.deltaTime;
            float t = Mathf.Clamp01(attractTime / Mathf.Max(0.01f, attractDuration));

            // cel liczony co klatkę, bo gracz biegnie – na koniec moneta jest dokładnie przy nim
            Vector3 target = attractTarget.position + attractOffset;
            transform.position = Vector3.Lerp(attractStart, target, t * t);

            if (t >= 1f)
                Collect(attractTarget.gameObject);

            return;
        }

        // basePosition zapisujemy w pierwszym Update (po tym, jak pool ustawi pozycję)
        if (!basePositionInitialized)
        {
            basePosition = transform.position;
            basePositionInitialized = true;
        }

        // Standardowe unoszenie
        float newY = basePosition.y + Mathf.Sin(Time.time * bobbingFrequency) * bobbingAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Standardowy obrót
        if (rotationSpeed > 0f)
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    // Wołane przez magnes
    public void StartAttraction(Transform target, Vector3 offset)
    {
        if (isCollected || attractTarget != null) return;

        attractTarget = target;
        attractOffset = offset;
        attractStart = transform.position;
        attractTime = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer || other.CompareTag("Player"))
            Collect(other.gameObject);
    }

    // Zebranie – z triggera albo po doleceniu do gracza
    public void Collect(GameObject collector)
    {
        if (isCollected) return;

        isCollected = true;
        attractTarget = null;

        if (itemCollider != null)
            itemCollider.enabled = false;

        if (collectSound != null && GameHUD.Instance != null)
            GameHUD.Instance.PlaySound(collectSound, soundVolume);

        OnCollect(collector);
        StartCoroutine(CollectAnimationRoutine());
    }

    private IEnumerator CollectAnimationRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * flyUpDistance;
        Vector3 startScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < collectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / collectDuration;

            transform.Rotate(Vector3.up, rotationSpeed * 5f * Time.deltaTime, Space.World);
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        CollectAndDispose();
    }

    protected abstract void OnCollect(GameObject player);

    protected virtual void CollectAndDispose()
    {
        // Oddajemy pod prefabem, z którego obiekt powstał (wcześniej szedł tu sam obiekt
        // i pool nigdy nie używał zebranych monet ponownie)
        if (poolableObject != null && poolableObject.prefab != null && SingleObjectPool.Instance != null)
            poolableObject.ReturnToPool();
        else
            gameObject.SetActive(false);
    }
}