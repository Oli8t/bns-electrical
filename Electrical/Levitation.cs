using BS;
using Harmony;
using UnityEngine;

namespace Electrical
{
  public class LevitationSettings
  {
    public float thrust = 5f;
  }

  [HarmonyPatch(typeof(SpellCaster))]
  [HarmonyPatch("Update")]
  internal static class LevitationPatch
  {
    [HarmonyPostfix]
    private static void Postfix(SpellCaster __instance)
    {
      if (__instance.creature != Creature.player)
      {
        return;
      }

      TryLevitate(__instance.casterLeft);
      TryLevitate(__instance.casterRight);
    }

    private static void TryLevitate(SpellCasterHand casterHand)
    {
      var hand = casterHand.bodyHand;
      if (casterHand.force > 0.1f &&
          !Player.local.locomotion.isGrounded &&
          hand.interactor.grabbedHandle == null)
      {
        Vector3 direction = (hand.bone.parent.position - hand.bone.position).normalized;
        var thrustMod = Electrical.LevitationSettings.thrust * Time.deltaTime * casterHand.force;
        if (casterHand.caster.currentMana < 5f)
        {
          thrustMod *= 0.5f;
        }
        Player.local.locomotion.rb.AddForceAtPosition(direction * thrustMod, hand.bone.transform.position, ForceMode.VelocityChange);
      }
    }
  }
}
