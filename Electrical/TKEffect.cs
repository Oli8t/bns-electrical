using BS;
using Harmony;
using UnityEngine;

namespace Electrical
{
  [HarmonyPatch(typeof(Telekinesis))]
  [HarmonyPatch("Update")]
  internal static class TKLightningPatch
  {
    [HarmonyPostfix]
    private static void Postfix(Telekinesis __instance)
    {
      var caster = __instance.interactor.bodyHand.caster;
      if (__instance.catchedHandle && caster.currentSpell && caster.currentSpell is SpellLightning)
      {
        float manaCost = ItemChargeSettings.Instance.manaPerSec * Time.deltaTime;
        if (caster.caster.currentMana > manaCost)
        {
          caster.caster.currentMana -= manaCost;
          var existingBolts = __instance.catchedHandle.GetComponents<LightningBolt>();
          if (existingBolts.Length < 4)
          {
            var toHand = __instance.catchedHandle.transform.position - caster.bodyHand.transform.position;
            if (toHand.sqrMagnitude < 3f)
            {
              LightningBolt.QueueLightningBolt(__instance.catchedHandle.gameObject, caster.currentSpell as SpellLightning, Random.Range(0.1f, 0.4f));
              if (__instance.catchedHandle.item)
              {
                ChargeGrabbedPatch.ChargeItem(__instance.catchedHandle.item);
              }
            }
          }
        }
      }
    }
  }
}
