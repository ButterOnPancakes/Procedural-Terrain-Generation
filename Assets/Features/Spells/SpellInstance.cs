using System.Collections;
using UnityEngine;

namespace Assets.Features.Spells
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class SpellInstance : MonoBehaviour
    {
        public static int InstanceCount { get; private set; } = 0;

        public SpellDefinition SpellDefinition { get; private set; }
        public Vector3 Direction { get; private set; }
        public bool IsDestroyRequested { get; private set; }

        private void Awake() => InstanceCount++;
        private void OnDestroy() => InstanceCount--;

        public void Init(SpellDefinition spellDefinition, Transform spawnPoint)
        {
            IsDestroyRequested = false;
            transform.position = spawnPoint.position;
            transform.forward = spawnPoint.forward;
            Direction = spawnPoint.forward;

            SpellDefinition = spellDefinition;

            SphereCollider collider = GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = SpellDefinition.Radius;

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = SpellDefinition.Speed * Direction;
        }

        private void Start()
        {
            SpellTriggerManager.TriggerOnSpawn(this);
            if (!IsDestroyRequested)
                SpellTriggerManager.StartTimedTriggers(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsDestroyRequested || SpellDefinition.SpellForm == SpellForm.AREA_OF_EFFECT)
                return;

            SpellTriggerManager.TriggerCollision(this, other);
        }

        public void ApplyTriggerEffects(SpellTrigger trigger, Collider target = null)
        {
            if (trigger?.effects == null)
                return;

            foreach (var effect in trigger.effects)
            {
                SpellEffectManager.Apply(this, target, effect);
                if (IsDestroyRequested)
                    return;
            }
        }

        public void DestroySpell()
        {
            if (IsDestroyRequested)
                return;

            IsDestroyRequested = true;
            Destroy(gameObject);
        }
    }
}
