using UnityEngine;

namespace hb.framework
{
    public interface IInteractAuthority
    {
        void RequestInteract(Transform player);
    }
}
