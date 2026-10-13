/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cowsins
{
    public class Crosshair : MonoBehaviour, IHUDModule
    {
        [System.Serializable]   
        public class CrosshairEvents
        {
            public UnityEvent OnEnemySpotted, OnVisibilityChanged, OnCrosshairResized, OnCrosshairReset;
        }

        #region variables

        private PlayerDependencies playerDependencies;
        [Title("Settings"), Tooltip("Parent transform for the crosshair UI. If null, a Canvas will be created dynamically."), SerializeField] private Transform crosshairParent;
        [SerializeField, Tooltip("If true, the crosshair will resize to the default spread even if shooting.")] private bool resizeToDefaultIfShooting;
        [SerializeField, Tooltip("If enabled, the crosshair will not be displayed when the game is paused.")] private bool hideCrosshairOnPaused;
        [SerializeField, Tooltip("If enabled, the crosshair will not be displayed when the player is inspecting.")] private bool hideCrosshairOnInspecting;

        [Title("Dimensions"), Tooltip(" How much space it takes from your screen"), SerializeField]
        private float size = 10f;

        [Tooltip(" Thickness of the crosshair  "), SerializeField]
        private float width = 2f;

        [Tooltip(" Original spread you want to start with "), SerializeField]
        private float defaultSpread = 10f;

        [Title("Spread Settings"), SerializeField] private bool resizeCrosshair;
        [SerializeField] private float walkSpread, runSpread, crouchSpread, jumpSpread;
        [SerializeField, Tooltip("Do not draw the crosshair when aiming a weapon")] private bool removeCrosshairOnAiming;

        [Title("Color Settings"), Tooltip(" Crosshair Color "), SerializeField]
        private Color defaultColor;

        [Tooltip(" Color of the crosshair whenever you aim at an enemy "), SerializeField]
        private Color enemySpottedColor;

        [SerializeField] private float enemySpottedWidth;

        [Title("Speed"), SerializeField] private float resizeSpeed = 3f;

        [Title("Outline Settings"), SerializeField] private bool useOutline = true;
        [SerializeField] private Color outlineColor = new Color(0, 0, 0, 0.5f);
        [SerializeField] private Vector2 outlineDistance = new Vector2(1, -1);

        [SerializeField, Title("Events", upMargin = 10)] private CrosshairEvents crosshairEvents;

        private IPlayerStatsProvider playerStatsProvider; // IPlayerStatsProvider is implemented in PlayerStats.cs
        private IPlayerMovementStateProvider playerProvider; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private IWeaponReferenceProvider weaponController; // IWeaponReferenceProvider is implemented in WeaponController.cs
        private IWeaponBehaviourProvider weaponBehaviour; // IWeaponBehaviourProvider is implemented in WeaponController.cs
        private IWeaponEventsProvider weaponEvents; // IWeaponEventsProvider is implemented in WeaponController.cs
        private IInteractManagerProvider interactManager; // IInteractManagerProvider is implemented in InteractManager.cs
        private CrosshairShape crosshairShape;

        private bool isVisible = true;
        private float spread;
        private float originalWidth;
        private Color color = Color.grey;

        private Transform crosshairContainer;
        private Image centerImage, topImage, downImage, leftImage, rightImage;
        private Image topLeftBracket1, topLeftBracket2, topRightBracket1, topRightBracket2;
        private Image bottomLeftBracket1, bottomLeftBracket2, bottomRightBracket1, bottomRightBracket2;
        public bool IsVisible => isVisible;

        #endregion

        private void Awake()
        {
            ResetCrosshair();

            crosshairShape = GetComponent<CrosshairShape>();

            CreateCrosshairUI();
        }

        private void CreateCrosshairUI()
        {
            Transform rootParent = crosshairParent;

            if (rootParent == null)
            {
                GameObject canvasObj = new GameObject("Crosshair Canvas");
                canvasObj.transform.SetParent(transform);
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                rootParent = canvasObj.transform;
            }

            GameObject containerObj = new GameObject("Crosshair Elements");
            containerObj.transform.SetParent(rootParent, false);
            crosshairContainer = containerObj.transform;

            centerImage = CreateImage("Center");
            topImage = CreateImage("Top");
            downImage = CreateImage("Down");
            leftImage = CreateImage("Left");
            rightImage = CreateImage("Right");

            topLeftBracket1 = CreateImage("TopLeftBracket1");
            topLeftBracket2 = CreateImage("TopLeftBracket2");
            topRightBracket1 = CreateImage("TopRightBracket1");
            topRightBracket2 = CreateImage("TopRightBracket2");
            bottomLeftBracket1 = CreateImage("BottomLeftBracket1");
            bottomLeftBracket2 = CreateImage("BottomLeftBracket2");
            bottomRightBracket1 = CreateImage("BottomRightBracket1");
            bottomRightBracket2 = CreateImage("BottomRightBracket2");
        }

        private Image CreateImage(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(crosshairContainer, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;

            if (useOutline)
            {
                Outline outline = obj.AddComponent<Outline>();
                outline.effectColor = outlineColor;
                outline.effectDistance = outlineDistance;
            }

            return img;
        }

        public void Initialize(PlayerDependencies dependencies)
        {
            this.playerDependencies = dependencies;
            playerStatsProvider = playerDependencies.PlayerStats;
            playerProvider = playerDependencies.PlayerMovementState;
            weaponController = playerDependencies.WeaponReference;
            weaponBehaviour = playerDependencies.WeaponBehaviour;
            weaponEvents = playerDependencies.WeaponEvents;
            interactManager = playerDependencies.InteractManager;

            weaponEvents.Events.OnShootHitscanProjectile.AddListener(Resize);
            weaponEvents.Events.OnShoot.AddListener(HideEnemySpotted);
            weaponEvents.Events.OnEnemySpotted.AddListener(SpotEnemy);
        }

        private void OnDestroy()
        {
            if (weaponEvents != null && weaponEvents.Events != null)
            {
                weaponEvents.Events.OnShootHitscanProjectile.RemoveListener(Resize);
                weaponEvents.Events.OnShoot.RemoveListener(HideEnemySpotted);
                weaponEvents.Events.OnEnemySpotted.RemoveListener(SpotEnemy);
            }
        }

        private void Update()
        {
            if (weaponBehaviour == null) return;

            // If we are shooting do not continue
            if (weaponBehaviour.IsShooting && !resizeToDefaultIfShooting) return;   

            if (spread != defaultSpread) spread = Mathf.MoveTowards(spread, defaultSpread, resizeSpeed * Time.deltaTime / 10); // if this is not the current spread, fall back to the original one

            // Manage different sizes
            if (playerProvider.Grounded)
            {
                if (playerProvider.CurrentSpeed == playerProvider.RunSpeed && !playerProvider.IsIdle) Resize(runSpread);
                else
                {
                    if (playerProvider.CurrentSpeed == playerProvider.WalkSpeed)
                    {
                        if (playerProvider.IsIdle) Resize(defaultSpread);
                        else Resize(walkSpread);
                    }

                    if (playerProvider.CurrentSpeed == playerProvider.CrouchSpeed) Resize(crouchSpread);
                }
            }
            else Resize(jumpSpread);
        }

        private void LateUpdate()
        {
            if (playerStatsProvider == null || weaponController == null) return;
            
            bool shouldBeVisible = isVisible 
                && !playerStatsProvider.IsDead
                && !(weaponController.Weapon != null && weaponBehaviour.IsAiming && removeCrosshairOnAiming)
                && !(PauseMenu.Instance != null && PauseMenu.isPaused && hideCrosshairOnPaused)
                && !(interactManager.Inspecting && hideCrosshairOnInspecting);

            if (!shouldBeVisible)
            {
                ToggleImages(false);
                return;
            }

            ToggleImages(true);

            CrosshairParts parts = crosshairShape.CurrentCrosshairParts;
            if (parts == null) return;

            UpdateImageRect(centerImage, parts.center, Vector2.zero, new Vector2(Mathf.Min(width, size), Mathf.Min(width, size)));
            UpdateImageRect(downImage, parts.downPart, new Vector2(0, -spread / 2 - size / 2), new Vector2(width, size));
            UpdateImageRect(topImage, parts.topPart, new Vector2(0, spread / 2 + size / 2), new Vector2(width, size));
            UpdateImageRect(rightImage, parts.rightPart, new Vector2(spread / 2 + size / 2, 0), new Vector2(size, width));
            UpdateImageRect(leftImage, parts.leftPart, new Vector2(-spread / 2 - size / 2, 0), new Vector2(size, width));

            UpdateImageRect(topLeftBracket1, parts.topLeftBracket, new Vector2(-spread + size/2, spread - width/2), new Vector2(size, width));
            UpdateImageRect(topLeftBracket2, parts.topLeftBracket, new Vector2(-spread + width/2, spread - size/2), new Vector2(width, size));

            UpdateImageRect(topRightBracket1, parts.topRightBracket, new Vector2(spread - size/2, spread - width/2), new Vector2(size, width));
            UpdateImageRect(topRightBracket2, parts.topRightBracket, new Vector2(spread - width/2, spread - size/2), new Vector2(width, size));

            UpdateImageRect(bottomLeftBracket1, parts.bottomLeftBracket, new Vector2(-spread + size/2, -spread + width/2), new Vector2(size, width));
            UpdateImageRect(bottomLeftBracket2, parts.bottomLeftBracket, new Vector2(-spread + width/2, -spread + size/2), new Vector2(width, size));

            UpdateImageRect(bottomRightBracket1, parts.bottomRightBracket, new Vector2(spread - size/2, -spread + width/2), new Vector2(size, width));
            UpdateImageRect(bottomRightBracket2, parts.bottomRightBracket, new Vector2(spread - width/2, -spread + size/2), new Vector2(width, size));
        }

        private void UpdateImageRect(Image img, bool active, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (img.gameObject.activeSelf != active)
                img.gameObject.SetActive(active);

            if (active)
            {
                img.color = color;
                img.rectTransform.anchoredPosition = anchoredPosition;
                img.rectTransform.sizeDelta = sizeDelta;
            }
        }

        private void ToggleImages(bool active)
        {
            if (crosshairContainer.gameObject.activeSelf != active)
                crosshairContainer.gameObject.SetActive(active);
        }

        private void ResetCrosshair()
        {
            spread = defaultSpread;
            color = defaultColor;
            originalWidth = width;

            crosshairEvents.OnCrosshairReset?.Invoke();
        }

        /// <summary>
        /// Resize the crosshair based on the current weapon ( if it exists )
        /// </summary>
        public void Resize()
        {
            if(weaponController == null || weaponController.Weapon == null) return;

            Resize(weaponController.Weapon.crosshairResize * 10);
        }

        /// <summary>
        /// Resize the crosshair to a new value.
        /// </summary>
        public void Resize(float newSize)
        {
            if (!resizeCrosshair) return;

            spread = Mathf.Lerp(spread, newSize, resizeSpeed * Time.deltaTime);
            crosshairEvents.OnCrosshairResized?.Invoke();
        }
        /// <summary>
        /// Change color of the crosshair on spotting an enemy
        /// </summary>
        public void SpotEnemy(bool condition)
        {
            color = (condition) ? enemySpottedColor : defaultColor;
            width = (condition) ? Mathf.Lerp(width, enemySpottedWidth, resizeSpeed) : Mathf.Lerp(width, originalWidth, resizeSpeed);

            if (condition)
                crosshairEvents.OnEnemySpotted?.Invoke();
        }

        public void ShowEnemySpotted()
        {
            color = enemySpottedColor;
            width = Mathf.Lerp(width, enemySpottedWidth, resizeSpeed);

            crosshairEvents.OnEnemySpotted?.Invoke();
        }

        public void HideEnemySpotted()
        {
            color = defaultColor;
            width = Mathf.Lerp(width, originalWidth, resizeSpeed);
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;

            crosshairEvents.OnVisibilityChanged?.Invoke();
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(Crosshair))] public class CrosshairEditor : HUDModuleEditorBase { } }
#endif
