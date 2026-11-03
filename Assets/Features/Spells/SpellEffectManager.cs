using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Features.Spells
{
    public interface ISpellEffect
    {
        void Apply(SpellInstance spellInstance, Collider target, SpellEffect effect);
    }

    public static class SpellEffectManager
    {

        private sealed class EffectRegister
        {
            public readonly ISpellEffect Effect;
            public readonly Type ArgumentsType;
            public readonly Func<SpellEffectArguments> CreateArgumentsObject;

            public EffectRegister(
                ISpellEffect effect, 
                Type argumentsType, 
                Func<SpellEffectArguments> createArgumentsObject)
            {
                Effect = effect;
                ArgumentsType = argumentsType;
                CreateArgumentsObject = createArgumentsObject;
            }
        }

        private static readonly Dictionary<EffectType, EffectRegister> Registry = new()
        {
            [EffectType.RADIAL_IMPULSE] = new EffectRegister(
                new RadialImpulseEffect(),
                typeof(RadialImpulseArguments),
                () => new RadialImpulseArguments()),

            [EffectType.SPAWN_CHILD_SPELL] = new EffectRegister(
                new SpawnChildSpellEffect(),
                typeof(SpawnChildSpellArguments),
                () => new SpawnChildSpellArguments()),
            [EffectType.SELF_DESTROY] = new EffectRegister(
                new SelfDestroyEffect(),
                typeof(SelfDestroyArguments),
                () => new SelfDestroyArguments())
        };

        public static void EnsureArguments(SpellEffect effect)
        {
            if (effect == null ||
                !Registry.TryGetValue(effect.effectType, out var registration))
                return;

            if (!registration.ArgumentsType.IsInstanceOfType(effect.arguments))
                effect.arguments = registration.CreateArgumentsObject();
        }

        public static void Apply(
            SpellInstance spellInstance,
            Collider target,
            SpellEffect effect)
        {
            if (effect == null ||
                !Registry.TryGetValue(effect.effectType, out var registration))
            {
                Debug.LogWarning($"No spell effect registered for {effect?.effectType}.");
                return;
            }

            EnsureArguments(effect);
            registration.Effect.Apply(spellInstance, target, effect);
        }
    }

    [Serializable]
    public class RadialImpulseArguments : SpellEffectArguments
    {
        public float force = 400f;
        public float radius = 4f;
        public float upwardsModifier = 0.5f;
        [Tooltip("Only bodies on these layers are affected by the impulse.")]
        // Layer index 3 is Player in this project. Keep this a constant expression:
        // Unity may construct managed-reference arguments while deserializing assets,
        // where LayerMask.GetMask is not allowed.
        public LayerMask affectedLayers = ~(1 << 3);
    }
    public class RadialImpulseEffect : ISpellEffect
    {
        public void Apply(SpellInstance spellInstance, Collider target, SpellEffect effect)
        {
            if(effect.arguments is not RadialImpulseArguments impulseArgs)
            {
                Debug.LogError($"Invalid arguments for {nameof(RadialImpulseEffect)}");
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(
                spellInstance.transform.position,
                impulseArgs.radius,
                impulseArgs.affectedLayers
            );
            HashSet<Rigidbody> affectedBodies = new();

            foreach (Collider collider in colliders)
            {
                Rigidbody rb = collider.attachedRigidbody;
                if (rb != null && affectedBodies.Add(rb))
                {
                    rb.AddExplosionForce(
                        impulseArgs.force,
                        spellInstance.transform.position,
                        impulseArgs.radius,
                        impulseArgs.upwardsModifier,
                        ForceMode.Impulse
                    );
                }
            }
        }
    }

    [Serializable]
    public class SpawnChildSpellArguments : SpellEffectArguments
    {
        public SpellDefinition childSpell;
    }
    public class SpawnChildSpellEffect : ISpellEffect
    {
        public void Apply(SpellInstance spellInstance, Collider target, SpellEffect effect)
        {
            if (effect.arguments is not SpawnChildSpellArguments args || args.childSpell == null)
            {
                Debug.LogError($"Invalid arguments for {nameof(SpawnChildSpellEffect)}");
                return;
            }

            SpellManager.Instance.SpawnSpell(args.childSpell, spellInstance.transform);
        }
    }

    [Serializable]
    public class SelfDestroyArguments : SpellEffectArguments { }

    public class SelfDestroyEffect : ISpellEffect
    {
        public void Apply(SpellInstance spellInstance, Collider target, SpellEffect effect)
        {
            spellInstance.DestroySpell();
        }
    }
}
