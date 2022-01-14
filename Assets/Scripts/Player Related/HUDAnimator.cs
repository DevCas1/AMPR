using AMPR.Manager;
using DG.Tweening;
using UnityEngine;

namespace AMPR.PlayerController
{
    public class HUDAnimator : MonoBehaviour
    {
        [Header("References")]
        public InputHandler InputHandler;
        public PlayerController PlayerController;
        public Transform PlayerCamTransform;

        [Header("Rotation settings")]
        public float LookEffectStrength = 1;
        public Vector2 MaxLookEffect = new Vector2(1, 5);
        public float JumpEffectStrength = 2;
        public float JumpEffectDuration = 0.2f;
        public float SmoothSpeed = 5;

        [Header("Shake settings"), Min(0)]
        public int AmountOfShakes;
        public float ShakeDuration;
        public float ShakeStrength;
        public int ShakeVibrato;
        [Range(0, 90)]
        public int ShakeRandomness;

        private bool _activeInput;
        private Vector2 _lookInput;
        private Vector3 _oldLookInput;
        private Vector2 _oldRot;
        private Vector2 _lookRot;
        private float _currentAcceleration;
        private bool _playerLock;

        private void Start()
        {
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullFindObject<Transform, HUDAnimator>(gameObject, this);
            // DebugUtility.HandleErrorIfNullGetComponent<InputHandler, HUDAnimator>(InputHandler, this, gameObject);
            DebugUtility.HandleErrorIfNullGetComponent<PlayerController, HUDAnimator>(PlayerController, this, gameObject);
#endif
            InputHandler.Controls.Player.Look.performed += context => OnLookInput(true, context.ReadValue<Vector2>());
            InputHandler.Controls.Player.Look.canceled += context => OnLookInput(false, Vector2.zero);

            PlayerController.ONPlayerJump += OnPlayerJump;
            PlayerController.ONPlayerLand += OnPlayerLand;
            PlayerController.ONPlayerLock += OnPlayerLock;
        }

        private void Update() => RotateHelmet();

        private void RotateHelmet()
        {
            Vector3 displacement = Vector3.Lerp(_oldLookInput,                                          // TODO: Replace input-based displacement with position-based difference
                                                _playerLock ? Vector3.zero : new Vector3(-_lookInput.y,
                                                                                         _lookInput.x,
                                                                                         0),
                                                Mathf.SmoothStep(0, 1, _currentAcceleration));

            if (Mathf.Abs(displacement.x) > MaxLookEffect.x)
                displacement.x = displacement.x > 0 ? MaxLookEffect.x : -MaxLookEffect.x;

            if (Mathf.Abs(displacement.y) > MaxLookEffect.y)
                displacement.y = displacement.y > 0 ? MaxLookEffect.y : -MaxLookEffect.y;

            switch (_activeInput)
            {
                case true when _currentAcceleration < 1:
                    _currentAcceleration += Time.deltaTime * LookEffectStrength;
                    break;
                case false when _currentAcceleration > 0:
                    _currentAcceleration -= Time.deltaTime * LookEffectStrength;
                    break;
            }

            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                       Quaternion.Euler(displacement * LookEffectStrength),
                                                       Time.deltaTime * SmoothSpeed);

            if (!_playerLock)
                _oldLookInput = displacement;
        }

        private void LateUpdate()
        {
            _oldRot = new Vector2(PlayerCamTransform.eulerAngles.x, PlayerController.transform.eulerAngles.y);
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

        private void OnPlayerLand()
        {
            _currentAcceleration = 0f;
            transform.DOBlendableLocalRotateBy(new Vector3(-JumpEffectStrength / 2, 0, 0), JumpEffectDuration);
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
            PlayerController.ONPlayerJump -= OnPlayerJump;
            PlayerController.ONPlayerLock -= OnPlayerLock;
        }
    }
}