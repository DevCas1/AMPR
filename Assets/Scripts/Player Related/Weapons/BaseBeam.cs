using UnityEngine;

namespace AMPR.Weapon
{
    public abstract class BaseBeam : ScriptableObject
    {
        public string Name { get => _name; }
        public float FireRate { get => _fireRate; }
        public int Damage { get => _damage; }
        public float BulletSpeed { get => _bulletSpeed; }

        public BaseBullet BulletPrefab;

        [SerializeField]
        private string _name;
        [SerializeField, Min(.01f)]
        private float _fireRate;
        [SerializeField, Min(0)]
        private int _damage;
        [SerializeField, Min(1)]
        private float _bulletSpeed;
        [SerializeField, Min(0)]
        private float _despawnTime;

        private ArmCannon _armCannon;

        internal virtual void Initialize(ArmCannon armCannon) => _armCannon = armCannon;

        internal virtual bool Shoot()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Shoot(_damage, _bulletSpeed, _despawnTime);
            return true;
        }
    }
}