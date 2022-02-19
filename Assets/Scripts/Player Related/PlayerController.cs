using System.Collections.Generic;
using AMPR.Manager;
using NaughtyAttributes;
using UnityEngine;

namespace AMPR.Controls
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private enum LockOnStatus { None, Transform, Position }
        private static int DEFAULT_NON_ALLOC_SIZE = 10;
        private static int DEFAULT_TARGETABLE_SIZE = 10;

        public Vector3 Velocity { get => _playerVelocity; }

        /// Public Unity initialized \\\\\
        [Header("References")]

        [Tooltip("Reference to the camera attached to the Player object.")]
        public Camera PlayerCamera;
        public InputHandler InputHandler;

        [Header("Movement Values")]

        [SerializeField, Tooltip("The speed at which the player moves.")]
        private float _MovementSpeed = 300;
        [SerializeField]
        private bool _UseAcceleration = true;
        [SerializeField, Tooltip("Rate of acceleration and deceleration when the player moves."), /*Min(0),*/ Range(0, 1), EnableIf(nameof(_UseAcceleration))]
        private float _Acceleration = .33f;
        [SerializeField, Tooltip("Whether to clamp the player's maximum velocity to a specific value.")]
        private bool _ClampVelocity;
        [SerializeField, Tooltip("The max magnitude for player movement."), EnableIf(nameof(_ClampVelocity))]
        private float _MaxVelocityMagnitude;

        [Header("Camera Settings")]

        [SerializeField, Tooltip("The rate at which the camera will turn relative to the input.")]
        private float _TurnSpeed = 16;
        [SerializeField, Tooltip("Inverse the X axis of camera input.")]
        private bool _InvertX;
        [SerializeField, Tooltip("Inverse the Y axis of camera input.")]
        private bool _InvertY;
        [SerializeField]
        private bool _UseRotationSmoothing;
        [SerializeField, Tooltip("The amount of smoothing used while rotating the player camera."), Min(0), EnableIf(nameof(_UseRotationSmoothing))]
        private float _SmoothTurnSpeed;
        [SerializeField, Tooltip("The highest angle at which the player can look up in degrees. (270 is straight upward)"), Range(270, 359)]
        private int _MaxCameraAngle = 270;
        [SerializeField, Tooltip("The lowest angle at which the player can look down in degrees. (90 is straight downard)"), Range(0, 90)]
        private int _MinCameraAngle = 90;

        [Header("Jump Settings")]

        [SerializeField, Tooltip("The amount of force applied to the player when performing a jump.")]
        private float _JumpForce = 8;
        [SerializeField, Tooltip("The distance from the player object's point of center at which will be checked for ground. \nSetting this too small might result in undesired negatives, while too high might feel like you can jump without touching any ground.")]
        private float _JumpCheckDistance = 0.15f;
        [SerializeField]
        private Vector3 _JumpCheckOffset;
        [SerializeField, Tooltip("The radius of the jump check.\nSetting this too small might result in irregular positives when trying to jump on narrow surfaces.")]
        private float _JumpCheckRadius = 0.2f;
        [SerializeField, Tooltip("The layers on which the player can jump.")] // ReSharper disable once IdentifierTypo
        private LayerMask _JumpableLayers;
        [SerializeField, Tooltip("The amount of time repeated jump inputs will be ignored, after a jump has been performed.")]
        private float _JumpCooldown = 0.15f;
        [SerializeField, Tooltip("The amount of times the player can jump without touching any ground."), Range(1, 3)]
        private int _AmountOfJumps = 2;

        [Header("Lock On Settings"), SerializeField]
        private float _PositionLockDistance = 100;

        public Vector2 Torque => _rotationVector;

        public delegate void PlayerJumpEvent();
        public delegate void PlayerLockEvent(bool state);
        public event PlayerJumpEvent OnPlayerJump;
        public event PlayerJumpEvent OnPlayerLand;
        public event PlayerLockEvent OnPlayerLock;

        private Transform _playerCamTransform;
        private CharacterController _controller;

        // Input related
        private Vector2 _movementInput;
        private Vector2 _lastMovementInput;
        private bool _activeInput;
        private Vector2 _lookInput;
        private bool _jumpInput;

        // Movement related
        private Vector3 _playerVelocity = Vector3.zero;
        private double _currentAcceleration;
        private CollisionFlags _collisionFlags;
        private Vector3 _gravity;

        // Rotation related
        private Vector2 _rotationVector;

        // Jump related
        private bool _isJumping;
        private bool _playerGrounded;
        private double _jumpCooldownTimer;
        private int _timesJumped;
        private RaycastHit[] _nonAllocBuffer;

        // Look lock related
        private LockOnStatus _lockOnStatus;
        private Transform _lockOnTransform;
        private Vector3 _lockOnPosition = Vector3.zero;
        private List<ITarget> _availableTargets;
        //TODO: Keep a list of all lockable targets currently present in view

        private void Reset()
        {
            _controller = GetComponent<CharacterController>();
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerController>(_controller, this, gameObject);
#endif
        }

        private void InitializeControls()
        {
            // #if UNITY_EDITOR
            //             DebugUtility.HandleErrorIfNullGetComponent<InputHandler, PlayerController>(InputHandler, this, gameObject);
            // #endif
            PlayerControls controls = InputHandler.Controls;

            controls.Player.Move.performed += context => OnMoveInput(true, context.ReadValue<Vector2>());
            controls.Player.Move.canceled += context => OnMoveInput();
            controls.Player.Look.performed += context => OnLookInput(context.ReadValue<Vector2>());
            controls.Player.Look.canceled += context => OnLookInput(Vector2.zero);
            controls.Player.Jump.performed += context => OnJumpInput();
            controls.Player.Lock.performed += context => OnPlayerLockOn();
            controls.Player.Lock.canceled += context => RemoveLockOn();
        }

        private void Start()
        {
            if (_controller == null)
                _controller = GetComponent<CharacterController>();

            InitializeControls();

            _gravity = Physics.gravity;
            _playerCamTransform = PlayerCamera.transform;
            _nonAllocBuffer = new RaycastHit[DEFAULT_NON_ALLOC_SIZE];
            _availableTargets = new List<ITarget>(DEFAULT_TARGETABLE_SIZE);

            InputHandler.Controls.Player.Enable();
        }

        private void OnEnable() => InputHandler.Controls.Player.Enable();

        private void OnDisable() => InputHandler.Controls.Player.Disable();

        private void Update()
        {
            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= Time.deltaTime;

            UpdateRotations();
            UpdateMovement();
        }

        private void UpdateRotations()
        {
            if (_lockOnStatus == LockOnStatus.None)
            {
                Vector2 newLookVector = _lookInput * (_TurnSpeed * Time.deltaTime);
                Vector2 newRotation = new(_playerCamTransform.localRotation.eulerAngles.x - (_InvertY ? -newLookVector.y : newLookVector.y),
                                          transform.rotation.eulerAngles.y + ((_InvertX ? -newLookVector.x : newLookVector.x)));

                ApplyRotationConstraints(ref newRotation);

                _playerCamTransform.localRotation = Quaternion.Euler(newRotation.x, 0, 0);
                transform.rotation = Quaternion.Euler(0, newRotation.y, 0);
                return;
            }

            Vector3 targetPos = _lockOnStatus == LockOnStatus.Transform ? _lockOnTransform.position : _lockOnPosition;

            transform.LookAt(targetPos);
            _playerCamTransform.LookAt(targetPos);

            transform.rotation = Quaternion.Euler(new(0, transform.eulerAngles.y, 0));
            _playerCamTransform.localRotation = Quaternion.Euler(new(_playerCamTransform.localEulerAngles.x, 0, 0));
        }

        private void ApplyRotationConstraints(ref Vector2 newRotation)
        {
            if (newRotation.x < 180 && newRotation.x > _MinCameraAngle)
            {
                newRotation.x = _MinCameraAngle;
                RemoveLockOn();
            }

            if (newRotation.x > 180 && newRotation.x < _MaxCameraAngle)
            {
                newRotation.x = _MaxCameraAngle;
                RemoveLockOn();
            }
        }

        private void UpdateMovement()
        {
            float deltaTime = Time.deltaTime;
            _playerGrounded = CheckForGround();

            if (_playerGrounded && _playerVelocity.y < 0)
                _playerVelocity.y = 0f;

            if (_jumpInput && CanPlayerJump())
                PerformJump();

            Vector3 forwardMovement;

            if (_UseAcceleration)
            {
                Vector3 moveInput = Vector3.Lerp(_lastMovementInput, _movementInput, Mathf.SmoothStep(0, 1, _Acceleration));

                if (_activeInput && _currentAcceleration < 1)
                    _currentAcceleration += deltaTime * _Acceleration;
                if (!_activeInput && _currentAcceleration > 0)
                    _currentAcceleration -= deltaTime * _Acceleration;

                Mathf.Clamp((float)_currentAcceleration, 0, 1);
                _lastMovementInput = moveInput;
                forwardMovement = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
            }
            else
                forwardMovement = transform.TransformDirection(new Vector3(_movementInput.x, 0, _movementInput.y));

            _playerVelocity += _gravity * deltaTime;

            _collisionFlags = _controller.Move((forwardMovement * _MovementSpeed + _playerVelocity) * deltaTime);
            _jumpInput = false;
        }

        private bool CheckForGround()
        {
#if UNITY_EDITOR
            // Debug.DrawRay(_rb.position + new Vector3(0, JumpCheckOffset, 0), Vector3.down * JumpLandCheckDistance, Color.magenta, 1);
#endif

            int hits = Physics.SphereCastNonAlloc(transform.position + _JumpCheckOffset,
                                                  _JumpCheckRadius,
                                                  Vector3.down,
                                                  _nonAllocBuffer,
                                                  _JumpCheckDistance,
                                                  _JumpableLayers,
                                                  QueryTriggerInteraction.Ignore);

            if (hits == 0)
            {
                _playerGrounded = false;
                return false;
            }

            if (!_playerGrounded && hits > 0)
                OnPlayerGrounded();

            return true;
        }

        private bool CanPlayerJump() => _playerGrounded || (_jumpCooldownTimer <= 0 && _timesJumped < _AmountOfJumps);

        private void PerformJump()
        {
            _playerVelocity.y = _JumpForce;
            _jumpInput = false;
            _jumpCooldownTimer = _JumpCooldown;
            _timesJumped++;

            OnPlayerJump?.Invoke();
        }

        public void LockOnTransform(Transform lockTransform)
        {
            _lockOnStatus = LockOnStatus.Transform;
            _lockOnTransform = lockTransform;
        }

        public void LockOnPosition(Vector3 lockPosition)
        {
            _lockOnStatus = LockOnStatus.Position;
            _lockOnPosition = lockPosition;
        }

        public void RemoveLockOn()
        {
            _lockOnStatus = LockOnStatus.None;
            _lockOnPosition = Vector3.zero;
            _lockOnTransform = null;

            OnPlayerLock?.Invoke(false);
        }

        private void OnMoveInput(bool activeInput = false, Vector2? input = null)
        {
            _activeInput = activeInput;
            _movementInput = input ?? Vector2.zero;
        }

        private void OnLookInput(Vector2 input) => _lookInput = input;

        private void OnJumpInput() => _jumpInput = true;

        private void OnPlayerGrounded()
        {
            _playerGrounded = true;
            _timesJumped = 0;

            OnPlayerLand?.Invoke();
        }

        private void OnPlayerLockOn()
        {
            //TODO: Check for all lockable targets in view, and lock onto the one nearest to the screen's center.

            LockOnPosition(_playerCamTransform.forward * _PositionLockDistance);

            OnPlayerLock?.Invoke(true);
        }

        private void TargetLost()
        {
            if (_lockOnTransform == null)
            {
                Debug.LogError("TargetLost should never be called without a target!");
                return;
            }

            _lockOnPosition = _lockOnTransform.position;
            _lockOnStatus = LockOnStatus.Position;
            _lockOnTransform = null;
        }

        public void RegisterTarget(ITarget target)
        {
            //TODO: Check if target isn't already registered.
            _availableTargets.Add(target);
        }

        public void UnregisterTarget(ITarget target)
        {
            if (_lockOnTransform.GetComponent<ITarget>() == target)
                TargetLost();

            _availableTargets.Remove(target);
        }

        private void OnDestroy()
        {
            PlayerControls controls = InputHandler.Controls;

            controls.Player.Move.performed -= context => OnMoveInput(true, context.ReadValue<Vector2>());
            controls.Player.Move.canceled -= context => OnMoveInput();
            controls.Player.Look.performed -= context => OnLookInput(context.ReadValue<Vector2>());
            controls.Player.Look.canceled -= context => OnLookInput(Vector2.zero);
            controls.Player.Jump.performed -= context => OnJumpInput();
            controls.Player.Lock.performed -= context => OnPlayerLockOn();
            controls.Player.Lock.canceled -= context => RemoveLockOn();
        }
    }
}