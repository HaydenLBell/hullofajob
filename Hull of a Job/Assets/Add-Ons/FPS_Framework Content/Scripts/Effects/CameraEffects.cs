using UnityEngine;
using System.Collections;

namespace cowsins
{
    public class CameraEffects : MonoBehaviour
    {
        [SerializeField, Header("SHARED REFERENCES")] private Transform playerCamera;
        [SerializeField] private Transform camShakeTarget;

        [SerializeField, Header("TILT")] private float tiltSpeed;
        [SerializeField] private float tiltAmount;

        [SerializeField, Tooltip("Maximum Head Bob")] private float headBobAmplitude = 0.2f;
        [SerializeField, Tooltip("Speed to reach the Maximum Head Bob ( headBobAmplitude)")] private float headBobFrequency = 2f;
        [SerializeField, Range(0,1)] private float headBobCrouchMultiplier;

        [SerializeField] private bool useAdvancedHeadBob = true;
        [SerializeField] private Vector3 rotationMultiplier = new Vector3(1f, 0.5f, 0.5f);
        [SerializeField] private float translationSpeed = 10f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Vector3 movementLimit = new Vector3(0.05f, 0.05f, 0.05f);
        [SerializeField] private Vector3 bobLimit = new Vector3(0.05f, 0.1f, 0f);
        [SerializeField] private float horizontalInclineMultiplier = 0.8f;
        [SerializeField] private float forwardInclineMultiplier = 1f;

        private Vector3 landingOffset = Vector3.zero;

        private float bobSpeed;
        private Vector3 bobPos;
        private Vector3 bobRot;
        private Vector3 currentBobPos;
        private Quaternion currentBobRot = Quaternion.identity;

        [SerializeField, Tooltip("Maximum Breathing Amount"), Header("BREATHING EFFECT")] private float breathingAmplitude = 0.2f;
        [SerializeField, Tooltip("Breathing Speed")] private float breathingFrequency = 2f;
        [SerializeField, Tooltip("Enables Rotation for the Breathing Effect")] private bool applyBreathingRotation;




        [SerializeField, Header("LAND CAMERA SHAKE")] private float landShakeIntensity;
        [SerializeField] private float landShakeDuration;

        [SerializeField, Header("EXPLOSION CAMERA SHAKE")] private float explosionTraumaMultiplier = 10f;
        [SerializeField] private float explosionPower = 30f;
        [SerializeField] private float explosionMovementAmount = 1f;
        [SerializeField] private float explosionRotationAmount = 30f;

        // Camera Shake
        private float trauma;
        public float Trauma { get { return trauma; } set { trauma = Mathf.Clamp01(value); } }

        private float power = 16;
        private float movementAmount = 0.8f;
        private float rotationAmount = 17f;

        private float traumaDepthMag = 0.6f;
        private float traumaDecay = 1.3f;

        float timeCounter = 0;

        private Coroutine landingShakeRoutine;

        private IPlayerMovementStateProvider player; // IPlayerMovementStateProvider is implemented in PlayerMovement.cs
        private IPlayerControlProvider playerControlProvider; // IPlayerControlProvider is implemented in PlayerControl.cs
        private IWeaponEventsProvider weaponEvents; // IWeaponEventsProvider is implemented in WeaponController.cs
        private Rigidbody rb;
        private InputManager inputManager;

        private Vector3 origPos;
        private Quaternion origRot;

        public void Initialize(PlayerDependencies playerDependencies)
        {
            player = playerDependencies.PlayerMovementState;
            playerControlProvider = playerDependencies.PlayerControl;
            weaponEvents = playerDependencies.WeaponEvents;
            rb = GetComponent<Rigidbody>();
            this.inputManager = playerDependencies.InputManager;

            playerDependencies.PlayerMovementEvents.Events.OnLand.AddListener(LandingShake);
            weaponEvents.Events.OnShootShake.AddListener(ShootShake);
        }

        private void OnEnable()
        {
            if (playerCamera == null)
            {
                CowsinsUtilities.LogErrorFormat("No <b><color=cyan>PlayerCamera</color></b> reference found in CameraEffects. " +
                    "Please assign this reference accordingly to fix this error.");
                return;
            }

            if (camShakeTarget == null)
            {
                CowsinsUtilities.LogErrorFormat("No <b><color=cyan>CamShakeTarget</color></b> reference found in CameraEffects. " +
                    "Please assign this reference accordingly to fix this error.");
                return;
            }

            origPos = playerCamera.localPosition;
            origRot = playerCamera.localRotation;
        }
        private void Update()
        {
            if (playerControlProvider == null || !playerControlProvider.IsControllable || playerCamera == null || camShakeTarget == null) return;

            playerCamera.localPosition -= landingOffset;

            if (useAdvancedHeadBob)
            {
                playerCamera.localPosition -= currentBobPos;
                playerCamera.localRotation = playerCamera.localRotation * Quaternion.Inverse(currentBobRot);
            }

            UpdateTilt();

            UpdateHeadBob();
            UpdateBreathing();

            playerCamera.localPosition += landingOffset;

            HandleCamShake();
        }

        private void UpdateTilt()
        {
            if (player.CurrentSpeed == 0) return;

            Quaternion rot = CalculateTilt();
            playerCamera.localRotation = Quaternion.Lerp(playerCamera.localRotation, rot, Time.deltaTime * tiltSpeed);
        }

        private Quaternion CalculateTilt()
        {
            float x = inputManager.X;
            float y = inputManager.Y;

            Vector3 vector = new Vector3(y, 0, -x).normalized * tiltAmount;

            return Quaternion.Euler(vector);
        }

        private void UpdateHeadBob()
        {
            if (useAdvancedHeadBob)
            {
                AdvancedHeadBob();
            }
            else
            {
                SimpleHeadBob();
            }
        }

        private void SimpleHeadBob()
        {
            if (player.IsIdle || inputManager.Jumping)
            {
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, origPos, Time.deltaTime * 2f);
                playerCamera.localRotation = Quaternion.Lerp(playerCamera.localRotation, origRot, Time.deltaTime * 2f);
                return;
            }

            float angle = Time.timeSinceLevelLoad * headBobFrequency;
            float amplitude = player.IsCrouching ? headBobAmplitude * headBobCrouchMultiplier : headBobAmplitude;
            float distanceY = amplitude * Mathf.Sin(angle) / 400f;
            float distanceX = amplitude * Mathf.Cos(angle) / 100f;

            playerCamera.position = new Vector3(playerCamera.position.x, playerCamera.position.y + distanceY, playerCamera.position.z);
            playerCamera.Rotate(distanceX, 0, 0, Space.Self);
        }

        private void AdvancedHeadBob()
        {
            if (player.IsIdle || inputManager.Jumping)
            {
                currentBobPos = Vector3.Lerp(currentBobPos, Vector3.zero, Time.deltaTime * translationSpeed);
                currentBobRot = Quaternion.Slerp(currentBobRot, Quaternion.identity, Time.deltaTime * rotationSpeed);
                playerCamera.localPosition += currentBobPos;
                playerCamera.localRotation = playerCamera.localRotation * currentBobRot;
                return;
            }

            bobSpeed += Time.deltaTime * (player.CurrentSpeed / 2) + 0.01f;

            bobPos.x = (Mathf.Cos(bobSpeed) * bobLimit.x) - (inputManager.X * movementLimit.x);
            bobPos.y = Mathf.Sin(bobSpeed) * bobLimit.y;
            if (player.CurrentSpeed > 0) bobPos.y -= rb.linearVelocity.y * movementLimit.y;
            bobPos.z = -(inputManager.Y * movementLimit.z);

            bobRot.x = inputManager.X != 0 ? rotationMultiplier.x * Mathf.Sin(2f * bobSpeed) : rotationMultiplier.x * Mathf.Sin(2f * bobSpeed) / 2f;
            bobRot.y = inputManager.X != 0 ? rotationMultiplier.y * Mathf.Cos(bobSpeed) : 0f;
            bobRot.z = inputManager.X != 0 ? rotationMultiplier.z * Mathf.Cos(bobSpeed) * inputManager.X : 0f;

            bobRot.x += inputManager.Y * forwardInclineMultiplier;
            bobRot.z += inputManager.X * horizontalInclineMultiplier;

            currentBobPos = Vector3.Lerp(currentBobPos, bobPos, Time.deltaTime * translationSpeed);
            currentBobRot = Quaternion.Slerp(currentBobRot, Quaternion.Euler(bobRot), Time.deltaTime * rotationSpeed);

            playerCamera.localPosition += currentBobPos;
            playerCamera.localRotation = playerCamera.localRotation * currentBobRot;
        }

        private void UpdateBreathing()
        {
            float angle = Time.timeSinceLevelLoad * breathingFrequency;
            float distance = breathingAmplitude * Mathf.Sin(angle) / 400f;
            float distanceRot = breathingAmplitude * Mathf.Cos(angle) / 100f;

            playerCamera.position = new Vector3(playerCamera.position.x, playerCamera.position.y + distance, playerCamera.position.z);

            if (applyBreathingRotation)
            {
                playerCamera.Rotate(distanceRot, 0, 0, Space.Self);
            }
        }

        #region CameraShake
        private float GetFloat(float seed) { return (Mathf.PerlinNoise(seed, timeCounter) - 0.5f) * 2f; }

        private Vector3 GetVec3() { return new Vector3(GetFloat(1), GetFloat(10), GetFloat(100) * traumaDepthMag); }

        private void HandleCamShake()
        {
            if (Trauma > 0)
            {
                timeCounter += Time.deltaTime * Mathf.Pow(Trauma, 0.3f) * power;

                Vector3 newPos = GetVec3() * movementAmount * Trauma;
                camShakeTarget.localPosition = newPos;

                camShakeTarget.localRotation = Quaternion.Euler(newPos * rotationAmount);

                Trauma -= Time.deltaTime * traumaDecay * (Trauma + 0.3f);
            }
            else
            {
                //lerp back towards default position and rotation once shake is done
                Vector3 newPos = Vector3.Lerp(camShakeTarget.localPosition, Vector3.zero, Time.deltaTime);
                camShakeTarget.localPosition = newPos;
                camShakeTarget.localRotation = Quaternion.Euler(newPos * rotationAmount);
            }
        }

        public void Shake(float amount, float _power, float _movementAmount, float _rotationAmount)
        {
            Trauma = amount;
            power = _power;
            movementAmount = _movementAmount;
            rotationAmount = _rotationAmount;
        }

        public void ShootShake(float amount)
        {
            Trauma += amount;
            power = 20;
            movementAmount = .8f;
            rotationAmount = 17f;
        }

        public void ExplosionShake(float distance)
        {
            Trauma += explosionTraumaMultiplier / distance;
            power = explosionPower;
            movementAmount = explosionMovementAmount;
            rotationAmount = explosionRotationAmount;
        }

        /// <summary>
        /// Triggers a vertical shake to simulate landing impact.
        /// </summary>
        public void LandingShake()
        {
            if (landingShakeRoutine != null) StopCoroutine(landingShakeRoutine);
            landingShakeRoutine = StartCoroutine(LandingShakeRoutine(landShakeIntensity, landShakeDuration));
        }

        private IEnumerator LandingShakeRoutine(float intensity, float duration)
        {
            if (playerCamera == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = elapsed / duration;

                // Quick "down then up" bounce curve
                float curve = Mathf.Sin(normalized * Mathf.PI);
                // Exponential decay
                float decay = 1f - (normalized * normalized);
                float displacement = curve * decay * -intensity;

                // Apply displacement in world space
                Vector3 worldDisplacement = Vector3.up * displacement;
                Vector3 localDisplacement = playerCamera.parent.InverseTransformDirection(worldDisplacement);

                landingOffset = localDisplacement;

                yield return null;
            }

            // Ensure reset
            landingOffset = Vector3.zero;
        }

        #endregion
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CameraEffects))]
    public class CameraEffectsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("playerCamera"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("camShakeTarget"));

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("tiltSpeed"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("tiltAmount"));

            UnityEditor.EditorGUILayout.Space(10f);
            UnityEditor.SerializedProperty useAdvancedProp = serializedObject.FindProperty("useAdvancedHeadBob");

            if (useAdvancedProp.boolValue)
            {
                UnityEditor.EditorGUILayout.LabelField("HEAD BOB (ADVANCED)", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.PropertyField(useAdvancedProp);
                UnityEditor.EditorGUI.indentLevel++;
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationMultiplier"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("translationSpeed"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("movementLimit"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("bobLimit"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalInclineMultiplier"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("forwardInclineMultiplier"));
                UnityEditor.EditorGUI.indentLevel--;
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("HEAD BOB (BASIC)", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.PropertyField(useAdvancedProp);
                UnityEditor.EditorGUI.indentLevel++;
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("headBobAmplitude"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("headBobFrequency"));
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("headBobCrouchMultiplier"));
                UnityEditor.EditorGUI.indentLevel--;
            }

            UnityEditor.EditorGUILayout.Space(10f);
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingAmplitude"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingFrequency"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("applyBreathingRotation"));

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("landShakeIntensity"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("landShakeDuration"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
