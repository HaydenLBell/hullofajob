using UnityEngine;
using TMPro;

namespace cowsins
{
    // Handles combat events such as the Killfeed, Hitmarkers, and Damage Pop-Ups
    public class CombatEventsHUDModule : MonoBehaviour, IHUDModule
    {
        [SerializeField] private Hitmarker hitmarker;

        [Title("Killfeed")]
        [Tooltip("An object showing death events will be displayed on kill"), SerializeField] private bool displayEvents;
        [Tooltip("UI element which contains the killfeed. Where the kilfeed object will be instantiated and parented to"), SerializeField]
        private GameObject killfeedContainer;
        [Tooltip("Object to spawn"), SerializeField] private GameObject killfeedObject;
        [SerializeField] private string killfeedMessage;

        [Title("Damage Pop Ups")]
        [Tooltip("Add a pop up showing the damage that has been dealt. Recommendation: use the already made pop up included in this package. "), SerializeField]
        private GameObject damagePopUp;
        [Tooltip("Horizontal randomness variation"), SerializeField] private float xVariation;
        [Tooltip("Vertical offset for the damage pop up"), SerializeField] private float yOffset;
        
        [SerializeField] private bool useDynamicColorScheme;
        [SerializeField] private Color healthColor = Color.white;
        [SerializeField] private Color shieldColor = Color.blue;
        [SerializeField] private Color critColor = Color.red;

        public void Initialize(PlayerDependencies dependencies)
        {
            UIEvents.onEnemyHit += Hitmarker;
            UIEvents.onEnemyKilled += AddKillfeed;
        }

        private void OnDestroy()
        {
            UIEvents.onEnemyHit -= Hitmarker;
            UIEvents.onEnemyKilled -= AddKillfeed;
        }

        public void AddKillfeed(string name)
        {
            if (!displayEvents) return;
            if(killfeedContainer == null || killfeedObject == null) return;

            GameObject killfeed = PoolManager.Instance.GetFromPool(killfeedObject, transform.position, Quaternion.identity);
            killfeed.transform.SetParent(killfeedContainer.transform);
            killfeed.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>().text = $"{killfeedMessage} {name}";
        }

        public void Hitmarker(bool headshot, bool damagePopUpParam, Vector3 position, float damage, bool isShield)
        {
            if (hitmarker != null)
                hitmarker.Play(headshot);

            if (damagePopUpParam) 
                AddDamagePopUp(position, damage, isShield, headshot);
        }

        public void AddDamagePopUp(Vector3 position, float damage, bool isShield, bool isHeadshot = false)
        {
            if (damagePopUp == null) return;
            
            float xRand = UnityEngine.Random.Range(-xVariation, xVariation);
            Vector3 posculatedPos = position + new Vector3(xRand, yOffset, 0);
            GameObject popup = PoolManager.Instance.GetFromPool(damagePopUp, posculatedPos, Quaternion.identity, .4f);
            
            if (popup == null) return;

            TMP_Text text = popup.transform.GetChild(0).GetComponent<TMP_Text>();
            if (damage / Mathf.FloorToInt(damage) == 1)
                text.text = damage.ToString("F0");
            else
                text.text = damage.ToString("F1");
            
            if (useDynamicColorScheme)
            {
                if (isHeadshot)
                    text.color = critColor;
                else
                    text.color = isShield ? shieldColor : healthColor; 
            }
        }
    }
}

#if UNITY_EDITOR
namespace cowsins 
{
    [UnityEditor.CustomEditor(typeof(CombatEventsHUDModule))]
    public class CombatEventsHUDModuleEditor : HUDModuleEditorBase
    {
        public override void DrawModuleProperties()
        {
            serializedObject.Update();
            CombatEventsHUDModule myScript = (CombatEventsHUDModule)target;

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("hitmarker"));
            
            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("displayEvents"));

            if (serializedObject.FindProperty("displayEvents").boolValue)
            {
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedContainer"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedObject"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("killfeedMessage"));
                
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("damagePopUp"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("xVariation"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("yOffset"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("useDynamicColorScheme"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("healthColor"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldColor"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("critColor"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
