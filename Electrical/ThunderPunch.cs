using BS;
using Harmony;
using UnityEngine;

namespace Electrical
{
  [HarmonyPatch(typeof(Creature))]
  [HarmonyPatch("OnDamage")]
  internal static class ThunderPunchPatch
  {
    [HarmonyPrefix]
    private static bool Prefix(Creature __instance, ref CollisionStruct collisionStruct)
    {
      var damageStruct = collisionStruct.damageStruct;
      if (damageStruct.damager && damageStruct.damager.data.id == "Punch")
      {
        var punch = damageStruct.damager.item;
        var spell = punch.mainHandler.bodyHand.caster.currentSpell;
        if (spell is SpellLightning)
        {
          // Convert damage to shock, add a bit of bonus damage
          damageStruct.damageType = Damager.DamageType.Shock;
          damageStruct.damage += 5f;
          damageStruct.knockOutDuration = 2f;

          // Apply bonus knockback
          var direction = punch.rb.velocity.normalized;
          direction *= 20f;
          foreach (RagdollPart ragdollPart in __instance.ragdoll.parts)
          {
            ragdollPart.rb.AddForce(direction, ForceMode.Impulse);
          }

          // Trigger some lightning bolts
          var lightningSpell = spell as SpellLightning;
          lightningSpell.PlayClipAt(lightningSpell.startLowClip, spell.transform.position);
          int bolts = Random.Range(5, 8);
          for (int i = 0; i < bolts; ++i)
          {
            float delay = Random.Range(0.1f, 0.5f);
            LightningBolt.QueueLightningBolt(damageStruct.hitRagdollPart.gameObject, lightningSpell, delay);
          }
        }
      }
      return true;
    }
  }
}
