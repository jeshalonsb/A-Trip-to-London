using TMPro;
using UnityEngine;

public class SoccerBallKick : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float kickForce = 8f;
    [SerializeField] private float upwardForce = 1.5f;
    [SerializeField] private float interactionDistance = 3f;

    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TMP_Text interactionText;

    [Header("Audio")]
    [SerializeField] private AudioClip kickClip;
    [Range(0f, 1f)]
    [SerializeField] private float kickclipVolume;

    private Transform player;
    private Rigidbody ballRigidbody;
    private bool minigameActive;
    private bool playerCloseEnough;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("No GameObject with the Player tag was found.");
        }

        if (ballRigidbody == null)
        {
            Debug.LogError("The soccer ball is missing a Rigidbody.");
        }
    }

    private void Start()
    {
        HideInteractionText();
    }

    private void Update()
    {
        if (!minigameActive || player == null)
            return;

        Vector3 playerPosition = player.position;
        Vector3 ballPosition = transform.position;

        // Ignore height when checking distance.
        playerPosition.y = 0f;
        ballPosition.y = 0f;

        float distance = Vector3.Distance(playerPosition, ballPosition);

        playerCloseEnough = distance <= interactionDistance;

        if (playerCloseEnough)
        {
            ShowInteractionText();

            if (Input.GetKeyDown(KeyCode.E))
            {
                KickBall();
            }
        }
        else
        {
            HideInteractionText();
        }
    }

    private void KickBall()
    {
        if (ballRigidbody == null)
            return;

        Vector3 direction;

        if (playerCamera != null)
        {
            direction = playerCamera.forward;
        }
        else
        {
            direction = player.forward;
        }

        direction.y = 0f;
        direction.Normalize();

        ballRigidbody.velocity = Vector3.zero;

        Vector3 kickDirection =
            direction * kickForce +
            Vector3.up * upwardForce;

        ballRigidbody.AddForce(kickDirection, ForceMode.Impulse);

        if (kickClip != null)
        {
            AudioSource.PlayClipAtPoint(kickClip, transform.position, kickclipVolume);
        }

        Debug.Log("Soccer ball kicked.");
    }

    public void SetMinigameActive(bool active)
    {
        minigameActive = active;

        Debug.Log("Ball interaction active: " + active);

        if (!active)
        {
            HideInteractionText();
        }
    }

    private void ShowInteractionText()
    {
        if (interactionText == null)
            return;

        interactionText.text = "Press E to Kick";
        interactionText.gameObject.SetActive(true);
    }

    private void HideInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}