using AMPR.Controls;
using UnityEngine;

namespace AMPR.Weapon
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BaseBullet : MonoBehaviour
    {
        public float Damage { get => _damage; }

        private int _damage;
        private float _speed;
        private float _despawnTimer;
        private Rigidbody _rb;

        internal virtual void Initialize(int damage, float speed, float despawnTime)
        {
            _damage = damage;
            _speed = speed;
            _despawnTimer = despawnTime;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        internal virtual void Shoot(int damage, float speed, float despawnTime)
        {
            Initialize(damage, speed, despawnTime);

            _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);
        }

        protected virtual void Update()
        {
            _despawnTimer -= Time.deltaTime;
            if (_despawnTimer < 0)
                End();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.transform.GetComponent<PlayerController>())
                return;

            End();
        }

        protected virtual void End()
        {
            Destroy(gameObject);
            Debug.Log("Bullet destroyed");
        }
    }
}