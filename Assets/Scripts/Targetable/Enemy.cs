using System.Collections;
using System.Collections.Generic;
using AMPR.Controls;
using UnityEngine;

namespace AMPR
{
    public class Enemy : MonoBehaviour, ITarget
    {
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