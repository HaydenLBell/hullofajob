using UnityEngine;

namespace cowsins
{
    public class AddonManager : MonoBehaviour
    {
        [HideInInspector] public bool isInventoryAddonAvailable;
        [HideInInspector] public bool isSaveLoadAddonAvailable;
        [HideInInspector] public bool isMultiplayerAddonAvailable;
        [HideInInspector] public bool isMirrorAddonAvailable;

        public static AddonManager instance;

        private void Awake()
        {
            instance = this;

#if INVENTORY_PRO_ADD_ON
            isInventoryAddonAvailable = true;
#else
            isInventoryAddonAvailable = false;
#endif
#if SAVE_LOAD_ADD_ON
            isSaveLoadAddonAvailable = true;
#else
            isSaveLoadAddonAvailable = false;
#endif

#if COWSINS_MULTIPLAYER
            isMultiplayerAddonAvailable = true;
#else
            isMultiplayerAddonAvailable = false;
#endif

#if COWSINS_MIRROR
            isMirrorAddonAvailable = true;
#else
            isMirrorAddonAvailable = false;
#endif
        }
    }

}
