using System;
using NaughtyAttributes;
using UnityEngine;

namespace AMPR.Weapon
{
    [Serializable]
    public abstract class BaseAmmoBeam : BaseBeam
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _fireRate;

        [SerializeField, Header("Ammo Settings")]
        private bool _requireAmmo;
        [SerializeField, EnableIf(nameof(_requireAmmo))]
        private AmmoTank _ammoReserve;
        [SerializeField, EnableIf(nameof(_requireAmmo))]
        private int _ammoPerShot;
    }
}