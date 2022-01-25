using DG.Tweening;
using UnityEngine;

namespace AMPR.Weapon
{
    public class PowerBullet : BaseBullet
    {
        private Light _light;
        private float _fadeDuration;

        internal bool Initialize(int damage, float speed, float despawnTime, float fadeDuration)
        {
            _light = GetComponent<Light>();
            _fadeDuration = fadeDuration;

            return base.Initialize(damage, speed, despawnTime);
        }

        internal bool Shoot(int damage, float speed, float despawnTime, float fadeDuration)
        {
            _light = GetComponent<Light>();
            _fadeDuration = fadeDuration;

            return base.Shoot(damage, speed, despawnTime);
        }

        protected override void End()
        {
            _rb.velocity = Vector3.zero;
            _col.enabled = false;
            DOTween.To(() => _light.intensity, x => _light.intensity = x, 0, _fadeDuration).OnComplete(() => base.End());
        }
    }
}