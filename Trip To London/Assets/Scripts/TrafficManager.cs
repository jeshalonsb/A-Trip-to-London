using System.Collections;
using TMPro;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [System.Serializable]
    public class TrafficCar
    {
        [Header("Car")]
        public Transform carTransform;

        [Tooltip("The first waypoint this car will drive toward.")]
        public int startingWaypoint;

        [Tooltip("Allows individual cars to move slightly faster or slower.")]
        [Range(0.25f, 2f)]
        public float speedMultiplier = 1f;

        [Header("Optional Audio Override")]
        public AudioClip customDrivingSound;

        [HideInInspector] public int currentWaypoint;
        [HideInInspector] public AudioSource audioSource;
        [HideInInspector] public Rigidbody rigidbody;
    }

    [Header("Traffic Cars")]
    [SerializeField] private TrafficCar[] trafficCars;

    [Header("Shared Waypoints")]
    [SerializeField] private Transform[] waypoints;

    [Header("Driving")]
    [SerializeField] private float drivingSpeed = 8f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointReachDistance = 1.5f;

    [Tooltip("Moves each car directly onto its selected starting waypoint.")]
    [SerializeField] private bool snapCarsToStartingWaypoints;

    [Header("Car Audio")]
    [SerializeField] private AudioClip defaultDrivingSound;

    [Range(0f, 1f)]
    [SerializeField] private float drivingVolume = 0.5f;

    [SerializeField] private float minimumAudioDistance = 4f;
    [SerializeField] private float maximumAudioDistance = 30f;

    [Header("Player Hit Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float carHitRadius = 2f;
    [SerializeField] private float hitCooldown = 2f;

    [Header("Hotel Respawn")]
    [SerializeField] private Transform hotelRespawnPoint;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private float warningDuration = 3f;

    private CharacterController playerCharacterController;
    private bool playerRecentlyHit;
    private Coroutine warningCoroutine;

    private void Start()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }

        if (player != null)
        {
            playerCharacterController =
                player.GetComponent<CharacterController>();

            if (playerCharacterController == null)
            {
                playerCharacterController =
                    player.GetComponentInChildren<CharacterController>();
            }
        }

        InitializeCars();
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        MoveAllCars();
    }

    private void Update()
    {
        CheckForPlayerHits();
    }

    private void InitializeCars()
    {
        if (trafficCars == null)
            return;

        foreach (TrafficCar trafficCar in trafficCars)
        {
            if (trafficCar == null ||
                trafficCar.carTransform == null)
            {
                continue;
            }

            trafficCar.startingWaypoint = Mathf.Clamp(
                trafficCar.startingWaypoint,
                0,
                Mathf.Max(0, waypoints.Length - 1)
            );

            trafficCar.currentWaypoint =
                trafficCar.startingWaypoint;

            SetupCarRigidbody(trafficCar);

            if (snapCarsToStartingWaypoints &&
                waypoints != null &&
                waypoints.Length > 0 &&
                waypoints[trafficCar.currentWaypoint] != null)
            {
                Vector3 startPosition =
                    waypoints[trafficCar.currentWaypoint].position;

                trafficCar.carTransform.position =
                    startPosition;

                trafficCar.currentWaypoint =
                    GetNextWaypointIndex(
                        trafficCar.currentWaypoint
                    );
            }

            SetupCarAudio(trafficCar);
        }
    }

    private void SetupCarRigidbody(
        TrafficCar trafficCar)
    {
        trafficCar.rigidbody =
            trafficCar.carTransform
                .GetComponent<Rigidbody>();

        if (trafficCar.rigidbody == null)
        {
            trafficCar.rigidbody =
                trafficCar.carTransform
                    .GetComponentInParent<Rigidbody>();
        }

        if (trafficCar.rigidbody == null)
            return;

        trafficCar.rigidbody.isKinematic = true;
        trafficCar.rigidbody.useGravity = false;

        trafficCar.rigidbody.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        trafficCar.rigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;
    }

    private void SetupCarAudio(
        TrafficCar trafficCar)
    {
        AudioClip soundToUse =
            trafficCar.customDrivingSound != null
                ? trafficCar.customDrivingSound
                : defaultDrivingSound;

        if (soundToUse == null)
            return;

        AudioSource audioSource =
            trafficCar.carTransform
                .GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                trafficCar.carTransform.gameObject
                    .AddComponent<AudioSource>();
        }

        audioSource.clip = soundToUse;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = drivingVolume;

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = minimumAudioDistance;
        audioSource.maxDistance = maximumAudioDistance;
        audioSource.rolloffMode =
            AudioRolloffMode.Logarithmic;

        trafficCar.audioSource = audioSource;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void MoveAllCars()
    {
        if (trafficCars == null)
            return;

        foreach (TrafficCar trafficCar in trafficCars)
        {
            if (trafficCar == null ||
                trafficCar.carTransform == null)
            {
                continue;
            }

            if (trafficCar.currentWaypoint < 0 ||
                trafficCar.currentWaypoint >= waypoints.Length)
            {
                trafficCar.currentWaypoint = 0;
            }

            Transform targetWaypoint =
                waypoints[trafficCar.currentWaypoint];

            if (targetWaypoint == null)
            {
                trafficCar.currentWaypoint =
                    GetNextWaypointIndex(
                        trafficCar.currentWaypoint
                    );

                continue;
            }

            MoveCarTowardWaypoint(
                trafficCar,
                targetWaypoint
            );
        }
    }

    private void MoveCarTowardWaypoint(
        TrafficCar trafficCar,
        Transform targetWaypoint)
    {
        Transform car =
            trafficCar.carTransform;

        Vector3 targetPosition =
            targetWaypoint.position;

        // Keeps the car at its current height.
        targetPosition.y =
            car.position.y;

        Vector3 direction =
            targetPosition -
            car.position;

        direction.y = 0f;

        float currentSpeed =
            drivingSpeed *
            trafficCar.speedMultiplier;

        Vector3 newPosition =
            Vector3.MoveTowards(
                car.position,
                targetPosition,
                currentSpeed *
                Time.fixedDeltaTime
            );

        Quaternion newRotation =
            car.rotation;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            newRotation =
                Quaternion.Slerp(
                    car.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.fixedDeltaTime
                );
        }

        if (trafficCar.rigidbody != null)
        {
            trafficCar.rigidbody.MovePosition(
                newPosition
            );

            trafficCar.rigidbody.MoveRotation(
                newRotation
            );
        }
        else
        {
            car.position =
                newPosition;

            car.rotation =
                newRotation;
        }

        Vector2 flatCarPosition =
            new Vector2(
                newPosition.x,
                newPosition.z
            );

        Vector2 flatWaypointPosition =
            new Vector2(
                targetPosition.x,
                targetPosition.z
            );

        float distanceToWaypoint =
            Vector2.Distance(
                flatCarPosition,
                flatWaypointPosition
            );

        if (distanceToWaypoint <=
            waypointReachDistance)
        {
            trafficCar.currentWaypoint =
                GetNextWaypointIndex(
                    trafficCar.currentWaypoint
                );
        }
    }

    private int GetNextWaypointIndex(
        int currentIndex)
    {
        currentIndex++;

        if (currentIndex >= waypoints.Length)
        {
            currentIndex = 0;
        }

        return currentIndex;
    }

    private void CheckForPlayerHits()
    {
        if (player == null ||
            playerRecentlyHit ||
            trafficCars == null)
        {
            return;
        }

        foreach (TrafficCar trafficCar in trafficCars)
        {
            if (trafficCar == null ||
                trafficCar.carTransform == null)
            {
                continue;
            }

            Vector2 carPosition =
                new Vector2(
                    trafficCar.carTransform.position.x,
                    trafficCar.carTransform.position.z
                );

            Vector2 playerPosition =
                new Vector2(
                    player.position.x,
                    player.position.z
                );

            float distanceToPlayer =
                Vector2.Distance(
                    carPosition,
                    playerPosition
                );

            if (distanceToPlayer <= carHitRadius)
            {
                StartCoroutine(
                    HandlePlayerHit()
                );

                return;
            }
        }
    }

    private IEnumerator HandlePlayerHit()
    {
        if (playerRecentlyHit)
            yield break;

        playerRecentlyHit = true;

        RespawnPlayerAtHotel();
        ShowWarning();

        yield return new WaitForSeconds(
            hitCooldown
        );

        playerRecentlyHit = false;
    }

    private void RespawnPlayerAtHotel()
    {
        if (player == null ||
            hotelRespawnPoint == null)
        {
            Debug.LogWarning(
                "Player or Hotel Respawn Point is not assigned."
            );

            return;
        }

        if (playerCharacterController != null)
        {
            playerCharacterController.enabled =
                false;
        }

        player.position =
            hotelRespawnPoint.position;

        player.rotation =
            hotelRespawnPoint.rotation;

        if (playerCharacterController != null)
        {
            playerCharacterController.enabled =
                true;
        }
    }

    private void ShowWarning()
    {
        if (warningText == null)
            return;

        if (warningCoroutine != null)
        {
            StopCoroutine(
                warningCoroutine
            );
        }

        warningCoroutine =
            StartCoroutine(
                ShowWarningRoutine()
            );
    }

    private IEnumerator ShowWarningRoutine()
    {
        warningText.text =
            "CARS KILL! BE CAREFUL!";

        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            warningDuration
        );

        warningText.gameObject.SetActive(false);

        warningCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints != null)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0;
                 i < waypoints.Length;
                 i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawSphere(
                    waypoints[i].position,
                    0.4f
                );

                int nextIndex = i + 1;

                if (nextIndex >=
                    waypoints.Length)
                {
                    nextIndex = 0;
                }

                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        waypoints[nextIndex].position
                    );
                }
            }
        }

        if (trafficCars != null)
        {
            Gizmos.color = Color.red;

            foreach (TrafficCar trafficCar
                     in trafficCars)
            {
                if (trafficCar != null &&
                    trafficCar.carTransform != null)
                {
                    Gizmos.DrawWireSphere(
                        trafficCar.carTransform.position,
                        carHitRadius
                    );
                }
            }
        }
    }
}