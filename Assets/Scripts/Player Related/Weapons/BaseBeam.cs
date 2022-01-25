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
        protected string _name;
        [SerializeField, Min(.01f)]
        protected float _fireRate;
        [SerializeField, Min(0)]
        protected int _damage;
        [SerializeField, Min(1)]
        protected float _bulletSpeed;
        [SerializeField, Min(0)]
        protected float _despawnTime;

        protected ArmCannon _armCannon;

        internal virtual void Initialize(ArmCannon armCannon) => _armCannon = armCannon;

        internal virtual bool Shoot()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Shoot(_damage, _bulletSpeed, _despawnTime);
            return true;
        }
    }
}