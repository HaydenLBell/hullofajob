using UnityEngine;

namespace cowsins
{
    public class MessInteractable : Interactable, IRepairTask
    {
        [Tooltip("Hide the mess once it's cleaned.")]
        [SerializeField] private bool hideOnClean = true;

        public bool IsRepaired => alreadyInteracted;

        public override void Interact(Transform player)
        {
            if (alreadyInteracted) return;

            base.Interact(player); // sets alreadyInteracted and fires OnInteract
            RepairEvents.Notify();

            if (hideOnClean) gameObject.SetActive(false);
            else GetComponent<Collider>().enabled = false; // stop re-interacting
        }
    }
}