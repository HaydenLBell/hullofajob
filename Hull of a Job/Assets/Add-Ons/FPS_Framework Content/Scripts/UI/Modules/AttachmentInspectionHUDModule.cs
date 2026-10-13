using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace cowsins
{
    // Handles weapon attachment inspection UI: Displaying scopes, barrels...
    public class AttachmentInspectionHUDModule : MonoBehaviour, IHUDModule
    {
        [Title("Inspection UI")]
        [Tooltip("UI enabled when inspecting."), SerializeField] private CanvasGroup inspectionUI;
        [SerializeField] private float inspectionFadeDuration = 0.5f;
        [SerializeField, Tooltip("Text that displays the name of the current weapon when inspecting.")] private TextMeshProUGUI weaponDisplayText_AttachmentsUI;
        [SerializeField, Tooltip("Prefab of the UI element that represents an attachment on-screen when inspecting")] private GameObject attachmentDisplay_UIElement;
        
        [Title("Attachment Groups")]
        [SerializeField, Tooltip("Group of attachments. Attachment UI elements are wrapped inside these.")]
        private GameObject barrels_AttachmentsGroup;
        [SerializeField] private GameObject scopes_AttachmentsGroup;
        [SerializeField] private GameObject stocks_AttachmentsGroup;
        [SerializeField] private GameObject grips_AttachmentsGroup;
        [SerializeField] private GameObject magazines_AttachmentsGroup;
        [SerializeField] private GameObject flashlights_AttachmentsGroup;
        [SerializeField] private GameObject lasers_AttachmentsGroup;
        
        [Title("Colors")]
        [SerializeField, Tooltip("Color of an attachment UI element when it is equipped.")] private Color usingAttachmentColor;
        [SerializeField, Tooltip("Color of an attachment UI element when it is unequipped. This is the default color.")] private Color notUsingAttachmentColor;
        
        [Title("Attachment Layout Settings")]
        [SerializeField, Tooltip("Defines how attachment groups are displayed on screen when inspecting")] private AttachmentUILayoutMode attachmentLayoutMode = AttachmentUILayoutMode.WorldSpaceTracking;
        [SerializeField, Tooltip("Spacing between attachment groups")] private float attachmentGroupSpacing = 150f;
        [SerializeField, Tooltip("Starting position for attachment groups in the UI ( 0,0 = Top Left Corner )")] private Vector2 attachmentGroupStartPosition = Vector2.zero;
        [SerializeField, Tooltip("Spacing direction for Vertical layout. True = downwards, False = upwards")] private bool verticalSpacingDown = true;

        private IInteractEventsProvider interactEvents;
        private PlayerDependencies dependencies;
        private Coroutine inspectFadeRoutine;
        private UIManager uiManager;

        public void Initialize(PlayerDependencies dependencies)
        {
            this.dependencies = dependencies;
            this.interactEvents = dependencies.InteractEvents;
            this.uiManager = GetComponentInParent<UIManager>();

            if (inspectionUI) inspectionUI.alpha = 0;

            interactEvents.Events.OnStartRealtimeInspection.AddListener(StartRealtimeInspection);
            interactEvents.Events.OnStopInspect.AddListener(StopInspection);
            interactEvents.Events.OnInspectionUIRefreshRequested.AddListener(GenerateInspectionUI);
        }

        private void OnDestroy()
        {
            if (interactEvents == null) return;
            interactEvents.Events.OnStartRealtimeInspection.RemoveListener(StartRealtimeInspection);
            interactEvents.Events.OnStopInspect.RemoveListener(StopInspection);
            interactEvents.Events.OnInspectionUIRefreshRequested.RemoveListener(GenerateInspectionUI);
        }

        private void StartRealtimeInspection(bool displayCurrentAttachmentsOnly)
        {
            uiManager?.UnlockMouse();
            GenerateInspectionUI(displayCurrentAttachmentsOnly);
            StartFadeCoroutine(1f);
        }

        private void StopInspection()
        {
            uiManager?.LockMouse();
            StartFadeCoroutine(0f);
        }

        private void StartFadeCoroutine(float targetAlpha)
        {
            if (inspectFadeRoutine != null)
                StopCoroutine(inspectFadeRoutine);

            gameObject.SetActive(true);
            inspectFadeRoutine = StartCoroutine(FadeInspectionUI(targetAlpha));
        }

        private IEnumerator FadeInspectionUI(float targetAlpha)
        {
            if (inspectionUI == null) yield break;

            inspectionUI.gameObject.SetActive(true);

            float startAlpha = inspectionUI.alpha;
            float elapsedTime = 0f;

            while (elapsedTime < inspectionFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                inspectionUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / inspectionFadeDuration);
                yield return null;
            }

            inspectionUI.alpha = targetAlpha;

            if (Mathf.Approximately(targetAlpha, 0f))
                inspectionUI.gameObject.SetActive(false);
        }

        private void GenerateInspectionUI(bool displayCurrentAttachments)
        {
            IWeaponReferenceProvider wRef = dependencies.WeaponReference;
            WeaponIdentification weapon = wRef.Id;
            if(weaponDisplayText_AttachmentsUI != null) weaponDisplayText_AttachmentsUI.text = wRef.Weapon._name;

            CleanAttachmentGroup(barrels_AttachmentsGroup);
            CleanAttachmentGroup(scopes_AttachmentsGroup);
            CleanAttachmentGroup(stocks_AttachmentsGroup);
            CleanAttachmentGroup(grips_AttachmentsGroup);
            CleanAttachmentGroup(magazines_AttachmentsGroup);
            CleanAttachmentGroup(flashlights_AttachmentsGroup);
            CleanAttachmentGroup(lasers_AttachmentsGroup);

            WeaponIdentification wID = wRef.Id;
            AttachmentStateManager state = wID.AttachmentState;
            CompatibleAttachments compAttachments = weapon.compatibleAttachments;

            int visibleGroupIndex = 0;
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Barrel), barrels_AttachmentsGroup, state.GetCurrent(AttachmentType.Barrel), state.GetDefault(AttachmentType.Barrel), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Scope), scopes_AttachmentsGroup, state.GetCurrent(AttachmentType.Scope), state.GetDefault(AttachmentType.Scope), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Stock), stocks_AttachmentsGroup, state.GetCurrent(AttachmentType.Stock), state.GetDefault(AttachmentType.Stock), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Grip), grips_AttachmentsGroup, state.GetCurrent(AttachmentType.Grip), state.GetDefault(AttachmentType.Grip), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Magazine), magazines_AttachmentsGroup, state.GetCurrent(AttachmentType.Magazine), state.GetDefault(AttachmentType.Magazine), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Flashlight), flashlights_AttachmentsGroup, state.GetCurrent(AttachmentType.Flashlight), state.GetDefault(AttachmentType.Flashlight), visibleGroupIndex);
            visibleGroupIndex = GenerateAttachmentGroup(displayCurrentAttachments, compAttachments.GetCompatible(AttachmentType.Laser), lasers_AttachmentsGroup, state.GetCurrent(AttachmentType.Laser), state.GetDefault(AttachmentType.Laser), visibleGroupIndex);
        }

        private int GenerateAttachmentGroup(bool displayCurrentAttachments, IReadOnlyList<Attachment> attachments, GameObject attachmentsGroup, Attachment atc, Attachment defaultAttachment, int groupIndex)
        {
            if (attachmentDisplay_UIElement == null) return groupIndex;

            if (attachments.Count == 0 || displayCurrentAttachments && atc == null)
            {
                attachmentsGroup.SetActive(false);
                return groupIndex;
            }
            AttachmentGroupUI atcG = attachmentsGroup.GetComponent<AttachmentGroupUI>();
            if (atc != null)
                atcG.target = atc.transform;
            else if (attachments[0] != null)
                atcG.target = attachments[0].transform;
            
            // Configure layout strategy
            atcG.groupIndex = groupIndex;
            atcG.SetLayoutStrategy(attachmentLayoutMode, attachmentGroupSpacing, attachmentGroupStartPosition, verticalSpacingDown);

            attachmentsGroup.SetActive(true);
            for (int i = 0; i < attachments.Count; i++)
            {
                if (attachments[i] == defaultAttachment || displayCurrentAttachments && attachments[i] != atc) continue; // Do not add default attachments to the UI 
                GameObject display = Instantiate(attachmentDisplay_UIElement, attachmentsGroup.transform);
                AttachmentUIElement disp = display.GetComponent<AttachmentUIElement>();

                if (attachments[i].attachmentIdentifier?.icon != null)
                    disp.SetIcon(attachments[i].attachmentIdentifier.icon);
                disp.assignedColor = usingAttachmentColor;
                disp.unAssignedColor = notUsingAttachmentColor;
                disp.DeselectAll(atc);
                if (attachments[i] == atc)
                    disp.SelectAsAssigned();
                disp.atc = attachments[i];
                disp.id = i;
                display.SetActive(false);
            }
            
            return groupIndex + 1;
        }

        private void CleanAttachmentGroup(GameObject attachmentsGroup)
        {
            if (attachmentsGroup == null) return;

            for (int i = 1; i < attachmentsGroup.transform.childCount; i++)
            {
                Destroy(attachmentsGroup.transform.GetChild(i).gameObject);
            }
        }
    }
}

#if UNITY_EDITOR
namespace cowsins { [UnityEditor.CustomEditor(typeof(AttachmentInspectionHUDModule))] public class AttachmentInspectionHUDModuleEditor : HUDModuleEditorBase { } }
#endif

