using AMPR.Interactable;
using UnityEngine;

namespace AMPR.Weapon
{
    internal abstract class BaseBullet : MonoBehaviour
    {
        public    float     Damage { get => damage; }

        protected bool      initialized = false;
        protected int       damage;
        protected float     speed;
        protected float     despawnTimer;
        protected LayerMask hittableLayers;

        internal virtual void Initialize(int damage, float speed, float despawnTime)
        {
            this.damage = damage;
            this.speed = speed;
            despawnTimer = despawnTime;

            initialized = true;
        }

        internal virtual void Shoot()
        {
            if (!initialized)
            {
                Debug.LogError($"Can't shoot bullet {transform.name} without initializing it!");
                return;
            }

            // TODO: Manual projectile translation and collision detection
        }

        protected virtual void Update()
        {
            if (despawnTimer < 0)
            {
                DestroyBullet();
                return;
            }

            UpdateBulletTimer();
        }

        protected virtual void UpdateBulletTimer() => despawnTimer -= Time.deltaTime;

        //protected void OnTriggerEnter(Collider col)
        protected void OnHit(Transform other)
        {
            Debug.Log($"Bullet {transform.name} collided with {other.name}");
            var interactable = other.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this);
                Debug.Log($"{transform.name} interacts with Interactable {other.name}");
            }

            DestroyBullet();
        }

        protected virtual void DestroyBullet()
        {
            Destroy(gameObject);
        }
    }
}