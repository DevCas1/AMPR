using AMPR.Weapon;
using UnityEngine;

namespace AMPR.Interactable
{
    public class Door : MonoBehaviour, IInteractable
    {
        public enum DoorState { Open, Opening, Closed, Closing }
        public enum ShieldType { Power }

        public ShieldType DoorShieldType { get => _shieldtype; }

        [SerializeField]
        private ShieldType _shieldtype = ShieldType.Power;

        private DoorState _state = DoorState.Closed;

        public void Interact(Component other)
        {
            if (!other.GetComponent<BaseBullet>())
            {
                Debug.Log($"{other.name} was no bullet, returning...");
                return;
            }

            if (_state is DoorState.Closed or DoorState.Closing)
                Open();
        }

        private void Open()
        {

        }

        private void Close()
        {

        }
    }
}