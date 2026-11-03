using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Features.Spells
{
    public class SpellManager : MonoBehaviour
    {
        public static SpellManager Instance { get; private set; }

        [SerializeField] private SpellInstance projectilePrefab;
        [SerializeField] private SpellInstance areaOfEffectPrefab;

        private Dictionary<SpellForm, SpellInstance> spells;

        private void Awake()
        {
            Instance = this;

            spells = new Dictionary<SpellForm, SpellInstance>
            {
                [SpellForm.PROJECTILE] = projectilePrefab,
                [SpellForm.AREA_OF_EFFECT] = areaOfEffectPrefab
            };
        }

        public SpellInstance SpawnSpell(SpellDefinition spell, Transform spawnPoint)
        {
            SpellInstance spellInstance = Instantiate(spells[spell.SpellForm]);
            spellInstance.Init(spell, spawnPoint);
            return spellInstance;
        }
    }
}