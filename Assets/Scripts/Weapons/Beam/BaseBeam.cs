using UnityEngine;

namespace AMPR.Weapon
{
    public abstract class BaseBeam : ScriptableObject
    {
        public string Name { get => _name; }

        public float FireRate { get => _fireRate; }
        public int Damage { get => _damage; }
        public float BulletSpeed { get => _bulletSpeed; }
        public int ChargeDamage { get => _chargeDamage; }
        public float ChargeBulletSpeed { get => _chargeBulletSpeed; }

        internal BaseBullet BulletPrefab;
        internal BaseBullet ChargeBulletPrefab;
        // public 

        [SerializeField]
        protected string _name;
        [SerializeField, Min(.01f), Header("Normal shot settings")]
        protected float _fireRate;
        [SerializeField, Min(0)]
        protected int _damage;
        [SerializeField, Min(1)]
        protected float _bulletSpeed;
        [SerializeField, Min(0)]
        protected float _despawnTime;
        [SerializeField, Min(1), Header("Charge shot settings")]
        protected int _chargeTime;
        [SerializeField, Min(0)]
        protected int _chargeDamage;
        [SerializeField, Min(1)]
        protected float _chargeBulletSpeed;
        [SerializeField, Min(0)]
        protected float _chargeDespawnTime;

        protected ArmCannon _armCannon;

        internal virtual void Initialize(ArmCannon armCannon) => _armCannon = armCannon;

        internal virtual void ShootBeam()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Shoot(_damage, _bulletSpeed, _despawnTime);
        }

        internal virtual void ChargeShoot()
        {

        }
    }
}