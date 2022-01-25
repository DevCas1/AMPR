using UnityEngine;

namespace AMPR.Interactable
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        public virtual void Interact(Component other)
        {
            throw new System.NotImplementedException();
        }
    }
}