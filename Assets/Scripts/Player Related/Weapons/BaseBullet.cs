using AMPR.Controls;
using UnityEngine;

namespace AMPR.Weapon
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BaseBullet : MonoBehaviour
    {
        public float Damage { get => _damage; }

        protected int _damage;
        protected float _speed;
        protected float _despawnTimer;
        protected Rigidbody _rb;
        protected Collider _col;

        internal virtual bool Initialize(int damage, float speed, float despawnTime)
        {
            _damage = damage;
            _speed = speed;
            _despawnTimer = despawnTime;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _col = GetComponent<SphereCollider>();
            _col.isTrigger = true;
            return true;
        }

        internal virtual bool Shoot(int damage, float speed, float despawnTime)
        {
            Initialize(damage, speed, despawnTime);

            _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);
            return true;
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

        // protected void OnCollisionEnter(Collision col)
        // {
        //     if (col.transform.GetComponent<PlayerController>())
        //         return;

        //     Debug.Log($"Bullet collided with {_col.transform.name}");
        //     End();
        // }

        protected void OnTriggerEnter(Collider col)
        {
            if (col.transform.GetComponent<PlayerController>())
                return;

            End();
        }

        protected virtual void End()
        {
            Destroy(gameObject);
        }
    }
}