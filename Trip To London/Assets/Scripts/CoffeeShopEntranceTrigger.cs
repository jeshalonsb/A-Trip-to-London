using UnityEngine;

public class CoffeeShopEntranceTrigger : MonoBehaviour
{
    private bool playerInside;
    private bool mustExitBeforeStarting;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = true;

        if (CoffeeShopSequenceManager.Instance == null)
            return;

        if (!CoffeeShopSequenceManager.Instance.EntranceUnlocked)
        {
            mustExitBeforeStarting = true;
            return;
        }

        if (!mustExitBeforeStarting)
        {
            CoffeeShopSequenceManager.Instance.TryEnterCoffeeShop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInside = false;
        mustExitBeforeStarting = false;
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }
}