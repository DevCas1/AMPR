using AMPR.Weapon;
using DG.Tweening;
using UnityEngine;

namespace AMPR.Interactable
{
    public class Door : MonoBehaviour, IInteractable // TODO: Add spherical trigger. If player is within trigger, keep door open (should be very small but usable)
    {
        public enum DoorState { Open, Opening, Unlocking, Locked, Closed, Closing }
        public enum ShieldType { Power }

        public ShieldType DoorShieldType { get => _shieldtype; }

        [SerializeField, Header("References")]
        private Renderer[] _shieldRenderers = new Renderer[2];
        [SerializeField]
        private GameObject _doorPlates;
        [SerializeField]
        private CharacterController _player;
        [SerializeField, Header("Settings")]
        private float _fadeTime;
        [SerializeField]
        private ShieldType _shieldtype = ShieldType.Power;

        private DoorState _state = DoorState.Closed;
        private Tweener[] _fadeTweeners;
        private float _lastDistance;

        private void Start()
        {
            if (_player == null)
            {
                Debug.LogWarning("Player unasigned at runtime, finding player");
                _player = FindObjectOfType<CharacterController>();
            }

            _fadeTweeners = new Tweener[_shieldRenderers.Length];
        }

        private void Update()
        {
            if (_state == DoorState.Locked || _state == DoorState.Unlocking || _state == DoorState.Opening)
                return;

            if ((_player.transform.position - transform.position).sqrMagnitude - _lastDistance > 0.1f)
            {
                for (int index = 0; index < _shieldRenderers.Length; index++)
                    Close(index);
            }

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
            {
                for (int index = 0; index < _shieldRenderers.Length; index++)
                    Unlock(index); // TODO: And start adjacent room
            }
        }

        private void Open()
        {
            _state = DoorState.Opening;

            _doorPlates.SetActive(false); // TODO: Replace with opening animation

            _state = DoorState.Open;
        }

        private void Close(int index)
        {
            if (_fadeTweeners[index] != null)
            {
                _fadeTweeners[index].Kill(false);
                _fadeTweeners[index] = null;
            }

            _state = DoorState.Closing;

            _doorPlates.SetActive(true);

            _state = DoorState.Closed;

            Lock(index);
        }

        private void Unlock(int index)
        {
            _state = DoorState.Unlocking;

            if (_fadeTweeners[index] != null)
            {
                _fadeTweeners[index].Kill(false);
                _fadeTweeners[index] = null;
            }

            _fadeTweeners[index] = _shieldRenderers[index].material.DOFade(0, _fadeTime).OnComplete(() => { Open(); _fadeTweeners[index] = null; });
        }

        private void Lock(int index) => _fadeTweeners[index] = _shieldRenderers[index].material.DOFade(1, _fadeTime).OnComplete(() => { _state = DoorState.Closed; _fadeTweeners[index] = null; });
    }
}