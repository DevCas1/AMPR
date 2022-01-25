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

        internal void Shoot(int damage, float speed, float despawnTime)
        {
            _damage = damage;
            _speed = speed;
            _despawnTimer = despawnTime;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            GetComponent<SphereCollider>().isTrigger = true;

            _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);
        }

        private void Update()
        {
            _despawnTimer -= Time.deltaTime;
            if (_despawnTimer < 0)
                End();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.GetComponent<PlayerController>())
                return;

            End();
        }

        private void End()
        {
            Destroy(gameObject);
            Debug.Log("Bullet destroyed");
        }
    }
}