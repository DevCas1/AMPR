using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace AMPR
{
    public class CannonAnimator : MonoBehaviour // TODO: Implement local rotation over time by high rate of fire
    {
        [Header("References")]
        public InputHandler InputHandler;

        [Header("Punch Related")]
        [SerializeField]
        private float PunchForce = 0.33f;
        [SerializeField]
        private float PunchDuration = 0.4f;
        [SerializeField]
        private float PunchElasticity = 0.33f;

        [Header("Cannon bob Related")]
        [SerializeField, Range(-1, 1)]
        private Vector2 BobMagnitudeMultiplier = new Vector2(0.5f, 0.5f);
        [SerializeField]
        private float BobSpeed = 1;

        private const float SINOffset = 0.5f;

        private bool _punchActive;
        private Tweener _punchTweener;

        private Vector3 _startPos;
        private Vector3 _bobDisplacement;
        private Vector2 _bobInput;
        private bool _shouldBob;
        private bool _bobActive;
        private float _currentSinT;
        private float _currentSinus;
        private IEnumerator _smoothBobReset;

        private void Start()
        {
            InputHandler.Controls.Player.Fire.performed += context => OnCannonFire();
            InputHandler.Controls.Player.Move.performed += context => OnPlayerMove(context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Move.canceled += context => OnPlayerStopMoving();
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, CannonAnimator>(InputHandler, this, gameObject);

            _startPos = transform.localPosition;
        }

        private void Update()
        {
            if (!_punchActive && (_shouldBob || _bobActive))
            {
                PerformCannonBob();
            }
        }

        private void PerformCannonBob()
        {
            _currentSinus = Mathf.Sin(_currentSinT);

            _bobDisplacement = new Vector3(_startPos.x + ((_currentSinus - SINOffset) * BobMagnitudeMultiplier.x),
                                           _startPos.y + -(_currentSinus * BobMagnitudeMultiplier.y),
                                           0);

            _currentSinT += Time.deltaTime * BobSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _bobDisplacement, Mathf.SmoothStep(0, 1, _currentSinus));
        }

        private void ResetBob(bool smoothly)
        {
            if (!smoothly)
            {
                transform.localPosition = _startPos;
                _shouldBob = false;
                _bobActive = false;
                _currentSinT = SINOffset;
                return;
            }

            StartCoroutine(_smoothBobReset = SmoothResetBob());
        }

        private IEnumerator SmoothResetBob()
        {
            return null;
        }

        private void OnCannonFire()
        {
            if (_bobActive)
            {
                ResetBob(false);
            }

            if (_punchActive)
                _punchTweener.Complete(false);

            TweenCallback SetPunchActive(bool active)
            {
                _punchActive = active;
                return null;
            }

            TweenCallback RemoveReference()
            {
                _punchTweener = null;
                return null;
            }

            _punchTweener = transform.DOPunchPosition(new Vector3(0, 0, -PunchForce), PunchDuration, 0, PunchElasticity)
                .OnStart(SetPunchActive(true))
                .OnComplete(SetPunchActive(false))
                .OnKill(RemoveReference());
        }

        private void OnPlayerMove(Vector2 input)
        {
            _bobInput = input;
            _shouldBob = true;
        }

        private void OnPlayerStopMoving() => _bobActive = false;
    }
}