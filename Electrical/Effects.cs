using BS;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;

namespace Electrical
{
  internal class LightningBolt : MonoBehaviour
  {
    private LightningBoltParameters lightningParameters = new LightningBoltParameters();
    private SpellLightning sourceSpell = null;
    private float delay = 0f;

    internal static void QueueLightningBolt(GameObject target, SpellLightning lightning, float delay)
    {
      var bolt = target.AddComponent<LightningBolt>();
      if (bolt)
      {
        bolt.InitializeParameters(lightning, delay);
      }
    }

    private void Update()
    {
      delay -= Time.deltaTime;
      if (delay <= 0f)
      {
        Trigger();
      }
    }

    private void Trigger()
    {
      if (sourceSpell)
      {
        lightningParameters.Start = sourceSpell.transform.position;
        lightningParameters.End = gameObject.transform.position;
        sourceSpell.lightningBoltScript.CreateLightningBolt(lightningParameters);
      }
      GameObject.Destroy(this);
    }

    private void InitializeParameters(SpellLightning lightning, float delay)
    {
      sourceSpell = lightning;
      this.delay = delay;
      float lifeTime = lightning.DurationRange.Random(lightningParameters.Random);
      lightningParameters.EndWidthMultiplier = lightning.EndWidthMultiplier;
      lightningParameters.Generations = lightning.Generations;
      lightningParameters.LifeTime = lifeTime;
      lightningParameters.ChaosFactor = lightning.ChaosFactor;
      lightningParameters.ChaosFactorForks = lightning.ChaosFactorForks;
      lightningParameters.TrunkWidth = lightning.trunkWidthMaxRange.y;
      lightningParameters.Intensity = lightning.Intensity;
      lightningParameters.GlowIntensity = lightning.GlowIntensity;
      lightningParameters.GlowWidthMultiplier = lightning.GlowWidthMultiplier;
      lightningParameters.Forkedness = lightning.Forkedness;
      lightningParameters.ForkLengthMultiplier = lightning.ForkLengthMultiplier;
      lightningParameters.ForkLengthVariance = lightning.ForkLengthVariance;
      lightningParameters.FadePercent = lightning.FadePercent;
      lightningParameters.FadeInMultiplier = lightning.FadeInMultiplier;
      lightningParameters.FadeOutMultiplier = lightning.FadeOutMultiplier;
      lightningParameters.FadeFullyLitMultiplier = lightning.FadeFullyLitMultiplier;
      lightningParameters.GrowthMultiplier = lightning.GrowthMultiplier;
      lightningParameters.ForkEndWidthMultiplier = lightning.ForkEndWidthMultiplier;
      lightningParameters.DelayRange = lightning.DelayRange;
      lightningParameters.LightParameters = lightning.LightParameters;
      lightningParameters.LightParameters.LightIntensity = lightning.lightIntensityRange.y;
      lightningParameters.LightParameters.LightRange = lightning.lightRadiusRange.y;
      lightningParameters.StartVariance = lightning.StartVariance;
      lightningParameters.EndVariance = lightning.EndVariance;
    }
  }
}
