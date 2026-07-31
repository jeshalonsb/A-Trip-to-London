using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SoccerBallKick : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float kickForce = 8f;
    [SerializeField] private float upwardForce = 1.5f;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Player References")]
    [Tooltip("Drag the main PlayerArmature object here.")]
    [SerializeField] private Transform player;

    [Tooltip("Drag the Main Camera here.")]
    [SerializeField] private Transform playerCamera;

    [Header("UI")]
    [Tooltip("Use a separate text object only for the soccer ball.")]
    [SerializeField] private TMP_Text interactionText;

    [Header("Audio")]
    [SerializeField] private AudioClip kickClip;

    [Range(0f, 1f)]
    [SerializeField] private float kickClipVolume = 1f;

    private Rigidbody ballRigidbody;

    private bool minigameActive;
    private bool playerWasClose;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody>();

        if (ballRigidbody == null)
        {
            Debug.LogError(
                "SoccerBallKick: The soccer ball needs a Rigidbody.",
                this
            );
        }
    }

    private void Start()
    {
        HideInteractionText();

        if (player == null)
        {
            Debug.LogError(
                "SoccerBallKick: Player is not assigned in the Inspector.",
                this
            );
        }

        if (playerCamera == null)
        {
            Debug.LogWarning(
                "SoccerBallKick: Player Camera is not assigned. " +
                "The player's forward direction will be used instead.",
                this
            );
        }

        if (interactionText == null)
        {
            Debug.LogError(
                "SoccerBallKick: Interaction Text is not assigned.",
                this
            );
        }
    }

    private void Update()
    {
        if (!minigameActive || player == null)
        {
            return;
        }

        bool playerIsClose = IsPlayerCloseEnough();

        if (playerIsClose)
        {
            ShowInteractionText();

            if (InteractPressed())
            {
                KickBall();
            }
        }
        else if (playerWasClose)
        {
            // Hide only once when the player leaves the ball.
            HideInteractionText();
        }

        playerWasClose = playerIsClose;
    }

    private bool IsPlayerCloseEnough()
    {
        Vector3 playerPosition = player.position;
        Vector3 ballPosition = transform.position;

        // Ignore vertical distance.
        playerPosition.y = 0f;
        ballPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            ballPosition
        );

        return distance <= interactionDistance;
    }

    private bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
        {
            return true;
        }
#endif

        return false;
    }

    private void KickBall()
    {
        if (ballRigidbody == null || player == null)
        {
            return;
        }

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

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = player.forward;
            direction.y = 0f;
        }

        direction.Normalize();

        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        Vector3 kickDirection =
            direction * kickForce +
            Vector3.up * upwardForce;

        ballRigidbody.AddForce(
            kickDirection,
            ForceMode.Impulse
        );

        if (kickClip != null)
        {
            AudioSource.PlayClipAtPoint(
                kickClip,
                transform.position,
                kickClipVolume
            );
        }

        HideInteractionText();
        playerWasClose = false;

        Debug.Log("Soccer ball kicked.");
    }

    public void SetMinigameActive(bool active)
    {
        minigameActive = active;
        playerWasClose = false;

        HideInteractionText();

        Debug.Log(
            "Ball interaction active: " + active
        );
    }

    private void ShowInteractionText()
    {
        if (interactionText == null)
        {
            return;
        }

        interactionText.text = "Press E to Kick";

        // Activate its parent in case a transition disabled it.
        if (interactionText.transform.parent != null &&
            !interactionText.transform.parent.gameObject.activeSelf)
        {
            interactionText.transform.parent.gameObject.SetActive(true);
        }

        interactionText.gameObject.SetActive(true);
        interactionText.enabled = true;
    }

    private void HideInteractionText()
    {
        if (interactionText == null)
        {
            return;
        }

        interactionText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        playerWasClose = false;
        HideInteractionText();
    }
}