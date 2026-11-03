using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Features.Spells
{
    public static class SpellTriggerManager
    {
        private interface ITriggerHandler
        {
            bool HasTimedRoutine { get; }
            void OnSpawn(SpellInstance spellInstance, SpellTrigger trigger);
            void OnCollision(SpellInstance spellInstance, SpellTrigger trigger, Collider other);
            IEnumerator RunTimed(SpellInstance spellInstance, SpellTrigger trigger);
        }

        private abstract class TriggerHandler : ITriggerHandler
        {
            public virtual bool HasTimedRoutine => false;
            public virtual void OnSpawn(SpellInstance spellInstance, SpellTrigger trigger) { }
            public virtual void OnCollision(SpellInstance spellInstance, SpellTrigger trigger, Collider other) { }
            public virtual IEnumerator RunTimed(SpellInstance spellInstance, SpellTrigger trigger)
            {
                yield break;
            }
        }

        private sealed class TriggerRegistration
        {
            public readonly Type ArgumentsType;
            public readonly Func<SpellTriggerArguments> CreateArguments;
            public readonly ITriggerHandler Handler;

            public TriggerRegistration(
                Type argumentsType,
                Func<SpellTriggerArguments> createArguments,
                ITriggerHandler handler)
            {
                ArgumentsType = argumentsType;
                CreateArguments = createArguments;
                Handler = handler;
            }
        }

        // Add each trigger type, its argument class, and its handler here.
        private static readonly Dictionary<TriggerType, TriggerRegistration> Registry = new()
        {
            [TriggerType.ON_SPAWN] = new TriggerRegistration(
                typeof(OnSpawnTriggerArguments),
                () => new OnSpawnTriggerArguments(),
                new OnSpawnHandler()),

            [TriggerType.ON_COLLIDE] = new TriggerRegistration(
                typeof(OnCollideTriggerArguments),
                () => new OnCollideTriggerArguments(),
                new OnCollideHandler()),

            [TriggerType.ON_EXPIRE] = new TriggerRegistration(
                typeof(OnExpireTriggerArguments),
                () => new OnExpireTriggerArguments(),
                new OnExpireHandler()),

            [TriggerType.ON_INTERVAL] = new TriggerRegistration(
                typeof(OnIntervalTriggerArguments),
                () => new OnIntervalTriggerArguments(),
                new OnIntervalHandler())
        };

        public static void EnsureArguments(SpellTrigger trigger)
        {
            if (trigger == null)
                return;

            // Migrate assets that still store the former ON_IMPACT enum value.
            if ((int)trigger.triggerOn == 2)
                trigger.triggerOn = TriggerType.ON_COLLIDE;

            if (!Registry.TryGetValue(trigger.triggerOn, out var registration))
                return;

            if (!registration.ArgumentsType.IsInstanceOfType(trigger.arguments))
                trigger.arguments = registration.CreateArguments();
        }

        public static void TriggerOnSpawn(SpellInstance spellInstance)
        {
            foreach (var trigger in spellInstance.SpellDefinition.Triggers)
            {
                if (trigger == null)
                    continue;

                EnsureArguments(trigger);
                if (trigger.triggerOn != TriggerType.ON_SPAWN)
                    continue;

                Registry[trigger.triggerOn].Handler.OnSpawn(spellInstance, trigger);
                if (spellInstance.IsDestroyRequested)
                    return;
            }
        }

        public static void TriggerCollision(SpellInstance spellInstance, Collider other)
        {
            foreach (var trigger in spellInstance.SpellDefinition.Triggers)
            {
                if (trigger == null)
                    continue;

                EnsureArguments(trigger);
                if (trigger.triggerOn != TriggerType.ON_COLLIDE)
                    continue;

                Registry[trigger.triggerOn].Handler.OnCollision(spellInstance, trigger, other);
                if (spellInstance.IsDestroyRequested)
                    return;
            }
        }

        public static void StartTimedTriggers(SpellInstance spellInstance)
        {
            foreach (var trigger in spellInstance.SpellDefinition.Triggers)
            {
                if (trigger == null)
                    continue;

                EnsureArguments(trigger);
                if (!Registry.TryGetValue(trigger.triggerOn, out var registration))
                    continue;

                var handler = registration.Handler;
                if (handler.HasTimedRoutine)
                    spellInstance.StartCoroutine(handler.RunTimed(spellInstance, trigger));
            }
        }

        private static bool IsLayerIncluded(LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        private sealed class OnSpawnHandler : TriggerHandler
        {
            public override void OnSpawn(SpellInstance spellInstance, SpellTrigger trigger)
            {
                spellInstance.ApplyTriggerEffects(trigger);
            }
        }

        private sealed class OnCollideHandler : TriggerHandler
        {
            public override void OnCollision(SpellInstance spellInstance, SpellTrigger trigger, Collider other)
            {
                if (other == null || trigger.arguments is not OnCollideTriggerArguments args)
                    return;

                if (IsLayerIncluded(args.collisionLayers, other.gameObject.layer))
                    spellInstance.ApplyTriggerEffects(trigger, other);
            }
        }

        private sealed class OnExpireHandler : TriggerHandler
        {
            public override bool HasTimedRoutine => true;

            public override IEnumerator RunTimed(SpellInstance spellInstance, SpellTrigger trigger)
            {
                var args = (OnExpireTriggerArguments)trigger.arguments;
                if (args.duration > 0f)
                    yield return new WaitForSeconds(args.duration);

                if (spellInstance != null && !spellInstance.IsDestroyRequested)
                    spellInstance.ApplyTriggerEffects(trigger);
            }
        }

        private sealed class OnIntervalHandler : TriggerHandler
        {
            public override bool HasTimedRoutine => true;

            public override IEnumerator RunTimed(SpellInstance spellInstance, SpellTrigger trigger)
            {
                var args = (OnIntervalTriggerArguments)trigger.arguments;
                float interval = Mathf.Max(args.intervalSeconds, 0.01f);

                spellInstance.ApplyTriggerEffects(trigger);
                while (spellInstance != null && !spellInstance.IsDestroyRequested)
                {
                    yield return new WaitForSeconds(interval);

                    if (spellInstance == null || spellInstance.IsDestroyRequested)
                        yield break;

                    spellInstance.ApplyTriggerEffects(trigger);
                }
            }
        }
    }

    [Serializable]
    public class OnSpawnTriggerArguments : SpellTriggerArguments { }

    [Serializable]
    public class OnCollideTriggerArguments : SpellTriggerArguments
    {
        public LayerMask collisionLayers;
    }

    [Serializable]
    public class OnExpireTriggerArguments : SpellTriggerArguments
    {
        [Min(0f)]
        public float duration = 3f;
    }

    [Serializable]
    public class OnIntervalTriggerArguments : SpellTriggerArguments
    {
        [Min(0.01f)]
        public float intervalSeconds = 1f;
    }
}
