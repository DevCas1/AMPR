using AMPR.Manager;
using DG.Tweening;
using UnityEngine;

namespace AMPR.Controls
{
    public class HUDAnimator : MonoBehaviour
    {
        [Header("References")]
        public InputHandler InputHandler;
        public PlayerController PlayerController;
        public Transform PlayerCamTransform;

        [Header("Rotation settings")]
        public float LookEffectStrength = 1;
        public Vector2 MaxLookEffect = new(3, 1);
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

            PlayerController.OnPlayerJump += OnPlayerJump;
            PlayerController.OnPlayerLand += OnPlayerLand;
            PlayerController.OnPlayerLock += OnPlayerLock;
        }

        private void Update() => RotateHelmet();

        // private void RotateHelmet()
        // {
        //     Vector3 rot = transform.eulerAngles;
        //     Vector2 target = new(PlayerCamTransform.localEulerAngles.x, PlayerController.transform.eulerAngles.y);
        //     Vector2 difference = new(target.x - rot.x, target.y - rot.y);

        //     // if (difference.x > MaxLookEffect.x)
        //     //     difference.x = MaxLookEffect.x;

        //     // if (difference.y > MaxLookEffect.y)
        //     //     difference.y = MaxLookEffect.y;

        //     Quaternion rotation = transform.localRotation;

        //     transform.localRotation = Quaternion.Slerp(rotation,
        //                                                Quaternion.Euler(difference),
        //                                                Time.deltaTime * SmoothSpeed);
        // }

        private void RotateHelmet()
        {
            Vector3 displacement = Vector3.Lerp(_oldLookInput,
                                                _playerLock ? Vector3.zero : new Vector3(-_lookInput.y,
                                                                                         _lookInput.x,
                                                                                         0),
                                                Mathf.SmoothStep(0, 1, _currentAcceleration));

            LimitValues(ref displacement);

            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                       Quaternion.Euler(displacement * LookEffectStrength),
                                                       Time.deltaTime * SmoothSpeed);

            if (!_playerLock)
                _oldLookInput = displacement;
        }

        private Vector3 LimitValues(ref Vector3 displacement)
        {
            if (Mathf.Abs(displacement.x) > MaxLookEffect.x)
                displacement.x = displacement.x > 0 ? MaxLookEffect.x : -MaxLookEffect.x;

            if (Mathf.Abs(displacement.y) > MaxLookEffect.y)
                displacement.y = displacement.y > 0 ? MaxLookEffect.y : -MaxLookEffect.y;

            if (_activeInput && _currentAcceleration < 1)
                _currentAcceleration += Time.deltaTime * LookEffectStrength;

            if (!_activeInput && _currentAcceleration > 0)
                _currentAcceleration -= Time.deltaTime * LookEffectStrength;

            return displacement;
        }

        private void LateUpdate() => _oldRot = new(PlayerCamTransform.localEulerAngles.x, PlayerController.transform.eulerAngles.y);

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
            PlayerController.OnPlayerJump -= OnPlayerJump;
            PlayerController.OnPlayerLock -= OnPlayerLock;
        }
    }
}