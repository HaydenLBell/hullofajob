/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>using UnityEngine;
using UnityEngine;

namespace cowsins
{
    public class Bullet : MonoBehaviour, IBullet
    {
        [HideInInspector] public float Speed { get; set; }
        [HideInInspector] public float Damage { get; set; }
        [HideInInspector] public Vector3 Destination { get; set; }
        [HideInInspector] public bool Gravity { get; set; }
        [HideInInspector] public Transform Player { get; set; }
        [HideInInspector] public bool HurtsPlayer { get; set; }
        [HideInInspector] public bool ExplosionOnHit { get; set; }
        [HideInInspector] public GameObject ExplosionVFX { get; set; }
        [HideInInspector] public float ExplosionRadius { get; set; }
        [HideInInspector] public float ExplosionForce { get; set; }
        [HideInInspector] public float CriticalMultiplier { get; set; }
        [HideInInspector] public float Duration { get; set; }

        [SerializeField] private LayerMask projectileHitLayer;


        private bool projectileHasAlreadyHit = false; // Prevent from double hitting issues

        private void Start()
        {
            transform.LookAt(Destination);
            Invoke(nameof(DestroyProjectile), Duration);
        }

        private void Update()
        {
            transform.Translate(0.0f, 0.0f, Speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (projectileHasAlreadyHit || other.gameObject.layer == LayerMask.NameToLayer("Effects")) return;

            IDamageable damageable = other.GetComponent<IDamageable>();

            if (other.CompareTag(CowsinsUtilities.CRITICAL_TAG))
            {
                if (Player == null || other.transform.root != Player.root)
                    DamageTarget(CowsinsUtilities.GatherDamageableParent(other.transform), Damage * CriticalMultiplier, false);
            }
            else if (other.CompareTag(CowsinsUtilities.BODY_SHOT_TAG))
            {
                if (Player == null || other.transform.root != Player.root)
                    DamageTarget(CowsinsUtilities.GatherDamageableParent(other.transform), Damage, false);
            }
            else if (damageable != null && (Player == null || other.transform.root != Player.root))
            {
                DamageTarget(damageable, Damage, false);
            }
            else if (IsGroundOrObstacleLayer(other.gameObject.layer))
            {
                DestroyProjectile();
            }
        }

        private void DamageTarget(IDamageable target, float dmg, bool isCritical)
        {
            if (target != null)
            {
                DamageService.RequestDamage(target, dmg, isCritical);
                projectileHasAlreadyHit = true;
                DestroyProjectile();
            }
        }

        private bool IsGroundOrObstacleLayer(int layer)
        {
            return (projectileHitLayer.value & (1 << layer)) != 0;
        }

        private void DestroyProjectile()
        {
            if (ExplosionOnHit)
            {
                if (ExplosionVFX != null)
                {
                    var contact = GetComponent<Collider>().ClosestPoint(transform.position);
                    Instantiate(ExplosionVFX, contact, Quaternion.identity);
                }

                Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);

                // Deduplicate: one damage call per IDamageable (players have multiple hitbox colliders).
                var damagedTargets = new System.Collections.Generic.HashSet<IDamageable>();

                foreach (var collider in colliders)
                {
                    var damageable = collider.GetComponent<IDamageable>()
                        ?? CowsinsUtilities.GatherDamageableParent(collider.transform);
                    var playerMovement = collider.GetComponent<PlayerMovement>();
                    var rigidbody = collider.GetComponent<Rigidbody>();

                    if (damageable != null && damagedTargets.Add(damageable))
                    {
                        float distanceRatio = 1 - Mathf.Clamp01(Vector3.Distance(collider.transform.position, transform.position) / ExplosionRadius);
                        float dmg = Damage * distanceRatio;

                        bool isShooter = Player != null && collider.transform.root == Player.root;
                        if (isShooter && !HurtsPlayer)
                            continue;

                        DamageService.RequestDamage(damageable, dmg, false);
                    }

                    if (playerMovement != null)
                    {
                        CameraEffects cameraEffects = playerMovement.GetComponent<CameraEffects>();
                        cameraEffects.ExplosionShake(Vector3.Distance(cameraEffects.transform.position, transform.position));
                    }

                    if (rigidbody != null && collider != this)
                    {
                        rigidbody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, 5, ForceMode.Force);
                    }
                }
            }

            SpawnService.Despawn(gameObject);
        }
    }
}
