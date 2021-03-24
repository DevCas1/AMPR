using System;
using System.Collections;
using System.Collections.Generic;
using Sjouke;
using UnityEngine;

namespace AMPR
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