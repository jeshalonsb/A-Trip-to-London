using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    [Header("Pileup Reset")]
    [SerializeField] private float stuckSpeedThreshold = 0.2f;
    [SerializeField] private float stuckResetTime = 6f;
    [SerializeField] private int resetWaypointOffset = 2;
    [SerializeField] private float resetClearanceRadius = 2.5f;

    private TrafficManager manager;
    private Rigidbody carRigidbody;

    private AudioSource engineAudioSource;
    private AudioSource hornAudioSource;

    private AudioClip engineClip;
    private AudioClip hornClip;

    private float baseEnginePitch = 1f;
    private float hornPitch = 1f;
    private float lastHornTime = -100f;

    private int waypointIndex;
    private float speedMultiplier = 1f;
    private float currentSpeed;
    private float stuckTimer;

    private bool initialized;
    private bool playerWasDetected;
    private bool currentlyBlocked;
    private bool wasBraking;
    private bool isResetting;

    public void Initialize(
        TrafficManager trafficManager,
        int startingWaypoint,
        float multiplier,
        AudioClip selectedEngineClip,
        AudioClip selectedHornClip,
        float selectedEnginePitch,
        float selectedHornPitch)
    {
        manager = trafficManager;
        waypointIndex = startingWaypoint;
        speedMultiplier = Mathf.Max(0.1f, multiplier);

        engineClip = selectedEngineClip;
        hornClip = selectedHornClip;

        baseEnginePitch =
            Mathf.Clamp(selectedEnginePitch, 0.5f, 1.5f);

        hornPitch =
            Mathf.Clamp(selectedHornPitch, 0.5f, 1.5f);

        currentSpeed =
            manager.DrivingSpeed * speedMultiplier;

        SetupRigidbody();
        SetupAudio();

        initialized = true;
    }

    private void SetupRigidbody()
    {
        carRigidbody = GetComponent<Rigidbody>();

        if (carRigidbody == null)
        {
            carRigidbody =
                gameObject.AddComponent<Rigidbody>();
        }

        carRigidbody.isKinematic = true;
        carRigidbody.useGravity = false;

        carRigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        carRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    private void SetupAudio()
    {
        AudioSource[] existingSources =
            GetComponents<AudioSource>();

        if (existingSources.Length > 0)
        {
            engineAudioSource = existingSources[0];
        }
        else
        {
            engineAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        if (existingSources.Length > 1)
        {
            hornAudioSource = existingSources[1];
        }
        else
        {
            hornAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        Configure3DAudioSource(engineAudioSource);
        Configure3DAudioSource(hornAudioSource);

        engineAudioSource.clip = engineClip;
        engineAudioSource.loop = true;
        engineAudioSource.playOnAwake = false;
        engineAudioSource.volume = manager.EngineVolume;
        engineAudioSource.pitch = baseEnginePitch;

        hornAudioSource.clip = hornClip;
        hornAudioSource.loop = false;
        hornAudioSource.playOnAwake = false;
        hornAudioSource.volume = manager.HornVolume;
        hornAudioSource.pitch = hornPitch;

        if (engineClip != null)
        {
            engineAudioSource.Play();
        }
    }

    private void Configure3DAudioSource(
        AudioSource source)
    {
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.minDistance =
            manager.AudioMinDistance;

        source.maxDistance =
            manager.AudioMaxDistance;

        source.dopplerLevel = 0.25f;
    }

    private void FixedUpdate()
    {
        if (!initialized ||
            manager == null ||
            manager.Waypoints == null ||
            manager.Waypoints.Length == 0)
        {
            return;
        }

        if (isResetting)
        {
            return;
        }

        HandleDriving();
        CheckForPileupReset();
        UpdateEngineSound();
    }

    private void HandleDriving()
    {
        Transform targetWaypoint =
            manager.Waypoints[waypointIndex];

        if (targetWaypoint == null)
        {
            AdvanceWaypoint();
            return;
        }

        ObstacleInformation obstacle =
            CheckForObstacle();

        currentlyBlocked =
            obstacle.obstacleDetected;

        float maximumSpeed =
            manager.DrivingSpeed * speedMultiplier;

        float targetSpeed =
            CalculateTargetSpeed(
                maximumSpeed,
                obstacle
            );

        bool brakingNow =
            obstacle.obstacleDetected &&
            targetSpeed < currentSpeed - 0.1f;

        HandleBrakeHorn(brakingNow);

        float speedChangeRate =
            targetSpeed < currentSpeed
                ? manager.Braking
                : manager.Acceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            speedChangeRate * Time.fixedDeltaTime
        );

        MoveTowardWaypoint(
            targetWaypoint,
            currentSpeed
        );

        HandlePlayerWarning(
            obstacle.playerDetected
        );
    }

    private void HandleBrakeHorn(
        bool brakingNow)
    {
        bool justStartedBraking =
            brakingNow && !wasBraking;

        bool cooldownFinished =
            Time.time >=
            lastHornTime + manager.HornCooldown;

        if (justStartedBraking &&
            cooldownFinished)
        {
            PlayHorn();
        }

        wasBraking = brakingNow;
    }

    private void PlayHorn()
    {
        if (hornAudioSource == null ||
            hornClip == null)
        {
            return;
        }

        hornAudioSource.pitch = hornPitch;
        hornAudioSource.PlayOneShot(
            hornClip,
            manager.HornVolume
        );

        lastHornTime = Time.time;
    }

    private void UpdateEngineSound()
    {
        if (engineAudioSource == null ||
            engineClip == null)
        {
            return;
        }

        float maximumSpeed =
            manager.DrivingSpeed * speedMultiplier;

        float normalizedSpeed =
            maximumSpeed > 0f
                ? Mathf.Clamp01(
                    currentSpeed / maximumSpeed
                )
                : 0f;

        engineAudioSource.pitch =
            Mathf.Lerp(
                baseEnginePitch * 0.75f,
                baseEnginePitch * 1.15f,
                normalizedSpeed
            );

        engineAudioSource.volume =
            Mathf.Lerp(
                manager.EngineVolume * 0.55f,
                manager.EngineVolume,
                normalizedSpeed
            );
    }

    private float CalculateTargetSpeed(
        float maximumSpeed,
        ObstacleInformation obstacle)
    {
        if (!obstacle.obstacleDetected)
        {
            return maximumSpeed;
        }

        if (obstacle.distance <=
            manager.StoppingDistance)
        {
            return 0f;
        }

        float usableDistance =
            manager.DetectionDistance -
            manager.StoppingDistance;

        if (usableDistance <= 0f)
        {
            return 0f;
        }

        float speedPercentage =
            Mathf.InverseLerp(
                manager.StoppingDistance,
                manager.DetectionDistance,
                obstacle.distance
            );

        return maximumSpeed *
               speedPercentage;
    }

    private ObstacleInformation CheckForObstacle()
    {
        ObstacleInformation information =
            new ObstacleInformation
            {
                obstacleDetected = false,
                playerDetected = false,
                distance = manager.DetectionDistance
            };

        Vector3 origin =
            transform.TransformPoint(
                manager.DetectionOriginOffset
            );

        Vector3 halfExtents =
            manager.DetectionBoxSize * 0.5f;

        RaycastHit[] hits =
            Physics.BoxCastAll(
                origin,
                halfExtents,
                transform.forward,
                transform.rotation,
                manager.DetectionDistance,
                ~0,
                QueryTriggerInteraction.Collide
            );

        foreach (RaycastHit hit in hits)
        {
            Collider hitCollider = hit.collider;

            if (hitCollider == null ||
                BelongsToThisCar(hitCollider))
            {
                continue;
            }

            bool isPlayer =
                IsPlayer(hitCollider);

            bool isObstacle =
                IsAnotherTrafficCar(hitCollider) ||
                IsMarkedTrafficObstacle(hitCollider);

            if (!isPlayer && !isObstacle)
            {
                continue;
            }

            information.obstacleDetected = true;

            if (isPlayer)
            {
                information.playerDetected = true;
            }

            if (hit.distance < information.distance)
            {
                information.distance = hit.distance;
            }
        }

        CheckStartingBoxOverlap(
            origin,
            halfExtents,
            ref information
        );

        return information;
    }

    private void CheckStartingBoxOverlap(
        Vector3 origin,
        Vector3 halfExtents,
        ref ObstacleInformation information)
    {
        Collider[] overlaps =
            Physics.OverlapBox(
                origin,
                halfExtents,
                transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide
            );

        foreach (Collider overlap in overlaps)
        {
            if (overlap == null ||
                BelongsToThisCar(overlap))
            {
                continue;
            }

            bool isPlayer =
                IsPlayer(overlap);

            bool isObstacle =
                IsAnotherTrafficCar(overlap) ||
                IsMarkedTrafficObstacle(overlap);

            if (!isPlayer && !isObstacle)
            {
                continue;
            }

            information.obstacleDetected = true;
            information.distance = 0f;

            if (isPlayer)
            {
                information.playerDetected = true;
            }
        }
    }

    private bool BelongsToThisCar(
        Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return other.transform == transform ||
               other.transform.IsChildOf(transform);
    }

    private bool IsPlayer(
        Collider other)
    {
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool IsAnotherTrafficCar(
        Collider other)
    {
        TrafficCar otherCar =
            other.GetComponentInParent<TrafficCar>();

        return otherCar != null &&
               otherCar != this;
    }

    private bool IsMarkedTrafficObstacle(
        Collider other)
    {
        TrafficObstacle obstacle =
            other.GetComponentInParent<TrafficObstacle>();

        return obstacle != null;
    }

    private void HandlePlayerWarning(
        bool playerDetected)
    {
        if (playerDetected &&
            !playerWasDetected)
        {
            manager.ShowPlayerWarning();
        }

        playerWasDetected = playerDetected;
    }

    private void MoveTowardWaypoint(
        Transform targetWaypoint,
        float movementSpeed)
    {
        Vector3 targetPosition =
            targetWaypoint.position;

        targetPosition.y =
            transform.position.y;

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        Quaternion nextRotation =
            transform.rotation;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            nextRotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    manager.TurnSpeed *
                    Time.fixedDeltaTime
                );
        }

        Vector3 nextPosition =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed *
                Time.fixedDeltaTime
            );

        carRigidbody.MoveRotation(nextRotation);
        carRigidbody.MovePosition(nextPosition);

        CheckWaypointDistance(
            nextPosition,
            targetPosition
        );
    }

    private void CheckWaypointDistance(
        Vector3 carPosition,
        Vector3 waypointPosition)
    {
        Vector2 flatCarPosition =
            new Vector2(
                carPosition.x,
                carPosition.z
            );

        Vector2 flatWaypointPosition =
            new Vector2(
                waypointPosition.x,
                waypointPosition.z
            );

        if (Vector2.Distance(
                flatCarPosition,
                flatWaypointPosition)
            <= manager.WaypointReachDistance)
        {
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >=
            manager.Waypoints.Length)
        {
            waypointIndex = 0;
        }
    }

    private void CheckForPileupReset()
    {
        bool stuck =
            currentSpeed <= stuckSpeedThreshold &&
            currentlyBlocked;

        if (stuck)
        {
            stuckTimer += Time.fixedDeltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        if (stuckTimer >= stuckResetTime)
        {
            ResetCarToClearWaypoint();
        }
    }

    private void ResetCarToClearWaypoint()
    {
        if (manager == null ||
            manager.Waypoints == null ||
            manager.Waypoints.Length == 0)
        {
            return;
        }

        isResetting = true;
        stuckTimer = 0f;

        int waypointCount =
            manager.Waypoints.Length;

        for (int attempt = 1;
             attempt <= waypointCount;
             attempt++)
        {
            int offset =
                resetWaypointOffset +
                attempt - 1;

            int candidateIndex =
                (waypointIndex + offset) %
                waypointCount;

            Transform candidateWaypoint =
                manager.Waypoints[candidateIndex];

            if (candidateWaypoint == null)
            {
                continue;
            }

            if (!IsResetPositionClear(
                candidateWaypoint.position))
            {
                continue;
            }

            waypointIndex = candidateIndex;

            TeleportToWaypoint(
                candidateWaypoint
            );

            isResetting = false;
            return;
        }

        Debug.LogWarning(
            name +
            " could not find a clear waypoint to reset to.",
            this
        );

        isResetting = false;
    }

    private bool IsResetPositionClear(
        Vector3 resetPosition)
    {
        Collider[] overlaps =
            Physics.OverlapSphere(
                resetPosition,
                resetClearanceRadius,
                ~0,
                QueryTriggerInteraction.Collide
            );

        foreach (Collider overlap in overlaps)
        {
            if (overlap == null ||
                BelongsToThisCar(overlap))
            {
                continue;
            }

            if (IsAnotherTrafficCar(overlap) ||
                IsMarkedTrafficObstacle(overlap) ||
                IsPlayer(overlap))
            {
                return false;
            }
        }

        return true;
    }

    private void TeleportToWaypoint(
        Transform resetWaypoint)
    {
        Vector3 resetPosition =
            resetWaypoint.position;

        Quaternion resetRotation =
            resetWaypoint.rotation;

        carRigidbody.position = resetPosition;
        carRigidbody.rotation = resetRotation;

        transform.position = resetPosition;
        transform.rotation = resetRotation;

        currentSpeed = 0f;
        currentlyBlocked = false;
        playerWasDetected = false;
        wasBraking = false;

        Physics.SyncTransforms();
    }

    private struct ObstacleInformation
    {
        public bool obstacleDetected;
        public bool playerDetected;
        public float distance;
    }
}