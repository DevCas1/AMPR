using System.Collections;
using System.Collections.Generic;
using AMPR.PlayerController;
using UnityEngine;

namespace AMPR
{
    public class Enemy : MonoBehaviour, ITargetable
    {
        public Controls.PlayerController Player;

        public void OnBecomeInvisible()
        {
            throw new System.NotImplementedException();
        }

        public void OnBecomeVisible()
        {
            throw new System.NotImplementedException();
        }
    }
}