using System;
using AMPR.Manager;
using UnityEngine;

namespace AMPR.Weapon
{
    public class PlayerWeapon : MonoBehaviour
    {
        public InputHandler InputHandler;

        private bool _canShoot;
        private double _cooldownTimer;

        private void Start()
        {
#if UNITY_EDITOR
            DebugUtility.HandleErrorIfNullFindObject<InputHandler, PlayerWeapon>(gameObject, this);
#endif

            InputHandler.Controls.Player.Fire.performed += OnPlayerShoot();
        }

        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> OnPlayerShoot()
        {
            return null;
        }

        private void OnDisable()
        {
            InputHandler.Controls.Player.Fire.performed -= OnPlayerShoot();
        }
    }
}