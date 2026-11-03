using Assets.Features.Spells;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpellController : MonoBehaviour
{
    public Animator Animator;
    public SpellDefinition Spell;
    public Transform SpellSpawnPoint;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Cast(Spell);
            Animator.SetTrigger("Attack");
        }
    }

    private void Cast(SpellDefinition spell)
    {
        SpellManager.Instance.SpawnSpell(spell, SpellSpawnPoint);
    }
}
