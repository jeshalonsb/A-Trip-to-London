using System.Collections;
using TMPro;
using UnityEngine;

public class TrafficPlayerRespawn : MonoBehaviour
{
    [Header("Player and Hotel")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform hotelRespawnPoint;

    [Header("Warning")]
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private float warningTime = 3f;
    [SerializeField] private float respawnCooldown = 2f;

    private CharacterController controller;

    public Transform Player => player;
    public bool IsRespawning { get; private set; }

    private void Start()
    {
        if (player != null)
        {
            controller = player.GetComponent<CharacterController>();

            if (controller == null)
                controller = player.GetComponentInChildren<CharacterController>();
        }

        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        if (!IsRespawning)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        IsRespawning = true;

        if (player == null || hotelRespawnPoint == null)
        {
            Debug.LogError("TrafficPlayerRespawn: Assign the player and hotel respawn point.");
            IsRespawning = false;
            yield break;
        }

        if (controller != null)
            controller.enabled = false;

        player.position = hotelRespawnPoint.position;
        player.rotation = hotelRespawnPoint.rotation;

        if (controller != null)
            controller.enabled = true;

        if (warningText != null)
        {
            warningText.text = "CARS KILL! BE CAREFUL!";
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(warningTime);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnCooldown);

        IsRespawning = false;
    }
}
