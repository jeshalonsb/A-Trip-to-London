using StarterAssets;
using UnityEngine;

public class HotelEntranceTrigger : MonoBehaviour
{
    private bool transitionStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (transitionStarted)
            return;

        ThirdPersonController player =
            other.GetComponentInParent<ThirdPersonController>();

        if (player == null)
            return;

        // DAY 4:
        // After taking the selfie and returning on the bus,
        // entering the hotel starts the Day 4 transition.
        if (Day3SequenceManager.Instance != null &&
            Day3SequenceManager.Instance.CanEnterHotelForDayFour)
        {
            transitionStarted = true;

            Day3SequenceManager.Instance.TryStartDayFour();
            return;
        }

        // DAY 3:
        // After completing the soccer activity,
        // entering the hotel starts Day 3.
        if (HotelSequenceManager.Instance != null &&
            HotelSequenceManager.Instance.DayThreeEntranceUnlocked)
        {
            transitionStarted = true;

            HotelSequenceManager.Instance.TryEnterHotelForDayThree();
            return;
        }

        // DAY 1:
        // Original hotel entrance that starts Day 2.
        if (HotelSequenceManager.Instance != null)
        {
            transitionStarted = true;

            HotelSequenceManager.Instance.TryEnterHotel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player =
            other.GetComponentInParent<ThirdPersonController>();

        if (player == null)
            return;

        // Lets the trigger work again if an entrance requirement
        // was not actually completed.
        transitionStarted = false;
    }
}