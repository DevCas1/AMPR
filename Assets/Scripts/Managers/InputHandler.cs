using UnityEngine;

namespace AMPR.Manager
{
    public class InputHandler : MonoBehaviour
    {
        public PlayerControls Controls { get => _controls; }
        private PlayerControls _controls;

        private void Awake() => InitPlayerControls();

        private void Start() // TODO:Integrate Cursor Lockstate with Menu UI
        {
            InitPlayerControls();
            Controls.Player.Cancel.performed += context => RemoveCursorLockState();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void InitPlayerControls()
        {
            if (_controls == null)
                _controls = new PlayerControls();
        }

        private static void RemoveCursorLockState() => Cursor.lockState = CursorLockMode.None;
    }
}