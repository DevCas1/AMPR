using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Sjouke
{
    public class HUDRotator : MonoBehaviour
    {
        public InputHandler InputHandler;
        public PlayerController PlayerController;

        public float LookEffectStrength = 1;
        public Vector2 MaxLookEffect = new Vector2(1, 5);
        public float JumpEffectStrength = 2;
        public float JumpEffectDuration = 0.2f;
        public float SmoothSpeed = 5;

        private bool _activeInput;
        private Vector2 _lookInput;
        private Vector3 _oldLookInput;
        private float _currentAcceleration;
        private bool _playerLock;

        private void Start()
        {
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, HUDRotator>(InputHandler, this, gameObject);
#endif
            InputHandler.Controls.Player.Look.performed += context => OnLookInput(true, context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Look.canceled += context => OnLookInput(false, Vector2.zero);

#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullGetComponent<PlayerController, HUDRotator>(PlayerController, this, gameObject);
#endif
            PlayerController.onPlayerJump += OnPlayerJump;
            PlayerController.onPlayerLock += OnPlayerLock;
        }

        private void Update()
        {
            Vector3 displacement = Vector3.Lerp(_oldLookInput,
                                                _playerLock ? Vector3.zero : new Vector3(-_lookInput.y,
                                                                                         _lookInput.x,
                                                                                         0),
                                                Mathf.SmoothStep(0, 1, _currentAcceleration));

            if (Mathf.Abs(displacement.x) > MaxLookEffect.x)
                displacement.x = displacement.x > 0 ? MaxLookEffect.x : -MaxLookEffect.x;

            if (Mathf.Abs(displacement.y) > MaxLookEffect.y)
                displacement.y = displacement.y > 0 ? MaxLookEffect.y : -MaxLookEffect.y;

            if (_activeInput && _currentAcceleration < 1)
                _currentAcceleration += Time.deltaTime * LookEffectStrength;
            if (!_activeInput && _currentAcceleration > 0)
                _currentAcceleration -= Time.deltaTime * LookEffectStrength;

            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                       Quaternion.Euler(displacement * LookEffectStrength),
                                                       Time.deltaTime * SmoothSpeed);

            if (!_playerLock)
                _oldLookInput = displacement;
        }

        private void OnLookInput(bool active, Vector2 input)
        {
            _activeInput = active;
            _lookInput = input;
        }

        private void OnPlayerJump()
        {
            _currentAcceleration = 0f;
            transform.DOBlendableLocalRotateBy(new Vector3(-JumpEffectStrength, 0, 0), JumpEffectDuration);
        }

        private void OnPlayerLock(bool state)
        {
            _playerLock = state;
            _lookInput = state ? Vector2.zero : new Vector2(_oldLookInput.x, _oldLookInput.y);
        }

        private void OnDestroy()
        {
            InputHandler.Controls.Player.Look.performed -= context => OnLookInput(true, context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Look.canceled -= context => OnLookInput(false, Vector2.zero);
            PlayerController.onPlayerJump -= OnPlayerJump;
            PlayerController.onPlayerLock -= OnPlayerLock;
        }
    }
}