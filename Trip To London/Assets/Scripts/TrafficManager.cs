using System.Collections;
using TMPro;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [System.Serializable]
    public class CarSetup
    {
        public Transform car;
        public int startingWaypoint;

        [Range(0.25f, 2f)]
        public float speedMultiplier = 1f;

        [Header("Individual Car Audio")]
        public AudioClip engineSound;
        public AudioClip hornSound;

        [Range(0.5f, 1.5f)]
        public float enginePitch = 1f;

        [Range(0.5f, 1.5f)]
        public float hornPitch = 1f;
    }

    [Header("Existing Waypoints")]
    [SerializeField] private Transform[] waypoints;

    [Header("Traffic Cars")]
    [SerializeField] private CarSetup[] cars;

    [Header("Driving")]
    [SerializeField] private float drivingSpeed = 8f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float waypointReachDistance = 1.5f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float braking = 35f;

    [Header("Obstacle Detection")]
    [SerializeField] private float detectionDistance = 12f;

    [SerializeField]
    private Vector3 detectionBoxSize =
        new Vector3(2.5f, 1.8f, 1.5f);

    [SerializeField]
    private Vector3 detectionOriginOffset =
        new Vector3(0f, 0.8f, 2f);

    [SerializeField] private float stoppingDistance = 4f;

    [Header("Traffic Audio")]
    [Tooltip("Used when an individual car has no engine clip.")]
    [SerializeField] private AudioClip defaultEngineSound;

    [Tooltip("Used when an individual car has no horn clip.")]
    [SerializeField] private AudioClip defaultHornSound;

    [Range(0f, 1f)]
    [SerializeField] private float engineVolume = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float hornVolume = 0.8f;

    [Tooltip("Engine is at full volume inside this distance.")]
    [SerializeField] private float audioMinDistance = 3f;

    [Tooltip("Engine becomes nearly silent at this distance.")]
    [SerializeField] private float audioMaxDistance = 35f;

    [Tooltip("Minimum time before the same car can honk again.")]
    [SerializeField] private float hornCooldown = 4f;

    [Header("Player Warning")]
    [SerializeField] private TMP_Text warningText;

    [SerializeField]
    private string warningMessage =
        "CARS KILL! BE CAREFUL!";

    [SerializeField] private float warningDuration = 2.5f;

    private Coroutine warningCoroutine;

    public Transform[] Waypoints => waypoints;

    public float DrivingSpeed => drivingSpeed;
    public float TurnSpeed => turnSpeed;
    public float WaypointReachDistance => waypointReachDistance;
    public float Acceleration => acceleration;
    public float Braking => braking;

    public float DetectionDistance => detectionDistance;
    public Vector3 DetectionBoxSize => detectionBoxSize;
    public Vector3 DetectionOriginOffset => detectionOriginOffset;
    public float StoppingDistance => stoppingDistance;

    public float EngineVolume => engineVolume;
    public float HornVolume => hornVolume;
    public float AudioMinDistance => audioMinDistance;
    public float AudioMaxDistance => audioMaxDistance;
    public float HornCooldown => hornCooldown;

    private void Awake()
    {
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError(
                "TrafficManager: No waypoints have been assigned.",
                this
            );

            enabled = false;
            return;
        }

        if (cars == null)
        {
            return;
        }

        foreach (CarSetup setup in cars)
        {
            if (setup == null || setup.car == null)
            {
                continue;
            }

            TrafficCar trafficCar =
                setup.car.GetComponent<TrafficCar>();

            if (trafficCar == null)
            {
                trafficCar =
                    setup.car.gameObject.AddComponent<TrafficCar>();
            }

            int startingWaypoint = Mathf.Clamp(
                setup.startingWaypoint,
                0,
                waypoints.Length - 1
            );

            AudioClip selectedEngine =
                setup.engineSound != null
                    ? setup.engineSound
                    : defaultEngineSound;

            AudioClip selectedHorn =
                setup.hornSound != null
                    ? setup.hornSound
                    : defaultHornSound;

            trafficCar.Initialize(
                this,
                startingWaypoint,
                setup.speedMultiplier,
                selectedEngine,
                selectedHorn,
                setup.enginePitch,
                setup.hornPitch
            );
        }
    }

    public void ShowPlayerWarning()
    {
        if (warningText == null)
        {
            return;
        }

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine =
            StartCoroutine(ShowWarningRoutine());
    }

    private IEnumerator ShowWarningRoutine()
    {
        warningText.text = warningMessage;
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningText.gameObject.SetActive(false);
        warningCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        DrawWaypoints();
        DrawDetectionBoxes();
    }

    private void DrawWaypoints()
    {
        if (waypoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                continue;
            }

            Gizmos.DrawSphere(
                waypoints[i].position,
                0.3f
            );

            int nextIndex =
                (i + 1) % waypoints.Length;

            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    waypoints[i].position,
                    waypoints[nextIndex].position
                );
            }
        }
    }

    private void DrawDetectionBoxes()
    {
        if (cars == null)
        {
            return;
        }

        foreach (CarSetup setup in cars)
        {
            if (setup == null || setup.car == null)
            {
                continue;
            }

            Transform car = setup.car;

            Vector3 startPosition =
                car.TransformPoint(detectionOriginOffset);

            Vector3 endPosition =
                startPosition +
                car.forward * detectionDistance;

            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.color = Color.cyan;

            Gizmos.matrix = Matrix4x4.TRS(
                startPosition,
                car.rotation,
                Vector3.one
            );

            Gizmos.DrawWireCube(
                Vector3.zero,
                detectionBoxSize
            );

            Gizmos.matrix = Matrix4x4.TRS(
                endPosition,
                car.rotation,
                Vector3.one
            );

            Gizmos.DrawWireCube(
                Vector3.zero,
                detectionBoxSize
            );

            Gizmos.matrix = oldMatrix;

            Gizmos.DrawLine(
                startPosition,
                endPosition
            );
        }
    }
}