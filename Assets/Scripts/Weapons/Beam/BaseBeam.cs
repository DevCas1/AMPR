using UnityEngine;

namespace AMPR.Weapon
{
    public abstract class BaseBeam : ScriptableObject
    {
        public string Name { get => name; }

        public float FireRate { get => fireRate; }
        public int Damage { get => damage; }
        public float BulletSpeed { get => bulletSpeed; }
        public int ChargeDamage { get => chargeDamage; }
        public float ChargeBulletSpeed { get => chargeBulletSpeed; }

        internal BaseChargeBullet ChargeBulletPrefab;

        [SerializeField]            protected new string name; 
        
        [Header("Normal shot settings")]
        [SerializeField, Min(.01f)] protected     float  fireRate;
        [SerializeField, Min(0)]    protected     int    damage;
        [SerializeField, Min(1)]    protected     float  bulletSpeed;
        [SerializeField, Min(0)]    protected     float  despawnTime;
        
        [Header("Charge shot settings")]
        [SerializeField, Min(1)]    protected     int    chargeTime;
        [SerializeField, Min(0)]    protected     int    chargeDamage;
        [SerializeField, Min(1)]    protected     float  chargeBulletSpeed;
        [SerializeField, Min(0)]    protected     float  chargeDespawnTime;

        [SerializeField]
        private BaseBullet BulletPrefab;

        protected ArmCannon _armCannon;

        internal virtual void Initialize(ArmCannon armCannon) => _armCannon = armCannon;

        internal virtual void ShootBeam()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Initialize(damage, bulletSpeed, despawnTime);
            bullet.Shoot();
        }

        internal virtual void ChargeBeam()
        {

        }

        internal virtual void ShootChargeBeam()
        {

        }
    }
}