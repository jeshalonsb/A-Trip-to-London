using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [System.Serializable]
    public class CarSetup
    {
        public Transform car;
        public int startingWaypoint;
        [Range(0.25f, 2f)] public float speedMultiplier = 1f;
        public AudioClip engineSound;
    }

    [Header("Keep Your Existing Waypoints Here")]
    [SerializeField] private Transform[] waypoints;

    [Header("Traffic Cars")]
    [SerializeField] private CarSetup[] cars;

    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float waypointDistance = 1.5f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float braking = 12f;

    [Header("Traffic Stopping")]
    [SerializeField] private LayerMask trafficLayer;
    [SerializeField] private float checkDistance = 5f;
    [SerializeField] private float checkWidth = 1.8f;
    [SerializeField] private float checkHeight = 1.5f;

    [Header("Engine Audio")]
    [SerializeField] private AudioClip defaultEngineSound;
    [Range(0f, 1f)]
    [SerializeField] private float engineVolume = 0.4f;
    [SerializeField] private float audioMinDistance = 3f;
    [SerializeField] private float audioMaxDistance = 25f;

    [Header("Player Respawn")]
    [SerializeField] private TrafficPlayerRespawn playerRespawn;
    [SerializeField] private float playerHitDistance = 2f;

    private readonly List<TrafficCar> activeCars = new List<TrafficCar>();

    public Transform[] Waypoints => waypoints;
    public float Speed => speed;
    public float TurnSpeed => turnSpeed;
    public float WaypointDistance => waypointDistance;
    public float Acceleration => acceleration;
    public float Braking => braking;
    public LayerMask TrafficLayer => trafficLayer;
    public float CheckDistance => checkDistance;
    public float CheckWidth => checkWidth;
    public float CheckHeight => checkHeight;
    public float EngineVolume => engineVolume;

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("TrafficManager: Add your existing waypoints.");
            return;
        }

        foreach (CarSetup setup in cars)
        {
            if (setup == null || setup.car == null)
                continue;

            TrafficCar trafficCar = setup.car.GetComponent<TrafficCar>();

            if (trafficCar == null)
                trafficCar = setup.car.gameObject.AddComponent<TrafficCar>();

            AudioClip clip = setup.engineSound != null
                ? setup.engineSound
                : defaultEngineSound;

            trafficCar.Initialize(
                this,
                Mathf.Clamp(setup.startingWaypoint, 0, waypoints.Length - 1),
                setup.speedMultiplier,
                clip,
                audioMinDistance,
                audioMaxDistance
            );

            activeCars.Add(trafficCar);
        }
    }

    private void Update()
    {
        if (playerRespawn == null || playerRespawn.IsRespawning)
            return;

        Transform player = playerRespawn.Player;

        if (player == null)
            return;

        foreach (TrafficCar car in activeCars)
        {
            if (car == null)
                continue;

            Vector2 carPosition = new Vector2(
                car.transform.position.x,
                car.transform.position.z
            );

            Vector2 playerPosition = new Vector2(
                player.position.x,
                player.position.z
            );

            if (Vector2.Distance(carPosition, playerPosition) <= playerHitDistance)
            {
                playerRespawn.Respawn();
                return;
            }
        }
    }

    public bool IsAnotherManagedCar(Transform root, TrafficCar askingCar)
    {
        foreach (TrafficCar car in activeCars)
        {
            if (car != null &&
                car != askingCar &&
                car.transform.root == root)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints != null)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                int next = (i + 1) % waypoints.Length;

                if (waypoints[next] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }

        if (cars == null)
            return;

        foreach (CarSetup setup in cars)
        {
            if (setup == null || setup.car == null)
                continue;

            Transform car = setup.car;

            Vector3 center =
                car.position +
                car.forward * (checkDistance * 0.5f) +
                Vector3.up * (checkHeight * 0.5f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, car.rotation, Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(
                Vector3.zero,
                new Vector3(checkWidth, checkHeight, checkDistance)
            );
            Gizmos.matrix = oldMatrix;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(car.position, playerHitDistance);
        }
    }
}
