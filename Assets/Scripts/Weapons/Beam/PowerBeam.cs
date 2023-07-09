using UnityEngine;

namespace AMPR.Weapon
{
    [CreateAssetMenu(menuName = "AMPR/PowerBeam", fileName = "Power Beam")]
    internal class PowerBeam : BaseBeam
    {
        [SerializeField] private PowerBullet bulletPrefab;
        [SerializeField] private float       fadeDuration;

        internal override void ShootBeam()
        {
            var bullet = Instantiate(bulletPrefab, _armCannon.BulletOrigin.position, _armCannon.transform.rotation);
            bullet.Initialize(damage, bulletSpeed, despawnTime, fadeDuration);
            bullet.Shoot();
        }
    }
}