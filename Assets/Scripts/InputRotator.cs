using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sjouke
{
    public class InputRotator : MonoBehaviour
    {
        public InputHandler InputHandler;
        public float EffectStrength = 1;
        public Vector2 MaxEffect = new Vector2(1, 5);
        public float SmoothSpeed = 5;

        private bool _activeInput;
        private Vector2 _lookInput;
        private Vector3 _oldLookInput;
        private float _currentAcceleration;

        private void Start()
        {
            InputHandler.Controls.Player.Look.performed += context => OnLookInput(true, context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Look.canceled += context => OnLookInput(false, Vector2.zero);
        }

        private void Update()
        {
            bool verticalLook = Mathf.Abs(_lookInput.y) > Mathf.Abs(_lookInput.x);

            Vector3 displacement = Vector3.Lerp(_oldLookInput, new Vector3(-_lookInput.y, verticalLook ? 0 : _lookInput.x, 0), Mathf.SmoothStep(0, 1, _currentAcceleration));

            if (Mathf.Abs(displacement.x) > MaxEffect.x)
                displacement.x = displacement.x > 0 ? MaxEffect.x : -MaxEffect.x;

            if (Mathf.Abs(displacement.y) > MaxEffect.y)
                displacement.y = displacement.y > 0 ? MaxEffect.y : -MaxEffect.y;

            if (_activeInput && _currentAcceleration < 1)
                _currentAcceleration += Time.deltaTime * EffectStrength;
            if (!_activeInput && _currentAcceleration > 0)
                _currentAcceleration -= Time.deltaTime * EffectStrength;

            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                       Quaternion.Euler(displacement * EffectStrength),
                                                       Time.deltaTime * SmoothSpeed);

            _oldLookInput = displacement;
        }

        private void OnLookInput(bool active, Vector2 input)
        {
            _activeInput = active;
            _lookInput = input;
        }
    }
}