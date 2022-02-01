using AMPR.Manager;
using UnityEngine;

namespace AMPR.Weapon
{
    public class ArmCannon : MonoBehaviour
    {
        public InputHandler InputHandler;
        public Transform BulletOrigin;

        [SerializeField]
        private BaseBeam[] Beams = new BaseBeam[4];

        public delegate void ShootEvent();
        public event ShootEvent OnShoot;

        private BaseBeam _currentBeam;
        private bool _canShoot = true;
        private bool _shootInput;
        private bool _chargeInput;
        private float _Cooldown;
        private float _cooldownTimer;

        private void OnEnable()
        {
            InputHandler.Controls.Player.Fire.started += context => _shootInput = true;
            InputHandler.Controls.Player.Fire.performed += context => OnCharge();
            InputHandler.Controls.Player.Fire.canceled += context => OnChargeShoot();
        }

        private void Start()
        {
            SetBeam(0);
            Initialize();

            _canShoot = true; //TODO: Use this for disabling shooting during cutscenes or the like
        }

        private void SetBeam(int beamIndex)
        {
            _currentBeam = Beams[beamIndex];
            _Cooldown = 1 / _currentBeam.FireRate;
            _cooldownTimer = 0;
        }

        private void Initialize()
        {
            for (int index = 0; index < Beams.Length; index++)
            {
                if (Beams[index] == null)
                {
                    Debug.LogWarning($"Empty Beam slot at index {index}!");
                    continue;
                }
                Beams[index].Initialize(this);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (_cooldownTimer > 0)
            {
                if (_canShoot && _shootInput)
                {
                    Debug.Log($"Not ready to shoot for another {_cooldownTimer}(m)s"); // TODO: remove when released
                    _shootInput = false;
                }

                UpdateTimer(ref deltaTime);
                return;
            }

            if (!_canShoot)
                return;

            if (_shootInput)
            {
                _currentBeam.ShootBeam();
                OnShoot?.Invoke();
                _shootInput = false;
                _cooldownTimer = _Cooldown;
            }
        }

        private void UpdateTimer(ref float deltaTime)
        {
            _cooldownTimer -= deltaTime;
            if (_cooldownTimer < 0)
                _cooldownTimer = 0;
        }

        private void OnCharge()
        {
            _chargeInput = true;
        }

        private void OnChargeShoot()
        {
            _chargeInput = false;
        }

        private void OnDisable() => CleanUp();

        private void OnDestroy() => CleanUp();

        private void CleanUp() => InputHandler.Controls.Player.Fire.performed -= context => _shootInput = true;
    }
}