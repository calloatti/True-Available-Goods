using HarmonyLib;
using System.Reflection;
using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Stockpiles;
using UnityEngine;


namespace Calloatti.TrueAvailableGoods
{
  // =================================================================================
  // HISTORY OF ATTEMPTS / FAILED METHODS
  // =================================================================================

  //[HarmonyPatch("Timberborn.DistributionSystem.DistributionInventoryRegistry", "IsStockInventory")]
  //public static class DistributionRegistryPatches
  //{
  //  private static bool _firstInterceptionLogged = false;
  //  [HarmonyPostfix]
  //  public static void IsStockInventoryPostfix(Inventory inventory, string goodId, ref bool __result)
  //  {
  //    if (!_firstInterceptionLogged)
  //    {
  //      Debug.Log("[TrueAvailableGoods] First interception of IsStockInventory successful.");
  //      _firstInterceptionLogged = true;
  //    }
  //    if (!__result) return;
  //    __result = inventory.Takes(goodId);
  //  }
  //}

  //[HarmonyPatch(typeof(ResourceCount), "Create")]
  //public static class ResourceCountCreatePatch
  //{
  //  private static bool _firstInterceptionLogged = false;
  //  public static void Prefix(ref int inputOutputStock, ref int outputStock, ref int inputOutputCapacity, ref int outputCapacity, ref int carriedStock, ref int processedStock, ref int processedCapacity)
  //  {
  //    if (!_firstInterceptionLogged)
  //    {
  //      Debug.Log("[TrueAvailableGoods] First interception of ResourceCount.Create successful.");
  //      _firstInterceptionLogged = true;
  //    }
  //    processedStock = 0;
  //    carriedStock = 0;
  //  }
  //}

  //[HarmonyPatch(typeof(ResourceCount), nameof(ResourceCount.TotalStock), MethodType.Getter)]
  //public class ResourceCount_TotalStock_Patch
  //{
  //  public static void Postfix(ref ResourceCount __instance, ref int __result)
  //  {
  //    __result = __instance.BufferedStock + __instance.StockpiledStock;
  //  }
  //}

  /* // FAILED ATTEMPT: Pure Storage Filter (Ignored factory output stock entirely)
  [HarmonyPatch]
  public class StockCounter_CountInventoryStock_Patch_FAILED_WHITELIST
  {
      public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("Timberborn.ResourceCountingSystem.StockCounter"), "CountInventoryStock");
      public static bool Prefix(Inventory inventory)
      {
           bool IsStorageBuilding = inventory.GetComponent<Stockpile>() != null;
           return IsStorageBuilding;
      }
  }
  */

  // =================================================================================
  // CURRENT ACTIVE PATCHES
  // =================================================================================

  [HarmonyPatch]
  public class CapacityCounter_CountInventoryCapacity_Patch
  {
    // 1. Target the internal and private method of CapacityCounter
    public static MethodBase TargetMethod()
    {
      var type = AccessTools.TypeByName("Timberborn.ResourceCountingSystem.CapacityCounter");
      return AccessTools.Method(type, "CountInventoryCapacity");
    }

    // 2. Filter to only include dedicated storage capacity
    public static bool Prefix(Inventory inventory)
    {
      // Only count capacity for buildings with the Stockpile component (Warehouses, Tanks, Piles)
      // Factories have input capacity, but we don't want to see that in the GUI bar
      bool IsStorageBuilding = inventory.GetComponent<Stockpile>() != null;

      return IsStorageBuilding;
    }
  }

  [HarmonyPatch]
  public class StockCounter_CountInventoryStock_Patch
  {
    private static bool _firstInterceptionLogged = false;

    // 1. Target the internal and private method using AccessTools
    public static MethodBase TargetMethod()
    {
      var type = AccessTools.TypeByName("Timberborn.ResourceCountingSystem.StockCounter");
      return AccessTools.Method(type, "CountInventoryStock");
    }

    // 2. Intercept the inventory counting to handle Factories vs Stockpiles differently
    public static bool Prefix(object __instance, Inventory inventory)
    {
      // Log only the first time this code ever runs
      if (!_firstInterceptionLogged)
      {
        Debug.Log("[TrueAvailableGoods] First interception of ResourceCountingSystem.StockCounter successful.");
        _firstInterceptionLogged = true;
      }

      // A. If it's a dedicated storage building (Warehouse/Pile/Tank), 
      // return true to let the original method count everything.
      if (inventory.GetComponent<Stockpile>() != null)
      {
        return true;
      }

      // B. If it's a factory, we manually count ONLY the goods beavers can take (Output Stock)
      // We use reflection to invoke the private CountStock(GoodAmount, Inventory) method found in StockCounter
      MethodInfo countStockMethod = __instance.GetType().GetMethod("CountStock", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(GoodAmount), typeof(Inventory) }, null);

      foreach (GoodAmount item in inventory.Stock)
      {
        // inventory.Gives(goodId) returns true for finished products (e.g., Planks in a Lumber Mill)
        // but returns false for raw materials/ingredients (e.g., Logs in a Lumber Mill)
        if (inventory.Gives(item.GoodId))
        {
          // Manually trigger the internal counting for this specific item
          countStockMethod.Invoke(__instance, new object[] { item, inventory });
        }
      }

      // Return false to skip the original method which would have included raw material inputs
      return false;
    }
  }
}