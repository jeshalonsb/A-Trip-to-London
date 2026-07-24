using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Movement")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startingPosition;
    private bool collected;

    private void Start()
    {
        startingPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World
        );

        float newY =
            startingPosition.y +
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position = new Vector3(
            startingPosition.x,
            newY,
            startingPosition.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        // Works even if the Player tag is on the player's parent.
        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
        {
            return;
        }

        CoinManager manager = CoinManager.Instance;

        // Backup in case the singleton reference was not created.
        if (manager == null)
        {
            manager = FindFirstObjectByType<CoinManager>();
        }

        collected = true;

        // Count the coin before destroying it.
        manager.CollectCoin();

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position
            );
        }

        Destroy(gameObject);
    }
}