using HarmonyLib;
using Timberborn.DistributionSystem;
using Timberborn.InventorySystem;
using UnityEngine;
using Timberborn.ResourceCountingSystem;


namespace Calloatti.TrueAvailableGoods
{
  //[HarmonyPatch("Timberborn.DistributionSystem.DistributionInventoryRegistry", "IsStockInventory")]

  //public static class DistributionRegistryPatches
  //{

  //  private static bool _firstInterceptionLogged = false;

  //  [HarmonyPostfix]
  //  public static void IsStockInventoryPostfix(Inventory inventory, string goodId, ref bool __result)
  //  {

  //    // Log only the first time this code ever runs
  //    if (!_firstInterceptionLogged)
  //    {
  //      Debug.Log("[TrueAvailableGoods] First interception of IsStockInventory successful.");
  //      _firstInterceptionLogged = true;
  //    }

  //    // If the game already thinks it's NOT a stock inventory, we do nothing.
  //    if (!__result) return;

  //    // Timberborn considers a building 'Stock' if it has a 'Give' (Output) slot for a good.
  //    // Factories have 'Give' slots for products, but only 'Take' (Input) slots for raw materials.
  //    // Warehouses/Stockpiles have BOTH 'Take' and 'Give' slots for the same good.

  //    // By requiring inventory.Takes(goodId), we filter out raw materials locked in factories.
  //    __result = inventory.Takes(goodId);
  //  }
  //}


  [HarmonyPatch(typeof(ResourceCount), "Create")]
  public static class ResourceCountCreatePatch
  {
    private static bool _firstInterceptionLogged = false;

    // The method signature: int inputOutputStock, int outputStock, int inputOutputCapacity, int outputCapacity
    public static void Prefix(ref int outputStock)
    {
      // Log only the first time this code ever runs
      if (!_firstInterceptionLogged)
      {
        Debug.Log("[TrueAvailableGoods] First interception of ResourceCount.Create successful.");
        _firstInterceptionLogged = true;
      }

      // Force the reported outputStock to zero
      outputStock = 0;
    }
  }

}