using DG.Tweening;
using UnityEngine;

namespace AMPR
{
    public class CannonAnimator : MonoBehaviour // TODO: Implement local rotation over time by high rate of fire
    {
        [Header("References")]
        public InputHandler InputHandler;

        [Header("Positioning")]
        public float PunchForce = 0.5f;
        public float PunchDuration = 0.5f;
        public float PunchElasticity = 0.5f;

        private bool _punchActive;

        private Tweener _punchTweener;


        private void Start()
        {
            InputHandler.Controls.Player.Fire.performed += context => OnCannonFire();
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, CannonAnimator>(InputHandler, this, gameObject);
        }

        private void OnCannonFire()
        {
            if (_punchActive)
                _punchTweener.Complete(false);

            TweenCallback SetPunchActive(bool active)
            {
                _punchActive = active;
                return null;
            }

            TweenCallback RemoveReference()
            {
                _punchTweener = null;
                return null;
            }

            _punchTweener = transform.DOPunchPosition(new Vector3(0, 0, -PunchForce), PunchDuration, 0, PunchElasticity)
                .OnStart(SetPunchActive(true))
                .OnComplete(SetPunchActive(false))
                .OnKill(RemoveReference());
        }
    }
}