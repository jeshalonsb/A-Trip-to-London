using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]

#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif

    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required before being able to jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required before entering the fall state")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("Radius of the grounded check")]
        public float GroundedRadius = 0.28f;

        [Tooltip("Layers used as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("Target followed by the Cinemachine camera")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("Maximum upward camera angle")]
        public float TopClamp = 70.0f;

        [Tooltip("Maximum downward camera angle")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional camera angle")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("Locks the camera position")]
        public bool LockCameraPosition = false;

        [Header("Camera Sensitivity")]
        [Range(0.1f, 5f)]
        public float LookSensitivity = 1f;

        [Header("Animation")]
        [Tooltip("Drag the low-poly character object with the Animator here")]
        [SerializeField] private Animator playerAnimator;

        [Header("Bus Riding")]
        [Tooltip("True while the player is riding the bus")]
        [SerializeField] private bool isRidingBus;

        // Cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // Player movement
        private float _speed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;

        private readonly float _terminalVelocity = 53.0f;

        // Timeout delta time
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Animation parameter IDs
        private int _animIDWalking;
        private int _animIDRunning;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        public bool IsRidingBus => isRidingBus;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera =
                    GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            if (CinemachineCameraTarget != null)
            {
                _cinemachineTargetYaw =
                    CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            }

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. " +
                "Use Tools/Starter Assets/Reinstall Dependencies."
            );
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            SetStandingAnimation();
        }

        private void Update()
        {
            // While riding the bus, player movement and gravity are paused.
            // Camera movement still works in LateUpdate.
            if (isRidingBus)
            {
                ClearMovementInput();
                SetStandingAnimation();
                return;
            }

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        public void SetBusRiding(bool riding)
        {
            isRidingBus = riding;

            ClearMovementInput();

            if (riding)
            {
                _speed = 0f;
                _verticalVelocity = 0f;

                SetStandingAnimation();
            }
        }

        private void ClearMovementInput()
        {
            if (_input == null)
            {
                return;
            }

            _input.move = Vector2.zero;
            _input.sprint = false;
            _input.jump = false;
        }

        private void AssignAnimationIDs()
        {
            _animIDWalking =
                Animator.StringToHash("Walking");

            _animIDRunning =
                Animator.StringToHash("Running");

            _animIDGrounded =
                Animator.StringToHash("Grounded");

            _animIDJump =
                Animator.StringToHash("Jump");

            _animIDFreeFall =
                Animator.StringToHash("FreeFall");
        }

        private void SetStandingAnimation()
        {
            if (playerAnimator == null)
            {
                return;
            }

            playerAnimator.SetBool(
                _animIDWalking,
                false
            );

            playerAnimator.SetBool(
                _animIDRunning,
                false
            );

            playerAnimator.SetBool(
                _animIDJump,
                false
            );

            playerAnimator.SetBool(
                _animIDFreeFall,
                false
            );
        }

        private void UpdateMovementAnimation()
        {
            if (playerAnimator == null ||
                _input == null)
            {
                return;
            }

            bool isMoving =
                _input.move.sqrMagnitude > 0.01f;

            bool isRunning =
                isMoving && _input.sprint;

            bool isWalking =
                isMoving && !_input.sprint;

            playerAnimator.SetBool(
                _animIDWalking,
                isWalking
            );

            playerAnimator.SetBool(
                _animIDRunning,
                isRunning
            );
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition =
                new Vector3(
                    transform.position.x,
                    transform.position.y - GroundedOffset,
                    transform.position.z
                );

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (playerAnimator != null)
            {
                playerAnimator.SetBool(
                    _animIDGrounded,
                    Grounded
                );
            }
        }

        private void CameraRotation()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }
            
            
            if (_input == null ||
                CinemachineCameraTarget == null)
            {
                return;
            }

            if (_input.look.sqrMagnitude >= _threshold &&
                !LockCameraPosition)
            {
                float deltaTimeMultiplier =
                    IsCurrentDeviceMouse
                        ? 1.0f
                        : Time.deltaTime;

                _cinemachineTargetYaw +=
                    _input.look.x *
                    deltaTimeMultiplier *
                    LookSensitivity;

                _cinemachineTargetPitch +=
                    _input.look.y *
                    deltaTimeMultiplier *
                    LookSensitivity;
            }

            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue
                );

            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch +
                    CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0.0f
                );
        }

        private void Move()
        {
            if (_input == null ||
                _controller == null)
            {
                return;
            }

            float targetSpeed =
                _input.sprint
                    ? SprintSpeed
                    : MoveSpeed;

            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;

            float speedOffset = 0.1f;

            float inputMagnitude =
                _input.analogMovement
                    ? _input.move.magnitude
                    : 1f;

            if (currentHorizontalSpeed <
                    targetSpeed - speedOffset ||
                currentHorizontalSpeed >
                    targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime *
                    SpeedChangeRate
                );

                _speed =
                    Mathf.Round(
                        _speed * 1000f
                    ) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection =
                new Vector3(
                    _input.move.x,
                    0.0f,
                    _input.move.y
                ).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation =
                    Mathf.Atan2(
                        inputDirection.x,
                        inputDirection.z
                    ) *
                    Mathf.Rad2Deg +
                    _mainCamera.transform.eulerAngles.y;

                float rotation =
                    Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        _targetRotation,
                        ref _rotationVelocity,
                        RotationSmoothTime
                    );

                transform.rotation =
                    Quaternion.Euler(
                        0.0f,
                        rotation,
                        0.0f
                    );
            }

            Vector3 targetDirection =
                Quaternion.Euler(
                    0.0f,
                    _targetRotation,
                    0.0f
                ) *
                Vector3.forward;

            _controller.Move(
                targetDirection.normalized *
                (_speed * Time.deltaTime) +
                new Vector3(
                    0.0f,
                    _verticalVelocity,
                    0.0f
                ) *
                Time.deltaTime
            );

            UpdateMovementAnimation();
        }

        private void JumpAndGravity()
        {
            if (_input == null)
            {
                return;
            }

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool(
                        _animIDJump,
                        false
                    );

                    playerAnimator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump &&
                    _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );

                    if (playerAnimator != null)
                    {
                        playerAnimator.SetBool(
                            _animIDJump,
                            true
                        );
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else if (playerAnimator != null)
                {
                    playerAnimator.SetBool(
                        _animIDFreeFall,
                        true
                    );
                }

                _input.jump = false;
            }

            if (_verticalVelocity <
                _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity *
                    Time.deltaTime;
            }
        }

        private static float ClampAngle(
            float angle,
            float minimum,
            float maximum)
        {
            if (angle < -360f)
            {
                angle += 360f;
            }

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return Mathf.Clamp(
                angle,
                minimum,
                maximum
            );
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );

            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );

            Gizmos.color =
                Grounded
                    ? transparentGreen
                    : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y -
                    GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }

        private void OnFootstep(
            AnimationEvent animationEvent)
        {
            if (animationEvent
                    .animatorClipInfo
                    .weight <= 0.5f)
            {
                return;
            }

            if (FootstepAudioClips == null ||
                FootstepAudioClips.Length == 0)
            {
                return;
            }

            int index =
                Random.Range(
                    0,
                    FootstepAudioClips.Length
                );

            AudioSource.PlayClipAtPoint(
                FootstepAudioClips[index],
                transform.TransformPoint(
                    _controller.center
                ),
                FootstepAudioVolume
            );
        }

        private void OnLand(
            AnimationEvent animationEvent)
        {
            if (animationEvent
                    .animatorClipInfo
                    .weight <= 0.5f)
            {
                return;
            }

            if (LandingAudioClip == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(
                LandingAudioClip,
                transform.TransformPoint(
                    _controller.center
                ),
                FootstepAudioVolume
            );
        }
    }
}