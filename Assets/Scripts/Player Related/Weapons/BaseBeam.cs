using UnityEngine;

namespace AMPR.Weapon
{
    public abstract class BaseBeam : ScriptableObject
    {
        public string Name { get => _name; }
        public int FireRate { get => _fireRate; }

        public BaseBullet BulletPrefab;

        [SerializeField]
        private string _name;
        [SerializeField, Min(1)]
        private int _fireRate;

        private ArmCannon _armCannon;

        internal virtual void Initialize(ArmCannon armCannon)
        {
            _armCannon = armCannon;
        }

        internal virtual bool Shoot()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Shoot();
            return true;
        }
    }
}