using DG.Tweening;
using UnityEngine;

namespace AMPR.Weapon
{
    internal class PowerBullet : BaseBullet
    {
        private Light _light;
        private float _fadeDuration;

        internal void Initialize(int damage, float speed, float despawnTime, float fadeDuration)
        {
            _light = GetComponent<Light>();
            _fadeDuration = fadeDuration;

            base.Initialize(damage, speed, despawnTime);
        }

        internal void Shoot(int damage, float speed, float despawnTime, float fadeDuration)
        {
            _light = GetComponent<Light>();
            _fadeDuration = fadeDuration;

            base.Shoot(damage, speed, despawnTime);
        }

        protected override void End()
        {
            _rb.velocity = Vector3.zero;
            _col.enabled = false;
            DOTween.To(() => _light.intensity, x => _light.intensity = x, 0, _fadeDuration).OnComplete(() => base.End());
        }
    }
}