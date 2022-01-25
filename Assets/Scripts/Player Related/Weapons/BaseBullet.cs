using AMPR.Controls;
using UnityEngine;

namespace AMPR.Weapon
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BaseBullet : MonoBehaviour
    {
        public float Damage { get => _damage; }

        [SerializeField, Min(0)]
        private int _damage;
        [SerializeField, Min(0)]
        private int _speed;

        private Rigidbody _rb;

        private void Reset() => Initialize();

        private void Awake() => Initialize();

        // private void Start() => Initialize(); // Doesn't trigger after instantiation

        private void Initialize()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;

            GetComponent<SphereCollider>().isTrigger = true;
        }

        internal void Shoot() => _rb.AddForce(transform.forward * _speed, ForceMode.VelocityChange);

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.GetComponent<PlayerController>())
                return;

            Destroy(gameObject);
            Debug.Log("Bullet destroyed");
        }
    }
}