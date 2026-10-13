using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.Animations;
#endif
using System.IO;

namespace cowsins
{
    public static class CowsinsUtilities
    {
        public const string CRITICAL_TAG = "Critical";
        public const string BODY_SHOT_TAG = "BodyShot";

        public static bool IsDamageableHitbox(Component c)
        {
            return c != null && (c.CompareTag(CRITICAL_TAG) || c.CompareTag(BODY_SHOT_TAG));
        }

        /// <summary>
        /// Returns a Vector3 that applies spread to the bullets shot
        /// </summary>
        public static Vector3 GetSpreadDirection(float amount, Transform origin)
        {
            float horSpread = UnityEngine.Random.Range(-amount, amount);
            float verSpread = UnityEngine.Random.Range(-amount, amount);
            Vector3 spread = origin.TransformDirection(new Vector3(horSpread, verSpread, 0));
            Vector3 dir = origin.forward + spread;

            return dir.normalized;
        }
        public static void PlayAnim(int animHash, Animator animator)
        {
            animator.SetTrigger(animHash);
        }

        public static void ForcePlayAnim(int animHash, Animator animator)
        {
            animator.Play(animHash, 0, 0);
        }
        public static void StartAnim(int animHash, Animator animated) => animated.SetBool(animHash, true);

        public static void StopAnim(int animHash, Animator animated) => animated.SetBool(animHash, false);
#if UNITY_EDITOR
        public static void SavePreset(UnityEngine.Object source, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError("ERROR: Do not forget to give your preset a name!");
                return;
            }
            Preset preset = new Preset(source);

            string directoryPath = "Assets/" + "Cowsins/" + "CowsinsPresets/";

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string fullPath = directoryPath + name + ".preset";
            AssetDatabase.CreateAsset(preset, fullPath);
            Debug.Log($"Preset successfully saved in {fullPath}");
        }
        public static void ApplyPreset(Preset preset, UnityEngine.Object target)
        {
            preset.ApplyTo(target);
        }

        public static bool IsUsingUnity6()
        {
            string unityVersion = Application.unityVersion;
            return unityVersion.StartsWith("6"); 
        }

#endif
        public static IDamageable GatherDamageableParent(Transform child)
        {
            // First check if this or any parent has a DamageForwarder
            var forwarder = child.GetComponentInParent<DamageForwarder>();
            if (forwarder != null && forwarder.targetStats != null)
            {
                return forwarder.targetStats;
            }

            // Otherwise, fallback to the original behavior but check the child itself first
            if (child.TryGetComponent(out IDamageable comp))
            {
                return comp;
            }

            for (Transform parent = child.parent; parent != null; parent = parent.parent)
            {
                if (parent.TryGetComponent(out IDamageable component))
                {
                    return component;
                }
            }
            return null;
        }

        public static bool InvokeFunc(Func<bool> del, bool defaultValue = true)
        {
            if (del == null)
                return defaultValue;

            foreach (Func<bool> func in del.GetInvocationList())
            {
                if (!func())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determine wether this is determined as floor or not
        /// </summary>
        public static bool IsFloor(Vector3 v, float maxSlopeAngle)
        {
            float currentFloorAngle = Vector3.Angle(Vector3.up, v);
            return currentFloorAngle < maxSlopeAngle;
        }

        /// <summary>
        /// Checks if the attachment is compatible with the given weapon identification
        /// </summary>
        /// <param name="weaponIdentification">Weapon identification to check compatibility against</param>
        /// <param name="identifier">Attachment identifier to check</param>
        /// <returns>A tuple containing whether it was found, the attachment itself, and its index</returns>
        public static (bool found, Attachment attachment, int index) CompatibleAttachment(WeaponIdentification weaponIdentification, AttachmentIdentifier_SO identifier)
        {
            if (weaponIdentification == null) return (false, null, -1);

            Weapon_SO weapon = weaponIdentification.weapon;

            if (weapon?.weaponObject == null || identifier == null)
                return (false, null, -1);

            var compatible = weaponIdentification.compatibleAttachments;

            foreach (AttachmentType type in System.Enum.GetValues(typeof(AttachmentType)))
            {
                IReadOnlyList<Attachment> attachments = compatible.GetCompatible(type);

                for (int i = 0; i < attachments.Count; i++)
                {
                    if (attachments[i]?.attachmentIdentifier == identifier)
                        return (true, attachments[i], i);
                }
            }

            return (false, null, -1);
        }


#if UNITY_EDITOR
        public static (bool, float) CheckClipAvailability(Animator animator, string stateName)
        {
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller == null) return (false, 0f);

            foreach (var layer in controller.layers)
            {
                foreach (var childState in layer.stateMachine.states)
                {
                    if (childState.state.name == stateName)
                    {
                        AnimationClip clip = childState.state.motion as AnimationClip;
                        if (clip != null)
                        {
                            return (true, clip.length);
                        }
                        return (false, 0f);
                    }
                }
            }

            return (false, 0f);
        }
#endif
        public static AudioClip[] GetSoundsForLayer(this PlayerMovementSettings.FootStepsSounds footsteps, int layer)
        {
            // Check dynamic entries first
            foreach (var entry in footsteps.surfaceSounds)
            {
                if (entry.cachedLayerIndex == -1)
                    entry.cachedLayerIndex = LayerMask.NameToLayer(entry.layerName);

                if (entry.cachedLayerIndex == layer && entry.sounds.Length > 0)
                    return entry.sounds;
            }

            // Fallback to default
            return footsteps.defaultStep;
        }

        public static GameObject GetImpactForLayer(this WeaponControllerSettings.ImpactEffects impactEffects, int layer)
        {
            foreach (var entry in impactEffects.impacts)
            {
                if (entry.cachedLayerIndex == -1)
                    entry.cachedLayerIndex = LayerMask.NameToLayer(entry.layerName);

                if (entry.cachedLayerIndex == layer && entry.impact != null)
                    return entry.impact;
            }

            return impactEffects.defaultImpact;
        }
        public static GameObject GetBulletHoleForLayer(this BulletHoleImpact bulletHoles, int layer)
        {
            foreach (var entry in bulletHoles.bulletHoleImpact)
            {
                if (entry.cachedLayerIndex == -1)
                    entry.cachedLayerIndex = LayerMask.NameToLayer(entry.layerName);

                if (entry.cachedLayerIndex == layer && entry.bulletHoleImpact != null)
                    return entry.bulletHoleImpact;
            }

            return bulletHoles.defaultImpact;
        }


        private const string logPrefix = "<color=red>[COWSINS]</color>";
        private static void Log(LogType type, string message, UnityEngine.Object context = null)
        {
            switch (type)
            {
                case LogType.Log:
                    if (context == null) Debug.Log(Format(message));
                    else Debug.Log(Format(message), context);
                    break;
                case LogType.Warning:
                    if (context == null) Debug.LogWarning(Format(message));
                    else Debug.LogWarning(Format(message), context);
                    break;

                case LogType.Error:
                    if (context == null) Debug.LogError(Format(message));
                    else Debug.LogError(Format(message), context);
                    break;

                case LogType.Exception:
                    if (context == null) Debug.LogError(Format(message));
                    else Debug.LogError(Format(message), context);
                    break;
            }
        }
        private static string Format(string message) => $"{logPrefix} {message}";

        public static void Log(string message, UnityEngine.Object context = null)
            => Log(LogType.Log, message, context);

        public static void LogWarning(string message, UnityEngine.Object context = null)
            => Log(LogType.Warning, message, context);

        public static void LogError(string message, UnityEngine.Object context = null)
            => Log(LogType.Error, message, context);

        public static void LogErrorFormat(string message, params object[] args)
            => Debug.LogErrorFormat(Format(message), args);

        public static void SetLinearVelocity(this Rigidbody rb, Vector3 velocity)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = velocity;
#else
            rb.velocity = velocity;
#endif
        }

        public static Vector3 GetLinearVelocity(this Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }
    }
}
