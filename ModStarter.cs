using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace Calloatti.TrueAvailableGoods
{
  public class TrueAvailableGoodsStartup : IModStarter
  {
    public void StartMod(IModEnvironment environment)
    {
      // Use the specific ModID we agreed upon
      new Harmony("calloatti.trueavailablegoods").PatchAll();

      Debug.Log("[TrueAvailableGoods] Initialization complete. Top-bar will now hide unusable factory inputs.");
    }
  }
}