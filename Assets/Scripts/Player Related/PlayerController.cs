using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Sjouke
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        private enum LockOnStatus { None, Transform, Position }

        ///// Public Unity initialized \\\\\
        [Header("References")]

        [Tooltip("Reference to the camera attached to the Player object.")]
        public Camera PlayerCamera;
        public InputHandler InputHandler;
        public UpdateManager UpdateManager;

        [Header("Movement Values")]

        [Tooltip("The speed at which the player moves.")]
        public float MovementSpeed;
        public bool UseAcceleration;
        [Tooltip("Rate of acceleration and deceleration when the player moves."), /*Min(0),*/ Range(0, 1), EnableIf(nameof(UseAcceleration))]
        public float Acceleration = 0.5f;
        [Tooltip("Whether to clamp the player's maximum velocity to a specific value.")]
        public bool ClampVelocity;
        [Tooltip("The max magnitude for player movement."), EnableIf(nameof(ClampVelocity))]
        public float MaxVelocityMagnitude;
        public bool UseSlopeModifier;
        [EnableIf(nameof(UseSlopeModifier))]
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

        [Header("Camera Settings")]

        [Tooltip("The rate at which the camera will turn relative to the input.")]
        public float TurnSpeed;
        [Tooltip("Inverse the X axis of camera input.")]
        public bool InverseX;
        [Tooltip("Inverse the Y axis of camera input.")]
        public bool InverseY;
        public bool UseRotationSmoothing;
        [Tooltip("The amount of smoothing used while rotating the player camera."), Min(0), EnableIf(nameof(UseRotationSmoothing))]
        public float SmoothTurnSpeed;
        [Tooltip("The highest angle at which the player can look up in degrees. (270 is straight upward)"), Range(270, 359)]
        public int MaxCameraAngle = 270;
        [Tooltip("The lowest angle at which the player can look down in degrees. (90 is straight downard)"), Range(0, 90)]
        public int MinCameraAngle = 90;

        public bool UseHeadbob;
        [EnableIf(nameof(UseHeadbob))]
        public Transform TransformToBob;
        [EnableIf(nameof(UseHeadbob))]
        public float HeadBobBaseInterval;
        [EnableIf(nameof(UseHeadbob))]
        public CurveControlledBob HeadbobSettings;

        [Header("Jump Settings")]

        [Tooltip("The amount of force applied to the player when performing a jump.")]
        public float JumpForce = 10;
        [Tooltip("The ForceMode used for the jump.")]
        public ForceMode JumpForceMode = ForceMode.VelocityChange;
        [Tooltip("The distance from the player object's point of center at which will be checked for ground. \nSetting this too small might result in irregular positives, while too high might feel like you can jump without touching any ground.")]
        public float JumpLandCheckDistance;
        public float JumpCheckOffset;
        [Tooltip("The radius of the jump check.\nSetting this too small might result in irregular positives when trying to jump on narrow surfaces.")]
        public float JumpLandCheckRadius;
        [Tooltip("The layers on which the player can jump.")] // ReSharper disable once IdentifierTypo
        public LayerMask JumpableLayers;
        [Tooltip("The amount of time repeated jump inputs will be ignored, after a grounded jump has been performed.")]
        public float JumpCooldown;
        [Tooltip("The amount of times the player can jump without touching any ground."), Range(1, 3)]
        public int AmountOfJumps;

        public delegate void PlayerJumpEvent();
        public delegate void PlayerLockEvent(bool state);
        public event PlayerJumpEvent onPlayerJump;
        public event PlayerLockEvent onPlayerLock;

        [Header("Advanced"), SerializeField]
        private ushort _rigidbodySolverIterations = 10;

        private Transform _playerCamTransform;
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private bool _useUpdateLoop;

        // Input related
        private Vector2 _movementInput;
        private bool _activeInput;
        private Vector2 _lookInput;
        // private Queue<Vector2> _movementInputBuffer; // Required if FixedUpdate is not linked to Update

        // Movement related
        private Vector2 _currentMovementVector;
        private double _currentAcceleration;

        // Jump related
        private bool _jumpInput;
        private bool _isJumping;
        private bool _isGrounded;
        private Vector3 _groundNormal;
        // private float _currentJumpPull;
        private double _jumpCooldownTimer;
        private int _timesJumped;

        // Jump check related
        private RaycastHit[] _nonAllocBuffer;

        // Look lock related
        private LockOnStatus _lockOnStatus;
        private Transform _lockOnTransform;
        private Vector3 _lockOnPosition = Vector3.zero;
        private List<Targetable> _availableTargets;
        //TODO: Keep a list of all lockable targets currently present in view

        private void Reset()
        {
            _rb = GetComponent<Rigidbody>();
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, PlayerController>(_rb, this, gameObject);
#endif
            _collider = GetComponentInChildren<CapsuleCollider>();
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<CapsuleCollider, PlayerController>(_collider, this, gameObject);
#endif
        }

        private void Awake()
        {
            InitializeControls();

            _playerCamTransform = PlayerCamera.transform;
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<Transform, PlayerController>(_playerCamTransform, this, gameObject);
#endif
            _useUpdateLoop = UpdateManager.UpdateLoop == UpdateManager.UpdateType.Update;
#if UNITY_EDITOR
            // DebugUtility.HandleErrorIfNullGetComponent<UpdateManager, PlayerController>(UpdateManager, this, gameObject);
            DebugUtility.HandleErrorIfNullFindObject<UpdateManager, PlayerController>(gameObject, this);
#endif
        }

        private void InitializeControls()
        {
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, PlayerController>(InputHandler, this, gameObject);
#endif
            PlayerControls controls = InputHandler.Controls;

            controls.Player.Move.performed += context => OnPlayerMove(true, context.ReadValue<Vector2>());
            controls.Player.Move.canceled += context => OnPlayerMove(false);
            controls.Player.Look.performed += context => OnPlayerLook(context.ReadValue<Vector2>());
            controls.Player.Look.canceled += context => OnPlayerLook(Vector2.zero);
            controls.Player.Jump.performed += context => OnPlayerJump();
            // controls.Player.Lock.performed += context => OnPlayerLockOn();
            controls.Player.Lock.canceled += context => RemoveLockOn();
        }

        private void Start()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, PlayerController>(_rb, this, gameObject);
#endif

            if (InputHandler == null)
                InputHandler = FindObjectOfType<InputHandler>();

#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, PlayerController>(InputHandler, this, gameObject);
#endif

            if (_collider == null)
                _collider = GetComponentInChildren<CapsuleCollider>();

#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<CapsuleCollider, PlayerController>(_collider, this, gameObject);
#endif

            _nonAllocBuffer = new RaycastHit[10];

            _availableTargets = new List<Targetable>(10);

            _rb.solverIterations = _rigidbodySolverIterations;

            // HeadbobSettings.Setup(PlayerCamera, HeadBobBaseInterval); // TODO: Implement HeadBob
        }

        private void OnEnable() => InputHandler.Controls.Player.Enable();

        private void OnDisable() => InputHandler.Controls.Player.Disable();

        private void Update()
        {
            _useUpdateLoop = UpdateManager.UpdateLoop == UpdateManager.UpdateType.Update;

            if (!_useUpdateLoop)
                return;

            UpdateMovement();

            if (_jumpCooldownTimer < 0)
                CheckForGround();

            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= Time.deltaTime;

            CheckForJump();
        }

        private void LateUpdate() => UpdateRotations();

        private void FixedUpdate()
        {
            if (_useUpdateLoop)
                return;

            UpdateMovement();

            if (_jumpCooldownTimer < 0)
                CheckForGround();

            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= Time.deltaTime;

            CheckForJump();
        }

        // private void StickToGroundHelper() // TODO: See if this StickToGroundHelper might be useful
        // {
        //     if (!Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out RaycastHit hitInfo, ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        //         return;
        //
        //     if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) > 85f)
        //         return;
        //
        //     m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
        // }

        private void UpdateMovement()
        {
            float deltaTime = Time.deltaTime;
            Vector2 newMovementVector = _movementInput * (MovementSpeed / 100); // Divide by 100 to allow greater numbers in editor

            if (UseAcceleration)
            {
                newMovementVector = Vector2.Lerp(_currentMovementVector, newMovementVector, Mathf.SmoothStep(0, 1, Acceleration));

                if (_activeInput && _currentAcceleration < 1)
                    _currentAcceleration += deltaTime * Acceleration;
                if (!_activeInput && _currentAcceleration > 0)
                    _currentAcceleration -= deltaTime * Acceleration;
            }

            // if (UseHeadbob)
            // {
            //     HeadbobSettings.DoHeadBob(newMovementVector.magnitude);
            // }

            Mathf.Clamp((float)_currentAcceleration, 0, 1);

            Vector3 newVelocity = transform.TransformDirection(new Vector3(newMovementVector.x, 0, newMovementVector.y));

            if (ClampVelocity)
                Vector3.ClampMagnitude(newVelocity, MaxVelocityMagnitude);

            _rb.MovePosition(_rb.position + (newVelocity * SlopeMultiplier()) * deltaTime);

            _currentMovementVector = newMovementVector;
        }

        private float SlopeMultiplier() => SlopeCurveModifier.Evaluate(Vector3.Angle(_isGrounded ? _groundNormal : Vector3.up, Vector3.up));

        private void CheckForJump()
        {
            if (_jumpInput && _jumpCooldownTimer <= 0 && (!_isJumping || _isJumping && _timesJumped < AmountOfJumps))
                PerformJump();

            _jumpInput = false;
        }

        private void CheckForGround()
        {
#if UNITY_EDITOR
            // Debug.DrawRay(_rb.position + new Vector3(0, JumpCheckOffset, 0), Vector3.down * JumpLandCheckDistance, Color.magenta, 1);
#endif

            // bool grounded = Physics.SphereCastNonAlloc(transform.position, JumpLandCheckRadius, Vector3.down, _nonAllocBuffer, JumpLandCheckDistance, JumpableLayers) > 0;
            int hits = Physics.SphereCastNonAlloc(transform.position,
                                                  JumpLandCheckRadius,
                                                  Vector3.down,
                                                  _nonAllocBuffer,
                                                  JumpLandCheckDistance,
                                                  JumpableLayers,
                                                  QueryTriggerInteraction.Ignore);

            if (hits == 0)
            {
                _isGrounded = false;
                return;
            }

            if (!_isGrounded && hits > 0)
                OnPlayerGrounded();

            RaycastHit nearestHit = _nonAllocBuffer[0];
            float nearestDistance = Vector3.Distance(nearestHit.point, transform.position);

            if (hits > 1)
            {
                for (int index = 1; index < hits; index++)
                {
                    float newDistance = Vector3.Distance(_nonAllocBuffer[index].point, transform.position);
                    if (newDistance < nearestDistance)
                    {
                        nearestHit = _nonAllocBuffer[index];
                        nearestDistance = newDistance;
                    }
                }
            }

            _groundNormal = nearestHit.normal;
        }

        private void PerformJump()
        {
            _isGrounded = false;
            _isJumping = true;
            _jumpCooldownTimer = JumpCooldown;
            _timesJumped++;

            Vector3 velocity = _rb.velocity;
            _rb.velocity = new Vector3(velocity.x, 0, velocity.z);
            // _currentMovementVector = Vector2.zero;
            // _currentAcceleration = 0;

            _rb.AddForce(Vector3.up * JumpForce, JumpForceMode);

            onPlayerJump?.Invoke();
        }

        private void UpdateRotations()
        {
            float newRotation;
            float newCameraRotation;

            if (_lockOnStatus == LockOnStatus.None)
            {
                float deltaTime = Time.deltaTime;
                Vector2 newLookVector = _lookInput * TurnSpeed;

                newRotation = transform.rotation.eulerAngles.y + (InverseX ? -newLookVector.x : newLookVector.x) * deltaTime;
                newCameraRotation = _playerCamTransform.localRotation.eulerAngles.x - (InverseY ? -newLookVector.y : newLookVector.y) * deltaTime;
            }
            else
            {
                Vector3 targetPos = _lockOnStatus == LockOnStatus.Transform ? _lockOnTransform.position : _lockOnPosition;
                Vector3 targetDir = (targetPos - new Vector3(transform.position.x, _playerCamTransform.position.y, transform.position.z)).normalized;

                newRotation = targetDir.y;
                newCameraRotation = targetDir.x;

                // Debug.DrawLine(Vector3.zero, new Vector3(transform.position.x, _playerCamTransform.position.y, transform.position.z).normalized, Color.magenta);
                Debug.DrawRay(targetDir, -targetDir, Color.magenta);
            }

            if (newCameraRotation < 180 && newCameraRotation > MinCameraAngle)
            {
                newCameraRotation = MinCameraAngle;
                RemoveLockOn();
            }

            if (newCameraRotation > 180 && newCameraRotation < MaxCameraAngle)
            {
                newCameraRotation = MaxCameraAngle;
                RemoveLockOn();
            }

            transform.rotation = Quaternion.Euler(0, newRotation, 0);
            _playerCamTransform.localRotation = Quaternion.Euler(newCameraRotation, 0, 0);
        }

        public void LookAt(Transform lockTransform)
        {
            _lockOnStatus = LockOnStatus.Transform;
            _lockOnTransform = lockTransform;
        }

        public void LookAt(Vector3 lockPosition)
        {
            _lockOnStatus = LockOnStatus.Position;
            _lockOnPosition = lockPosition;
        }

        public void RemoveLockOn()
        {
            _lockOnStatus = LockOnStatus.None;
            _lockOnPosition = Vector3.zero;
            _lockOnTransform = null;

            onPlayerLock?.Invoke(false);
        }

        private void OnPlayerMove(bool activeInput, Vector2? input = null)
        {
            _activeInput = activeInput;
            _movementInput = activeInput ? input.Value : Vector2.zero;
        }

        private void OnPlayerLook(Vector2 input) => _lookInput = input;

        private void OnPlayerJump() => _jumpInput = true;

        private void OnPlayerGrounded()
        {
            _isGrounded = true;
            _isJumping = false;
            _timesJumped = 0;
        }

        private void OnPlayerLockOn()
        {
            //TODO: Check for all lockable targets in view, and lock onto the one nearest to the screen's center.

            LookAt(Vector3.zero);

            onPlayerLock?.Invoke(true);
        }

        public void RegisterTargetable(Targetable targetable)
        {
            //TODO: Check if target isn't already registered.
            _availableTargets.Add(targetable);
        }

        private void OnDestroy()
        {
            PlayerControls controls = InputHandler.Controls;

            controls.Player.Move.performed -= context => OnPlayerMove(true, context.ReadValue<Vector2>());
            controls.Player.Move.canceled -= context => OnPlayerMove(false);
            controls.Player.Look.performed -= context => OnPlayerLook(context.ReadValue<Vector2>());
            controls.Player.Look.canceled -= context => OnPlayerLook(Vector2.zero);
            controls.Player.Jump.performed -= context => OnPlayerJump();
            controls.Player.Lock.performed -= context => OnPlayerLockOn();
            controls.Player.Lock.canceled -= context => RemoveLockOn();
        }
    }
}