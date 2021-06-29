using UnityEngine;

namespace AMPR.Manager
{
    [CreateAssetMenu(menuName = "AMPR/Input Manager", fileName = "Input Handler")]
    public class InputHandler : ScriptableObject
    {
        public PlayerControls Controls { get => _controls ??= new PlayerControls(); }
        private PlayerControls _controls;

        private void Reset()
        {

        }

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

        private static void CreateInputManagerSO()
        {

        }
    }
}