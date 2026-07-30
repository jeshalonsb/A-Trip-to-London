using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    private TrafficManager manager;
    private Rigidbody body;
    private AudioSource engine;

    private int waypointIndex;
    private float speedMultiplier;
    private float currentSpeed;
    private bool ready;

    public void Initialize(
        TrafficManager trafficManager,
        int startingWaypoint,
        float multiplier,
        AudioClip engineClip,
        float minAudioDistance,
        float maxAudioDistance)
    {
        manager = trafficManager;
        waypointIndex = startingWaypoint;
        speedMultiplier = Mathf.Max(0.1f, multiplier);
        currentSpeed = manager.Speed * speedMultiplier;

        body = GetComponent<Rigidbody>();

        if (body == null)
            body = gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        if (engineClip != null)
        {
            engine = GetComponent<AudioSource>();

            if (engine == null)
                engine = gameObject.AddComponent<AudioSource>();

            engine.clip = engineClip;
            engine.loop = true;
            engine.playOnAwake = false;
            engine.spatialBlend = 1f;
            engine.volume = manager.EngineVolume;
            engine.minDistance = minAudioDistance;
            engine.maxDistance = maxAudioDistance;
            engine.Play();
        }

        ready = true;
    }

    private void FixedUpdate()
    {
        if (!ready || manager.Waypoints.Length == 0)
            return;

        Transform target = manager.Waypoints[waypointIndex];

        if (target == null)
        {
            NextWaypoint();
            return;
        }

        bool blocked = TrafficAhead();

        float normalSpeed = manager.Speed * speedMultiplier;
        float targetSpeed = blocked ? 0f : normalSpeed;
        float changeRate = blocked ? manager.Braking : manager.Acceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            changeRate * Time.fixedDeltaTime
        );

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentSpeed * Time.fixedDeltaTime
        );

        Quaternion newRotation = transform.rotation;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            newRotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                manager.TurnSpeed * Time.fixedDeltaTime
            );
        }

        body.MovePosition(newPosition);
        body.MoveRotation(newRotation);

        UpdateEngine(normalSpeed);

        Vector2 flatCar = new Vector2(newPosition.x, newPosition.z);
        Vector2 flatTarget = new Vector2(targetPosition.x, targetPosition.z);

        if (Vector2.Distance(flatCar, flatTarget) <= manager.WaypointDistance)
            NextWaypoint();
    }

    private bool TrafficAhead()
    {
        Vector3 center =
            transform.position +
            transform.forward * (manager.CheckDistance * 0.5f) +
            Vector3.up * (manager.CheckHeight * 0.5f);

        Vector3 halfSize = new Vector3(
            manager.CheckWidth * 0.5f,
            manager.CheckHeight * 0.5f,
            manager.CheckDistance * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfSize,
            transform.rotation,
            manager.TrafficLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            if (hit == null || hit.transform.root == transform.root)
                continue;

            if (manager.IsAnotherManagedCar(hit.transform.root, this))
                return true;
        }

        return false;
    }

    private void UpdateEngine(float normalSpeed)
    {
        if (engine == null)
            return;

        float amount = normalSpeed > 0f
            ? currentSpeed / normalSpeed
            : 0f;

        engine.volume = Mathf.Lerp(
            manager.EngineVolume * 0.35f,
            manager.EngineVolume,
            amount
        );

        engine.pitch = Mathf.Lerp(0.8f, 1f, amount);
    }

    private void NextWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >= manager.Waypoints.Length)
            waypointIndex = 0;
    }
}
