using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotelEntranceTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        if (HotelSequenceManager.Instance != null)
        {
            HotelSequenceManager.Instance.TryEnterHotel();
        }
    }
}
