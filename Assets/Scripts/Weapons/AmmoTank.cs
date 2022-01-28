using UnityEngine;

namespace AMPR.Weapon
{
    [CreateAssetMenu(menuName = "AMPR/Ammo Reserve", fileName = "Ammo Reserve")]
    public class AmmoTank : ScriptableObject
    {
        public int Ammo { get => _ammo; }
        public int AmmoCapacity { get => _ammoCap; }

        [SerializeField, Min(0)]
        private int _ammo;
        [SerializeField, Min(0)]
        private int _ammoCap;

        internal bool UseAmmo(int amount)
        {
            if (_ammo - amount > 0)
            {
                _ammo -= amount;
                return true;
                // Notify subscribed scripts of usage (UI elements for example)
            }

            return false;
        }

        public bool AddAmmo(int amount)
        {
            _ammo += amount;
            if (_ammo > _ammoCap)
                _ammo = _ammoCap;

            return true;
        }

        public bool IncreaseTankCapacity(int amount)
        {
            _ammoCap += amount;
            // Notify subscribed scripts of increase (UI elements for example)
            return true;
        }

        public bool ResetTank()
        {
            _ammo = _ammoCap;
            // Notify subscribed scripts of reset
            return true;
        }
    }
}