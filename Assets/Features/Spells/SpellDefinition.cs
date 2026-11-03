using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Features.Spells
{
    public enum SpellForm
    {
        PROJECTILE,
        AREA_OF_EFFECT
    }

    public enum TriggerType
    {
        ON_SPAWN = 0,
        ON_COLLIDE = 1,
        ON_EXPIRE = 3,
        ON_INTERVAL = 4
    }

    public enum EffectType
    {
        SPAWN_CHILD_SPELL = 0,
        RADIAL_IMPULSE = 1,
        SELF_DESTROY = 2
    }

    [CreateAssetMenu(fileName = "SpellDefinition", menuName = "Scriptable Objects/SpellDefinition")]
    public class SpellDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private SpellForm spellForm;
        [SerializeField] private float speed;
        [SerializeField] private float radius;
        [SerializeField] private List<SpellTrigger> triggers = new();

        public string Id => id;
        public SpellForm SpellForm => spellForm;
        public float Speed => speed;
        public float Radius => radius;
        public IReadOnlyList<SpellTrigger> Triggers => triggers;

        private void OnValidate()
        {
            if (triggers == null)
                return;

            foreach (var trigger in triggers)
            {
                SpellTriggerManager.EnsureArguments(trigger);
                if (trigger?.effects == null) continue;

                foreach (var effect in trigger.effects)
                    SpellEffectManager.EnsureArguments(effect);
            }
        }
    }

    [Serializable]
    public class SpellTrigger
    {
        public TriggerType triggerOn;
        [SerializeReference] public SpellTriggerArguments arguments;
        public List<SpellEffect> effects = new();
    }

    [Serializable]
    public abstract class SpellTriggerArguments { }

    [Serializable]
    public class SpellEffect
    {
        public EffectType effectType;
        [SerializeReference] public SpellEffectArguments arguments;
    }

    [Serializable]
    public abstract class SpellEffectArguments { }

}
