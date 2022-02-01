using AMPR.Interactable;
using UnityEngine;

namespace AMPR.Weapon
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    internal abstract class BaseBullet : MonoBehaviour
    {
        public float Damage { get => _damage; }

        protected bool _initialized = false;
        protected int _damage;
        protected float _speed;
        protected float _despawnTimer;
        protected Rigidbody _rb;
        protected Collider _col;

        internal virtual void Initialize(int damage, float speed, float despawnTime)
        {
            _damage = damage;
            _speed = speed;
            _despawnTimer = despawnTime;
            _rb = GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _col = GetComponent<SphereCollider>();

            _col.isTrigger = true;
            _initialized = true;
        }

        internal virtual void Shoot()
        {
            if (!_initialized)
            {
                Debug.LogError($"Can't shoot bullet {transform.name} without initializing it!");
                return;
            }
            _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);
        }

        internal virtual void Shoot(int damage, float speed, float despawnTime)
        {
            Initialize(damage, speed, despawnTime);
            _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);
        }

        protected virtual void Update()
        {
            if (_despawnTimer < 0)
            {
                End();
                return;
            }

            _despawnTimer -= Time.deltaTime;
        }

        protected void OnTriggerEnter(Collider col)
        {
            Debug.Log($"Bullet {transform.name} collided with {col.transform.name}");
            var interactable = col.transform.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this);
                Debug.Log($"{transform.name} interacts with Interactable {col.transform.name}");
            }

            End();
        }

        protected virtual void End()
        {
            Destroy(gameObject);
        }
    }
}