using UnityEngine;
using System.Collections.Concurrent;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if INVENTORY_PRO_ADD_ON
using cowsins.Inventory;
#endif
namespace hb.framework
{
    // Disallow creating Items if the Inventory Pro Add-On is not available
    [System.Serializable]
#if INVENTORY_PRO_ADD_ON
[CreateAssetMenu(fileName = "newInventoryItem", menuName = "COWSINS/Inventory/New Inventory Item", order = 1)]
#endif
    public class Item_SO : ScriptableObject
    {
        [Tooltip("Your item name. Ex: Glock")] public string _name;
        [Tooltip("You can describe your item here. Inventory Add-On will show this description on hovering slots in the inventory.")] public string description;
        [Tooltip("Custom image of your item")] public Sprite icon;
        [Tooltip("Visuals that will appear on a dropped item.")] public GameObject pickUpGraphics;
        [Range(1, 99)] public int maxStack = 1;
        public Vector2Int itemSize = new Vector2Int(1, 1);
        public Sprite irregularItemIcon;

        [Tooltip("Velocity will be decreased depending on the weight of the weapon if this is true.")] public bool applyWeight;
        [Range(.2f, 1)]
        [Tooltip("Represents the weight of an entire stack of this item.")] public float weightMultiplier = 1f;

#if INVENTORY_PRO_ADD_ON
        [Header("Durability")]
        [SerializeField] private bool useDurability = false;

        [SerializeField, Min(1f)] private float maxDurability = 100f;

        [SerializeField] private DurabilityReductionMethod durabilityMethod = DurabilityReductionMethod.Usage;

        public bool UseDurability => useDurability;
        public float MaxDurability => maxDurability;
        public DurabilityReductionMethod DurabilityMethod => durabilityMethod;

        public virtual void OnDurabilityReduced(InventorySlot slot, float previousDurability, float currentDurability) {}
        public virtual void OnDurabilityDepleted(InventorySlot slot, PlayerDependencies player) {}

        public virtual void Use(InventoryProManager inventoryProManager, InventorySlot inventorySlot)
        {
            if (inventorySlot == null || inventoryProManager == null) return;

            if (useDurability && durabilityMethod == DurabilityReductionMethod.Usage && inventorySlot.AllowsDurability)
                inventorySlot.ReduceDurability(1f);

            InventorySlot anchor = inventorySlot.GetAnchorSlot() ?? inventorySlot;
            if (anchor == null || anchor.slotData == null || anchor.slotData.item == null) return;

            bool affectsWeight = anchor.Policy != null && anchor.Policy.AffectsPlayerWeight;

            anchor.slotData.amount -= 1;

            if (affectsWeight)
                inventoryProManager._GridGenerator?.WeightCalculator?.UpdateCurrentWeight(this, -1);

            if (anchor.slotData.amount <= 0)
            {
                if (inventoryProManager._GridGenerator?.Operations != null)
                    inventoryProManager._GridGenerator.Operations.ClearSlotArea(anchor);
                else
                    anchor.ClearSlot();

                anchor.Policy?.OnItemRemoved(anchor, inventoryProManager);
                return;
            }

            if (anchor.Policy != null && anchor.Policy.IsGridBased && inventoryProManager.DataModel != null) inventoryProManager.DataModel.NotifySlotAreaChanged(anchor);
            else anchor.UpdateSlotGraphics();

            anchor.Policy?.OnSlotModified(anchor, inventoryProManager);
        }

        public virtual void ApplyCustomSlotEffects(InventorySlot slot, PlayerDependencies player) {}

        public virtual void RemoveCustomSlotEffects(InventorySlot slot, PlayerDependencies player){}

        public virtual void OnEquipCustomSlot(InventorySlot slot, PlayerDependencies player) => ApplyCustomSlotEffects(slot, player);
        public virtual void OnUnequipCustomSlot(InventorySlot slot, PlayerDependencies player) => RemoveCustomSlotEffects(slot, player);

        private static readonly ConcurrentDictionary<System.Type, bool> useOverrideCache = new ConcurrentDictionary<System.Type, bool>();

        /// <summary>
        /// Determines whether this item can be used in the given slot
        /// </summary>
        public virtual bool CanUse(InventorySlot slot, InventoryProManager manager)
        {
            System.Type type = GetType();
            if (!useOverrideCache.TryGetValue(type, out bool overridden))
            {
                var method = type.GetMethod(nameof(Use));
                var baseMethod = typeof(Item_SO).GetMethod(nameof(Use));
                overridden = method != null && baseMethod != null && method.DeclaringType != baseMethod.DeclaringType;
                useOverrideCache[type] = overridden;
            }
            return overridden;
        }

        public virtual string GetContextActionName(InventorySlot slot, InventoryProManager manager) => "USE";

        public virtual bool ShowUseToast => true;

        public virtual string GetUseToastMessage() => $"{_name} {ToastManager.Instance?.ItemUsed}";

        public virtual SlotData[] CreateDefaultContainedItems() => null;
#endif
    }
}

#if UNITY_EDITOR

namespace hb.framework
{
    [CustomEditor(typeof(Item_SO), true)]
    public class Item_SOEditor : Editor
    {
        private bool isInventoryAddonAvailable;
        private void OnEnable()
        {
            // Check if the Inventory Addon is available
            #if INVENTORY_PRO_ADD_ON
            isInventoryAddonAvailable = true;
            #else
            isInventoryAddonAvailable = false;
            #endif
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            var myScript = target as Item_SO;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pickUpGraphics"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyWeight"));
            if (myScript.applyWeight)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weightMultiplier"));
                EditorGUI.indentLevel--;
            }
            else myScript.weightMultiplier = 1;

            GUI.enabled = isInventoryAddonAvailable;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("INVENTORY PRO MANAGER", EditorStyles.boldLabel);
            if (!isInventoryAddonAvailable)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("These features are locked. Inventory Pro Manager + Save & Load Add-On is not installed.", MessageType.Info);
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStack"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemSize"));
            if (myScript.itemSize.x != myScript.itemSize.y)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox($"Your item has an irregular size [{myScript.itemSize.x} : {myScript.itemSize.y}], which may cause icon stretching." +
                "To maintain the correct aspect ratio, consider setting 'irregularItemIcon' with a properly scaled icon.", MessageType.Info);
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("irregularItemIcon"));
                EditorGUI.indentLevel--;
            }
            GUI.enabled = true;
#if INVENTORY_PRO_ADD_ON
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("DURABILITY SETTINGS", EditorStyles.boldLabel);
            var useDurabilityProp = serializedObject.FindProperty("useDurability");
            if (useDurabilityProp != null)
            {
                EditorGUILayout.PropertyField(useDurabilityProp);
                if (useDurabilityProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDurability"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("durabilityMethod"));
                    EditorGUI.indentLevel--;
                }
            }
#endif
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("SPECIFIC SETTINGS", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, "m_Script", "id", "icon", "_name", "pickUpGraphics", "applyWeight", "weightMultiplier", "maxStack", "itemSize", "irregularItemIcon", "useDurability", "maxDurability", "durabilityMethod");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
