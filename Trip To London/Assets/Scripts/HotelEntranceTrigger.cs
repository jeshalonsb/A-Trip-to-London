using UnityEngine;

public class HotelEntranceTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (HotelSequenceManager.Instance == null)
            return;

        // After the soccer goal, entering the hotel starts Day 3.
        if (HotelSequenceManager.Instance.DayThreeEntranceUnlocked)
        {
            HotelSequenceManager.Instance.TryEnterHotelForDayThree();
            return;
        }

        // During Day 1, entering the hotel starts Day 2.
        if (HotelSequenceManager.Instance.HotelEntranceUnlocked)
        {
            HotelSequenceManager.Instance.TryEnterHotel();
        }
    }
}