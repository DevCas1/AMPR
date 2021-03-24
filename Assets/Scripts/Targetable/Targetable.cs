using UnityEngine;

namespace AMPR
{
    public abstract class Targetable : MonoBehaviour, ITargetable
    {
        protected PlayerController Player;

        // Start is called before the first frame update
        private void Start()
        {
            Player = FindObjectOfType<PlayerController>();

            Player.RegisterTargetable(this);
        }

        public void OnBecameInvisible()
        {
            throw new System.NotImplementedException();
        }

        public void OnBecameVisible()
        {
            throw new System.NotImplementedException();
        }
    }
}
