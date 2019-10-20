using BS;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace Electrical
{
  public class Electrical : LevelModule
  {
    private static Electrical instance = null;
    private HarmonyInstance harmony = null;

    public static bool IsLoaded()
    {
      return instance != null;
    }

    public override void OnLevelLoaded(LevelDefinition levelDefinition)
    {
      base.OnLevelLoaded(levelDefinition);

      try
      {
        if (instance != null)
        {
          Debug.LogError("Tried to load more than one instance of Electrical");
        }
        else
        {
          harmony = HarmonyInstance.Create("Electrical");
          harmony.PatchAll(Assembly.GetExecutingAssembly());
          instance = this;
          Debug.Log("Electrical successfully loaded!");
        }
      }
      catch (System.Exception e)
      {
        Debug.LogException(e);
      }
    }
  }
}
