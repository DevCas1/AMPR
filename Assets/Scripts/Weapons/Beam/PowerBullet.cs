using DG.Tweening;
using UnityEngine;

namespace AMPR.Weapon
{
    internal class PowerBullet : BaseBullet
    {

        [SerializeField] private     float bulletRadius;

                         private new Light light;
                         private     float fadeDuration;

        internal void Initialize(int damage, float speed, float despawnTime, float fadeDuration)
        {
            light = GetComponent<Light>();
            this.fadeDuration = fadeDuration;

            base.Initialize(damage, speed, despawnTime);
        }

        protected override void Update()
        {
            base.Update();

            transform.Translate(speed * Time.deltaTime * transform.forward);
        }

        protected void FixedUpdate()
        {
            if (Physics.SphereCast(transform.position, bulletRadius, transform.forward, out RaycastHit hit, hittableLayers))
            {
                OnHit(hit.transform);
            }
        }

        internal override void Shoot()
        {
            light = GetComponent<Light>();
            base.Shoot();
        }

        protected override void DestroyBullet()
        {
            DOTween.To(() => light.intensity, x => light.intensity = x, 0, fadeDuration).OnComplete(() => base.DestroyBullet());
        }
    }
}