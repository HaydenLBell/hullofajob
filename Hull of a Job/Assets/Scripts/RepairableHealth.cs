/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
#endif
#if SAVE_LOAD_ADD_ON
using hb.framework.SaveLoad;
using System.Collections.Generic;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

namespace hb.framework
{
    /// <summary>
    /// Super simple enemy script that allows any object with this component attached to receive damage,aim towards the player and shoot at it.
    /// This is not definitive and it will 100% be modified and re structured in future updates
    /// </summary>
    public class RepairableHealth : Identifiable, IDamageable, IRepairTask
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnSpawn, OnDamaged, OnDeath;
        }

        [Tooltip("Name of the object. This will appear on the killfeed"), SerializeField]
        protected string _name;

        public string Name => _name;

        [SaveField] public float health;
        [SaveField] public float shield;

        public float Health => health;
        public float Shield => shield;

        [Tooltip("initial enemy health "), SerializeField]
        protected float maxHealth;


        [Tooltip("When the object dies, decide if it should be destroyed or not."), SerializeField] private bool destroyOnDie;

        [Tooltip("When the enemy dies, remove EnemyHealthComponent if true."), SerializeField] private bool destroyComponentOnDie;

        [SerializeField] private GameObject deathEffect;

        [Tooltip("Object that Contains Health & Shield Bar"), SerializeField]
        public GameObject enemyStatusContainer;

        [Tooltip("display enemy status via UI"), SerializeField]
        public Image healthBar;

        [Tooltip("If true, it will display the UI with the shield and health sliders."), SerializeField]
        private bool showUI;

        public bool showDamagePopUps;

        [Tooltip("If true, it will display the KillFeed UI."), SerializeField]
        protected bool showKillFeed;

        [SerializeField]
        protected AudioClip dieSFX;

        public Events events;

        protected bool isDead;

        public bool IsDead => isDead;

        public bool DestroyOnDie => destroyOnDie;
        public bool ShowUI => showUI;
        public Image HealthSlider => healthBar;

        public Material healthy_Mat;
        public Material damaged_Mat;

        [Tooltip("Objects whose material changes with health. Leave empty to use this object.")]
        public MeshRenderer[] objectsToRepair;

        [Tooltip("Hide the health bar when the object is at full health.")]
        public bool hideBarWhenFull = true;

        [Tooltip("Once fully repaired, switch this object to the Interactable layer.")]
        public bool becomesInteractable;

        [Tooltip("Name of the layer to switch to once repaired.")]
        public string interactableLayerName = "Interactable";

        public bool IsRepaired => health >= maxHealth;

        private bool wasRepaired;

        public virtual void Start()
        {
            // Spawn
            events.OnSpawn.Invoke();

            // UI 
            if (healthBar != null) healthBar.fillAmount = health / maxHealth;

            if (objectsToRepair == null || objectsToRepair.Length == 0)
            {
                objectsToRepair = new[] { GetComponent<MeshRenderer>() };
            }
            UpdateRepairState();
        }



        //public virtual void Damage(float _healAmount, bool isHeadshot) { }


        /// <summary>
        /// Since it is IDamageable, it can take damage, if a shot is landed, damage the enemy
        /// </summary>
        public virtual void Damage(float _healAmount, bool isHeadshot)
        {
            if (isDead) return;

            health = Mathf.Clamp(health - _healAmount, 0f, maxHealth);
            UpdateHealthBar();
            UpdateRepairState();

            events.OnDamaged.Invoke();

#if SAVE_LOAD_ADD_ON
    StoreData();
#endif
            if (health <= 0) Die();
        }

        private void UpdateRepairState()
        {

            bool isRepaired = health >= maxHealth;

            if (isRepaired != wasRepaired)
            {
                wasRepaired = isRepaired;
                RepairEvents.Notify();
            }

            // Materials
            Material mat = isRepaired ? healthy_Mat : damaged_Mat;
            foreach (MeshRenderer r in objectsToRepair)
            {
                if (r != null) r.material = mat;
            }

            // Health bar visibility
            if (hideBarWhenFull)
            {
                GameObject bar = enemyStatusContainer != null ? enemyStatusContainer
                               : healthBar != null ? healthBar.gameObject : null;
                if (bar != null) bar.SetActive(!isRepaired);
            }

            // Layer switch
            if (isRepaired && becomesInteractable)
            {
                int layer = LayerMask.NameToLayer(interactableLayerName);
                if (layer == -1)
                {
                    Debug.LogWarning($"Layer '{interactableLayerName}' doesn't exist.", this);
                    return;
                }
                SetLayerRecursively(gameObject, layer);
                SetLayerRecursively(this.gameObject, layer);
            }
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public virtual void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                if (healthLerpCoroutine != null) StopCoroutine(healthLerpCoroutine);
                healthLerpCoroutine = StartCoroutine(LerpBar(healthBar, healthBar.fillAmount, health / maxHealth, 0.2f));
            }
        }

        public virtual void Die()
        {
            isDead = true;
            // Custom event on damaged
            events.OnDeath.Invoke();

            if (enemyStatusContainer != null) enemyStatusContainer.SetActive(false);

            SoundManager.Instance.PlaySound(dieSFX, 0, 1, false);
            // Does it display killfeed on death? 
            if (showKillFeed)
            {
                UIEvents.onEnemyKilled?.Invoke(_name);
            }
            if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);

            if (destroyOnDie)
            {
                Destroy(this.gameObject);
            }
            else if(destroyComponentOnDie)
            {
                Destroy(this);
            }
        }

        private Coroutine healthLerpCoroutine;
        private Coroutine shieldLerpCoroutine;

        private IEnumerator LerpBar(Image bar, float startValue, float targetValue, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                if (bar == null) yield break;

                time += Time.deltaTime;
                float t = time / duration;
                bar.fillAmount = Mathf.Lerp(startValue, targetValue, t);
                yield return null;
            }

            bar.fillAmount = targetValue;
        }

        /// <summary>
        /// Restores this enemy to full health and shield, clearing the dead state.
        /// Useful for wave systems, round resets, quest mechanics, etc.
        /// Override to add custom revival effects.
        /// </summary>
        public virtual void Revive()
        {
            isDead = false;
            health = maxHealth;
            if (enemyStatusContainer != null) enemyStatusContainer.SetActive(true);

            // Stop any in-flight lerp coroutines and snap bars to their new values
            // immediately.  Using StartCoroutine here is fragile: the RPC and SyncVar
            // hooks both call UpdateHealthBar/Shield, and whichever runs last stops the
            // other's coroutine � leaving the bar stuck at a mid-lerp value on clients.
            if (healthLerpCoroutine != null) { StopCoroutine(healthLerpCoroutine); healthLerpCoroutine = null; }
            if (shieldLerpCoroutine != null) { StopCoroutine(shieldLerpCoroutine); shieldLerpCoroutine = null; }
            if (healthBar != null) healthBar.fillAmount = maxHealth > 0f ? health / maxHealth : 0f;
        }

#if SAVE_LOAD_ADD_ON
        // If dead, destroy the Enemy.
        public override void LoadedState()
{
    if (health <= 0 && destroyOnDie) Destroy(gameObject);
    else
    {
        if (healthBar != null) healthBar.fillAmount = health / maxHealth;
        UpdateMaterials();
    }
}

        // Store the enemy into the enemy health data
        public override void StoreData()
        {
            if (GameDataManager.instance == null) return;

            if (GameDataManager.instance.enemyHealthData == null)
            {
                GameDataManager.instance.enemyHealthData = new Dictionary<string, CustomSaveData>();
            }
            GameDataManager.instance.enemyHealthData[UniqueIDValue] = SaveFields();
        }
#endif
    }

}
