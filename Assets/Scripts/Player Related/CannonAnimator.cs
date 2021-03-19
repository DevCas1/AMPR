using DG.Tweening;
using UnityEngine;

namespace AMPR
{
    public class CannonAnimator : MonoBehaviour // TODO: Implement rotation over time
    {
        [Header("References")]
        public InputHandler InputHandler;

        [Header("Positioning")]
        public float PunchForce = 0.5f;
        public float PunchDuration = 0.5f;
        public float PunchElasticity = 0.5f;

        // [Header("Rotation related")]
        // public float RotationRange;
        // public float RotationDuration;

        private bool _punchActive;
        private Tweener _punchTweener;
        // private float _repeatedFire;
        // [SerializeField]
        // private float _minRepeatedFre;


        // Start is called before the first frame update
        private void Start()
        {
            InputHandler.Controls.Player.Fire.performed += context => OnCannonFire();
            DebugUtility.HandleErrorIfNullGetComponent<InputHandler, CannonAnimator>(InputHandler, this, gameObject);
        }

        private void OnCannonFire()
        {
            if (_punchActive)
                _punchTweener.Complete(false);

            // _repeatedFire += 1;

            // if (_repeatedFire > _minRepeatedFre)
            // {
            //     float range = Random.Range(-RotationRange, RotationRange);
            //     transform.DOLocalRotate(new Vector3(range, 0, 0), RotationDuration);
            // }

            _punchTweener = transform.DOPunchPosition(new Vector3(0, 0, -PunchForce), PunchDuration, 0, PunchElasticity)
                                     .OnStart(() => _punchActive = true)
                                     .OnComplete(() => _punchActive = false);
                                 //  .OnComplete(OnPunchComplete);
        }

        // private void OnPunchComplete()
        // {
        //     _repeatedFire = 0;
        //     transform.DORotate(Vector3.zero, RotationDuration);
        //     _punchActive = false;
        // }
    }
}