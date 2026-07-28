using System.Collections;
using StarterAssets;
using UnityEngine;

public class DoubleDeckerBusController : MonoBehaviour
{
    [Header("Routes")]
    [SerializeField] private Transform[] routeToBigBen;
    [SerializeField] private Transform[] routeToHotel;

    [Header("Player Exit Points")]
    [SerializeField] private Transform bigBenPlayerExit;
    [SerializeField] private Transform hotelPlayerExit;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float waypointReachDistance = 0.4f;

    [SerializeField] private Transform passengerSeat;

    private GameObject currentPlayer;
    private bool busMoving;
    private bool atBigBen;
    private Transform originalPlayerParent;

    public void TryBoardBus(GameObject player)
    {
        if (busMoving || player == null)
            return;

        if (atBigBen)
        {
            if (Day3SequenceManager.Instance == null ||
                !Day3SequenceManager.Instance.SelfieTaken)
            {
                Debug.Log("The player must take the selfie first.");
                return;
            }

            StartCoroutine(DriveRoute(
                player,
                routeToHotel,
                hotelPlayerExit,
                false
            ));
        }
        else
        {
            StartCoroutine(DriveRoute(
                player,
                routeToBigBen,
                bigBenPlayerExit,
                true
            ));
        }
    }
    private void AttachPlayerToBus()
    {
        if (currentPlayer == null || passengerSeat == null)
            return;

        originalPlayerParent = currentPlayer.transform.parent;

        CharacterController controller =
            currentPlayer.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        currentPlayer.transform.SetParent(passengerSeat);
        currentPlayer.transform.localPosition = Vector3.zero;
        currentPlayer.transform.localRotation = Quaternion.identity;
    }

    private void DetachPlayerFromBus()
    {
        if (currentPlayer == null)
            return;

        currentPlayer.transform.SetParent(
            originalPlayerParent,
            true
        );
    }

    private IEnumerator DriveRoute(
    GameObject player,
    Transform[] route,
    Transform playerExitPoint,
    bool travellingToBigBen)
    {
        if (route == null || route.Length == 0)
        {
            Debug.LogWarning("The bus route has no waypoints assigned.");
            yield break;
        }

        busMoving = true;
        currentPlayer = player;

        AttachPlayerToBus();
        StartBusRide();

        foreach (Transform waypoint in route)
        {
            if (waypoint == null)
                continue;

            yield return MoveToWaypoint(waypoint);
        }

        atBigBen = travellingToBigBen;
        busMoving = false;

        DetachPlayerFromBus();
        PlacePlayerAtExit(playerExitPoint);
        EndBusRide();

        if (Day3SequenceManager.Instance != null)
        {
            if (travellingToBigBen)
            {
                Day3SequenceManager.Instance.ArriveAtBigBen();
            }
            else
            {
                Day3SequenceManager.Instance.ReturnToHotel();
            }
        }

        currentPlayer = null;
    }

    private IEnumerator MoveToWaypoint(Transform waypoint)
    {
        while (Vector3.Distance(transform.position, waypoint.position)
               > waypointReachDistance)
        {
            Vector3 direction =
                waypoint.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction.normalized);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                waypoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = waypoint.position;
    }

    private void StartBusRide()
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
            movement.SetBusRiding(true);
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

    private void EndBusRide()
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

        foreach (Renderer playerRenderer in renderers)
        {
            playerRenderer.enabled = true;
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }

        if (movement != null)
        {
            movement.SetBusRiding(false);
        }
    }

    private void PlacePlayerAtExit(Transform exitPoint)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("There is no current player to exit the bus.");
            return;
        }

        if (exitPoint == null)
        {
            Debug.LogWarning("The player exit point is not assigned.");
            return;
        }

        CharacterController controller =
            currentPlayer.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        currentPlayer.transform.SetPositionAndRotation(
            exitPoint.position,
            exitPoint.rotation
        );

        // Forces all child transforms to update before enabling the controller.
        Physics.SyncTransforms();
    }
}