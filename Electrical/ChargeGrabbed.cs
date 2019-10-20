using BS;
using Harmony;
using UnityEngine;

namespace Electrical
{
  public class ItemChargeSettings
  {
    internal static ItemChargeSettings Instance = new ItemChargeSettings();

    public bool autoCharge = true;
    public float manaPerSec = 5f;
  }

  public class RagdollChargeSettings
  {
    internal static RagdollChargeSettings Instance = new RagdollChargeSettings();

    public float manaPerSec = 12f;
    public float damagePerSec = 7f;
    public float launchManaCost = 10f;
  }

  [HarmonyPatch(typeof(SpellCaster))]
  [HarmonyPatch("Update")]
  internal static class ChargeGrabbedPatch
  {
    [HarmonyPostfix]
    private static void Postfix(SpellCaster __instance)
    {
      TryCharge(__instance.casterLeft);
      TryCharge(__instance.casterRight);
    }

    private static void TryCharge(SpellCasterHand hand)
    {
      if (hand && hand.currentSpell is SpellLightning)
      {
        if (hand.bodyHand && hand.bodyHand.interactor && hand.bodyHand.interactor.grabbedHandle)
        {
          bool tryCast = hand.caster.creature == Creature.player && PlayerControl.GetHand(hand.bodyHand.side).indexCurl > 0.5f;
          SpellLightning spell = hand.currentSpell as SpellLightning;

          if (hand.bodyHand.interactor.grabbedHandle.item)
          {
            if (tryCast || ItemChargeSettings.Instance.autoCharge || hand.caster.creature != Creature.player)
            {
              float manaCost = ItemChargeSettings.Instance.manaPerSec * Time.deltaTime;
              if (hand.caster.currentMana > manaCost)
              {
                hand.caster.currentMana -= manaCost;
                Item item = hand.bodyHand.interactor.grabbedHandle.item;
                ChargeItem(item);

                // If we're holding a bow, charge nocked arrows too
                var bowString = item.GetComponentInChildren<BowString>();
                if (bowString)
                {
                  if (bowString.nockedArrow)
                  {
                    ChargeItem(bowString.nockedArrow);
                  }
                }
              }
            }
          }
          if (hand.bodyHand.interactor.grabbedHandle is RagdollHandle && tryCast)
          {
            float manaCost = RagdollChargeSettings.Instance.manaPerSec * Time.deltaTime;
            if (hand.caster.currentMana > manaCost)
            {
              var ragdollHandle = hand.bodyHand.interactor.grabbedHandle as RagdollHandle;
              DamageStruct shockDamage = spell.damageStruct;
              shockDamage.damage = RagdollChargeSettings.Instance.damagePerSec * Time.deltaTime;
              shockDamage.hitRagdollPart = ragdollHandle.ragdollPart;
              CollisionStruct spellCollision = new CollisionStruct(shockDamage);
              ragdollHandle.ragdollPart.ragdoll.creature.health.Damage(ref spellCollision);
            }
          }
        }
      }
    }

    internal static void ChargeItem(Item item)
    {
      DamageStruct chargeDamage = new DamageStruct(Damager.DamageType.Shock, 1f);
      chargeDamage.effectId = "Shock";
      chargeDamage.effectRatio = 1f;
      // TODO?: Charging the first collider works for vanilla weapons, investigate with modded weapons?
      CollisionStruct collision = new CollisionStruct(chargeDamage, targetCollider: item.definition.colliderGroups[0].colliders[0]);
      item.OnDamageReceived(ref collision);
    }
  }

  [HarmonyPatch(typeof(RagdollHandle))]
  [HarmonyPatch("OnUnGrab")]
  internal static class UngrabPatch
  {
    [HarmonyPostfix]
    internal static void Postfix(RagdollHandle __instance, Interactor interactor)
    {
      var caster = interactor.bodyHand.caster;
      if (caster.currentSpell &&
          caster.currentSpell is SpellLightning &&
          PlayerControl.GetHand(interactor.bodyHand.side).indexCurl > 0.5f)
      {
        float manaCost = RagdollChargeSettings.Instance.launchManaCost;
        if (caster.caster.currentMana > manaCost)
        {
          caster.caster.currentMana -= manaCost;

          // Do a final burst of spell damage
          var spell = interactor.bodyHand.caster.currentSpell as SpellLightning;
          var ragdoll = __instance.ragdollPart.ragdoll;
          DamageStruct shockDamage = spell.damageStruct;
          shockDamage.damage = RagdollChargeSettings.Instance.damagePerSec;
          shockDamage.hitRagdollPart = __instance.ragdollPart;
          CollisionStruct spellCollision = new CollisionStruct(shockDamage);
          ragdoll.creature.health.Damage(ref spellCollision);

          // Aim the ragdoll launch
          Vector3 dir = (__instance.transform.position - interactor.bodyHand.body.creature.transform.position);
          dir.Set(dir.x, 0.0f, dir.z);
          dir.Normalize();
          dir.Set(dir.x, 0.7f, dir.z);

          // Apply the force to each part with a chance of a visual lightning bolt
          foreach (RagdollPart ragdollPart in __instance.ragdollPart.ragdoll.creature.ragdoll.parts)
          {
            Vector3 force = dir * 30f;
            ragdollPart.rb.AddForce(force, ForceMode.Impulse);
            if (Random.Range(0, 3) == 0)
            {
              LightningBolt.QueueLightningBolt(ragdollPart.gameObject, spell, Random.Range(0.1f, 0.5f));
            }
          }
        }
      }
    }
  }
}
