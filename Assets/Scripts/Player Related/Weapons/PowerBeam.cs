using UnityEngine;

namespace AMPR.Weapon
{
    [CreateAssetMenu(menuName = "AMPR/PowerBeam", fileName = "Power Beam")]
    public class PowerBeam : BaseBeam
    {
        public new PowerBullet BulletPrefab;

        [SerializeField]
        private float _fadeDuration;

        internal override bool Shoot()
        {
            var bullet = Instantiate(BulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Shoot(_damage, _bulletSpeed, _despawnTime, _fadeDuration);
            return true;
        }
    }
}