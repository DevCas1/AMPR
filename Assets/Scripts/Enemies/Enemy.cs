using AMPR.Controls;
using UnityEngine;

namespace AMPR
{
    public abstract class Enemy : MonoBehaviour, ITarget
    {
        public int Health { get; private set; }

        public PlayerController Player;

        public void OnBecomeVisible()
        {
            Player.RegisterTarget(this);
        }

        public void OnBecomeInvisible()
        {
            Player.UnregisterTarget(this);
        }
    }
}