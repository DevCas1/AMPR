using UnityEngine;

namespace AMPR.Interactable
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        public abstract void Interact();
    }
}