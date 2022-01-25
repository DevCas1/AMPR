using UnityEngine;

namespace AMPR.Interactable
{
    public interface IInteractable
    {
        void Interact(Component other);
    }
}