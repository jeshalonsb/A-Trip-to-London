using StarterAssets;
using TMPro;
using UnityEngine;

public class SelfieSpot : MonoBehaviour
{
    [SerializeField] private TMP_Text interactionText;

    private bool playerInside;

    private void Start()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Day3SequenceManager.Instance != null)
            {
                Day3SequenceManager.Instance.TryTakeSelfie();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player =
            other.GetComponentInParent<ThirdPersonController>();

        if (player == null)
            return;

        playerInside = true;

        if (interactionText != null)
        {
            interactionText.text = "Press E to take selfie";
            interactionText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player =
            other.GetComponentInParent<ThirdPersonController>();

        if (player == null)
            return;

        playerInside = false;

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}