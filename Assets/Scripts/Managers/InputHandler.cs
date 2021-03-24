using UnityEngine;

namespace AMPR
{
    public class InputHandler : MonoBehaviour
    {
        public PlayerControls Controls => _controls ??= new PlayerControls();

        private PlayerControls _controls;

        private void Start() // TODO:Integrate Cursor Lockstate with Menu UI
        {
            Controls.Player.Cancel.performed += context => RemoveCursorLockState();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private static void RemoveCursorLockState() => Cursor.lockState = CursorLockMode.None;
    }
}