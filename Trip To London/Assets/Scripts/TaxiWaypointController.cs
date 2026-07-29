using System.Collections;
using StarterAssets;
using UnityEngine;

public class TaxiWaypointController : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private Transform[] routeToBridge;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float waypointReachDistance = 0.4f;

    [Header("Passenger")]
    [SerializeField] private Transform passengerSeat;

    private GameObject currentPlayer;
    private Transform originalPlayerParent;
    private bool taxiMoving;

    public bool TryEnterTaxi(GameObject player)
    {
        if (taxiMoving)
        {
            Debug.LogWarning("The taxi is already moving.");
            return false;
        }

        if (player == null)
        {
            Debug.LogWarning("No player was passed to the taxi.");
            return false;
        }

        if (passengerSeat == null)
        {
            Debug.LogWarning(
                "Passenger Seat is not assigned on the taxi."
            );

            return false;
        }

        if (routeToBridge == null ||
            routeToBridge.Length == 0)
        {
            Debug.LogWarning(
                "Route To Bridge has no waypoints assigned."
            );

            return false;
        }

        currentPlayer = player;

        StartCoroutine(DriveRoute());

        return true;
    }

    private IEnumerator DriveRoute()
    {
        taxiMoving = true;

        AttachPlayerToTaxi();
        StartTaxiRide();

        // Give Unity one frame to update the player's parent,
        // Cinemachine target and child transforms.
        yield return null;

        if (Day4SequenceManager.Instance != null)
        {
            Day4SequenceManager.Instance.TaxiRideStarted();
        }

        foreach (Transform waypoint in routeToBridge)
        {
            if (waypoint == null)
                continue;

            yield return MoveToWaypoint(waypoint);
        }

        taxiMoving = false;

        if (Day4SequenceManager.Instance != null)
        {
            Day4SequenceManager.Instance.TaxiReachedBridge();
        }
    }

    private void AttachPlayerToTaxi()
    {
        if (currentPlayer == null ||
            passengerSeat == null)
        {
            return;
        }

        originalPlayerParent =
            currentPlayer.transform.parent;

        CharacterController controller =
            currentPlayer.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        // Parent the entire player root to the seat.
        currentPlayer.transform.SetParent(
            passengerSeat,
            false
        );

        currentPlayer.transform.localPosition =
            Vector3.zero;

        currentPlayer.transform.localRotation =
            Quaternion.identity;

        Physics.SyncTransforms();

        Debug.Log(
            "Player attached to taxi seat. Parent: " +
            currentPlayer.transform.parent.name
        );
    }

    private void StartTaxiRide()
    {
        if (currentPlayer == null)
            return;

        ThirdPersonController movement =
            currentPlayer.GetComponent<ThirdPersonController>();

        CharacterController controller =
            currentPlayer.GetComponent<CharacterController>();

        Animator animator =
            currentPlayer.GetComponentInChildren<Animator>();

        Renderer[] renderers =
            currentPlayer.GetComponentsInChildren<Renderer>(true);

        if (movement != null)
        {
            // Stops walking while leaving the controller enabled.
            // This allows CameraRotation() in LateUpdate to continue.
            movement.SetBusRiding(true);
            movement.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "ThirdPersonController was not found on the player root."
            );
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        foreach (Renderer playerRenderer in renderers)
        {
            playerRenderer.enabled = false;
        }
    }

    private IEnumerator MoveToWaypoint(
        Transform waypoint)
    {
        while (
            Vector3.Distance(
                transform.position,
                waypoint.position
            ) > waypointReachDistance)
        {
            Vector3 direction =
                waypoint.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction.normalized
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        turnSpeed * Time.deltaTime
                    );
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    waypoint.position,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        transform.position = waypoint.position;
    }
}