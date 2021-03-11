using UnityEngine;

namespace Sjouke
{
    public abstract class Targetable : MonoBehaviour, ITargetable
    {
        protected PlayerController player;

        // Start is called before the first frame update
        private void Start()
        {
            player = FindObjectOfType<PlayerController>();

            player.RegisterTargetable(this);
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
