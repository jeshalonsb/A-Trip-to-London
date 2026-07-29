using StarterAssets;
using UnityEngine;

public class TaxiBoardingTrigger : MonoBehaviour
{
    [SerializeField] private TaxiWaypointController taxiController;

    private bool entered;

    private void Awake()
    {
        if (taxiController == null)
        {
            taxiController =
                GetComponentInParent<TaxiWaypointController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entered)
            return;

        ThirdPersonController player =
            other.GetComponentInParent<ThirdPersonController>();

        if (player == null)
            return;

        if (taxiController == null)
        {
            Debug.LogWarning(
                "TaxiWaypointController is not assigned."
            );

            return;
        }

        bool successfullyEntered =
            taxiController.TryEnterTaxi(player.gameObject);

        if (successfullyEntered)
        {
            entered = true;
        }
    }
}