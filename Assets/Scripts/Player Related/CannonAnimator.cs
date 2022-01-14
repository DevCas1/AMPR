using System;
using AMPR.Manager;
using DG.Tweening;
using UnityEngine;

namespace AMPR.PlayerController
{
    public class CannonAnimator : MonoBehaviour // TODO: Implement local rotation over time by high rate of fire
    {
        [Header("References")]
        public InputHandler InputHandler;
        public PlayerController PlayerController;

        [Header("Punch Related")]
        [SerializeField]
        private float PunchForce = 0.33f;
        [SerializeField]
        private float PunchDuration = 0.4f;
        [SerializeField]
        private float PunchElasticity = 0.33f;

        [Header("Cannon bob Related")]
        [SerializeField]
        private Vector2 BobMagnitudeMultiplier = new Vector2(0.01f, 0.033f);
        [SerializeField]
        private float BobSpeed = 6;
        [SerializeField]
        private float BobSmoothSpeed = 10;

        [Header("Jump landing Related")]
        [SerializeField]
        private float LandPunchForce = 0.05f;

        private bool _punchActive;
        private Tweener _punchTweener;

        private Vector3 _startPos;
        private Vector3 _bobDisplacement;
        private bool _activeInput;
        private Vector2 _bobInput;
        private bool _isGrounded = true;
        private bool _shouldBob;
        private bool _bobActive;
        private float _currentSinusT;
        private float _currentSinus;
        private Tweener _smoothBobReset;

        private bool _landPunchActive;
        private Tweener _jumpLandPunch;

        private Vector3 LocalPos => transform.localPosition;

        private void Start()
        {
            InputHandler.Controls.Player.Fire.performed += context => OnCannonFire();
            InputHandler.Controls.Player.Move.performed += context => OnPlayerMove(context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Move.canceled += context => OnPlayerStopMoving();

            PlayerController.ONPlayerJump += OnPlayerJump;
            PlayerController.ONPlayerLand += OnPlayerLand;

            // DebugUtility.HandleErrorIfNullGetComponent<InputHandler, CannonAnimator>(InputHandler, this, gameObject);
            DebugUtility.HandleErrorIfNullGetComponent<PlayerController, CannonAnimator>(PlayerController, this, gameObject);

            _startPos = LocalPos;
            _isGrounded = true;
        }

        private void Update()
        {
            if (_jumpLandPunch != null)
                return;

            if (!_punchActive && _isGrounded && (_bobActive || _activeInput || (_shouldBob && !_bobActive))) // TODO: Test if conditions are grouped correctly (especially _activeInput)
            {
                PerformCannonBob();
            }
        }

        private void PerformCannonBob()
        {
            _shouldBob = false;
            _bobActive = true;

            _currentSinus = (float)Math.Sin(_currentSinusT);
            _bobDisplacement = _startPos + new Vector3(_currentSinus * BobMagnitudeMultiplier.x,
                                                       -Mathf.Abs(_currentSinus) * BobMagnitudeMultiplier.y,
                                                       0);
            _currentSinusT += Time.deltaTime * BobSpeed * _bobInput.magnitude;
            transform.localPosition = Vector3.Lerp(LocalPos, _bobDisplacement, Time.deltaTime * BobSmoothSpeed);
        }

        private void ResetBob(bool smoothly)
        {
            _shouldBob = false;
            _bobActive = false;

            if (!smoothly)
            {
                transform.localPosition = _startPos;
                _smoothBobReset?.Complete(false);
                return;
            }

            _currentSinusT = 0;
            _smoothBobReset = transform.DOLocalMove(_startPos, PunchDuration).OnKill(() => _smoothBobReset = null);
        }

        private void OnCannonFire()
        {
            if (_bobActive)
                ResetBob(false);

            if (_landPunchActive)
                _jumpLandPunch.Complete(false);

            if (_punchActive)
                _punchTweener.Complete(false);

            _punchTweener = transform.DOPunchPosition(new Vector3(0, 0, -PunchForce), PunchDuration, 0, PunchElasticity)
                .OnStart(() => _punchActive = true)
                .OnComplete(() => _punchActive = false)
                .OnKill(() => _punchTweener = null);
        }

        private void OnPlayerMove(Vector2 input)
        {
            _bobInput = input;
            _activeInput = true;
            _shouldBob = true;
        }

        private void OnPlayerStopMoving()
        {
            ResetBob(true);
            _activeInput = false;
        }

        private void OnPlayerJump() => _isGrounded = false;

        private void OnPlayerLand()
        {
            _isGrounded = true;
            _jumpLandPunch = transform.DOPunchPosition(Vector3.down * LandPunchForce, PunchDuration / 2, 1, 0).OnKill(() => _jumpLandPunch = null)
                .OnStart(() => _landPunchActive = true)
                .OnComplete(() => _landPunchActive = false)
                .OnKill(() => _jumpLandPunch = null);
        }

        private void OnDestroy()
        {
            InputHandler.Controls.Player.Fire.performed -= context => OnCannonFire();
            InputHandler.Controls.Player.Move.performed -= context => OnPlayerMove(context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Move.canceled -= context => OnPlayerStopMoving();

            PlayerController.ONPlayerJump -= OnPlayerJump;
            PlayerController.ONPlayerLand -= OnPlayerLand;
        }
    }
}