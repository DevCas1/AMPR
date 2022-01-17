using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AMPR
{
    public class DummyEnemy : Enemy
    {
        private void Awake() => Player = FindObjectOfType<Controls.PlayerController>();

        void Start()
        {
            OnBecomeVisible();
        }

        private void OnEnable() => OnBecomeVisible();

        private void OnDisable() => OnBecomeInvisible();
    }
}
