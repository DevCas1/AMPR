using AMPR.Controls;
using AMPR.Weapon;
using DG.Tweening;
using UnityEngine;

namespace AMPR.Interactable
{
    public class Door : MonoBehaviour, IInteractable
    {
        public enum DoorState { Open, Opening, Unlocking, Locked, Closed, Closing }
        public enum ShieldType { Power }

        public ShieldType DoorShieldType { get => _shieldtype; }

        [SerializeField, Header("References")]
        private Renderer _shieldRenderer;
        [SerializeField]
        private Collider _collider;
        [SerializeField]
        private GameObject _doorPlates;
        [SerializeField]
        private CharacterController _player;
        [SerializeField, Header("Settings")]
        private float _fadeTime;
        [SerializeField]
        private ShieldType _shieldtype = ShieldType.Power;

        private DoorState _state = DoorState.Closed;
        private Tweener _fadeTweener;
        private float _lastDistance;

        private void Start()
        {
            if (_player == null)
            {
                Debug.LogWarning("Player unasigned at runtime, finding player");
                _player = FindObjectOfType<CharacterController>();
            }
        }

        private void Update()
        {
            if (_state == DoorState.Locked || _state == DoorState.Unlocking || _state == DoorState.Opening)
                return;

            if ((_player.transform.position - transform.position).sqrMagnitude - _lastDistance > 0.1f) // If door is anything but closed and the player is moving away from the door
                Close();

            _lastDistance = (_player.transform.position - transform.position).sqrMagnitude;
        }

        public void Interact(Component other)
        {
            if (!other.GetComponent<BaseBullet>())
            {
                Debug.Log($"{other.name} was no bullet, returning...");
                return;
            }

            if (_state is DoorState.Locked or DoorState.Closed or DoorState.Closing)
                Unlock(); // TODO: And start adjacent room
        }

        private void Open()
        {
            _state = DoorState.Opening;

            _doorPlates.SetActive(false); // TODO: Replace with opening animation

            _collider.enabled = false;
            _state = DoorState.Open;
        }

        private void Close()
        {
            if (_fadeTweener != null)
                _fadeTweener.Complete(false);

            _state = DoorState.Closing;

            _doorPlates.SetActive(true);

            _collider.enabled = true;
            _state = DoorState.Closed;

            Lock();
        }

        private void Unlock()
        {
            _state = DoorState.Unlocking;

            if (_fadeTweener != null)
                _fadeTweener.Complete(false);

            _fadeTweener = _shieldRenderer.material.DOFade(0, _fadeTime).OnComplete(() => { Open(); _fadeTweener = null; });
        }

        private void Lock() => _fadeTweener = _shieldRenderer.material.DOFade(1, _fadeTime).OnComplete(() => { _state = DoorState.Closed; _collider.enabled = true; _fadeTweener = null; });
    }
}