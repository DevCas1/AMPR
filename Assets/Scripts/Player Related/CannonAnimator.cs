using System;
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
        [SerializeField, /*Range(-1, 1)*/]
        private Vector2 BobMagnitudeMultiplier = new Vector2(0.5f, 0.5f);
        [SerializeField, /*Range(-1, 1)*/]
        private Vector2 SinusOffset = new Vector2(0.2f, 0);
        [SerializeField]
        private float BobSpeed = 4;

        private const float SINOffset = 1;
        private const float SINLength = 6.25f;

        private bool _punchActive;
        private Tweener _punchTweener;

        private Vector3 _startPos;
        private Vector3 _bobDisplacement;
        private Vector2 _bobInput;
        private bool _shouldBob;
        private bool _bobActive;
        private bool _shouldReset;
        private float _currentSinusT;
        private float _currentSinus;
        private IEnumerator _smoothBobReset;

        private Vector3 LocalPos => transform.localPosition;

        private void Start()
        {
            InputHandler.Controls.Player.Fire.performed += context => OnCannonFire();
            InputHandler.Controls.Player.Move.performed += context => OnPlayerMove(context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Move.canceled += context => OnPlayerStopMoving();

            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, CannonAnimator>(InputHandler, this, gameObject);

            _startPos = LocalPos;
        }

        private void Update()
        {
            if (!_punchActive && (_bobActive || (_shouldBob && !_bobActive)))
            {
                PerformCannonBob();
            }
        }

        private void PerformCannonBob()
        {
            _shouldBob = false;
            _bobActive = true;

            if (_shouldReset)
            {

            }
            else
            {
                _currentSinus = (float)Math.Sin(_currentSinusT);

                _bobDisplacement = _startPos + new Vector3(_currentSinus * BobMagnitudeMultiplier.x,
                                                                  -Mathf.Abs(_currentSinus) * BobMagnitudeMultiplier.y,
                                                                  0);


                _currentSinusT += Time.deltaTime * BobSpeed * _bobInput.magnitude;
            }

            transform.localPosition = Vector3.Lerp(LocalPos, _bobDisplacement, Time.deltaTime);
        }

        private void ResetBob(bool smoothly)
        {
            if (!smoothly)
            {
                transform.localPosition = _startPos;
                _shouldBob = false;
                _bobActive = false;
                _currentSinusT = SINOffset;
                return;
            }

            StartCoroutine(_smoothBobReset = SmoothResetBob());
        }

        private IEnumerator SmoothResetBob()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _startPos, Time.deltaTime * BobSpeed);

            if (Vector3.Distance(LocalPos, _startPos) < 0.05f)
            {
                transform.localPosition = _startPos;
                _shouldBob = false;
                _bobActive = false;
                _currentSinusT = SINOffset;

                StopCoroutine(_smoothBobReset);

                _smoothBobReset = null;
            }

            yield return new WaitForEndOfFrame();
        }

        private void OnCannonFire()
        {
            if (_bobActive)
            {
                ResetBob(false);
            }

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
            _shouldBob = true;
        }

        private void OnPlayerStopMoving() => ResetBob(true);
    }
}