using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static ErrorMessage;

namespace Fish_Out_Of_Water
{
    class Fish_Out_Of_Water
    {
        private const float seconds = 60f;
        public static Dictionary<LiveMixin, float> fishOutOfWater = new Dictionary<LiveMixin, float>();
        public static Dictionary<LiveMixin, float> fishInInventory = new Dictionary<LiveMixin, float>();

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            return creature && eatable && liveMixin;
        }

        public static void OnPlayerIsUnderwaterForSwimmingChanged(Utils.MonitoredValue<bool> isUnderwaterForSwimming)
        {
            //AddDebug(" OnPlayerIsUnderwaterForSwimmingChanged " + Player.main.IsUnderwaterForSwimming());
            //AddFishToList();
        }

        private static void CheckFishInContainer(ItemsContainer container)
        {
            if (container == null)
                return;

            foreach (InventoryItem item in container)
            {
                //AddDebug("CheckFishInInventory "+ item.item.gameObject.name);
                GameObject go = item.item.gameObject;
                if (IsEatableFish(go))
                {
                    LiveMixin liveMixin = go.GetComponent<LiveMixin>();
                    if (liveMixin == null || !liveMixin.IsAlive())
                        continue;

                    if (fishInInventory.ContainsKey(liveMixin))
                    {
                        float timeOutOfWater = DayNightCycle.main.timePassedAsFloat - fishInInventory[liveMixin];
                        if (timeOutOfWater > Main.config.outOfWaterLifeTime * seconds)
                            KillFish(liveMixin);
                    }
                    else
                        fishInInventory.Add(liveMixin, DayNightCycle.main.timePassedAsFloat);
                }
            }
        }

        private static void CheckFishInInventory()
        {
            if (Player.main.inExosuit)
            {
                Exosuit exosuit = Player.main.GetComponentInParent<Exosuit>();
                if (exosuit && !exosuit.IsUnderwater() && exosuit.storageContainer)
                    CheckFishInContainer(exosuit.storageContainer.container);
            }
            if (Player.main.IsUnderwaterForSwimming())
                fishInInventory = new Dictionary<LiveMixin, float>();
            else
                CheckFishInContainer(Inventory.main.container);
        }

        private static void CheckFish()
        {
            if (fishOutOfWater.Count == 0)
                return;

            //AddDebug("CheckFish Count " + dict.Count);
            List<LiveMixin> fishToKill = new List<LiveMixin>();
            foreach (KeyValuePair<LiveMixin, float> pair in fishOutOfWater)
            {
                LiveMixin liveMixin = pair.Key;
                float timeOutOfWater = DayNightCycle.main.timePassedAsFloat - fishOutOfWater[liveMixin];
                //AddDebug("CheckFish " + liveMixin.gameObject.name + " timeOutOfWater " + timeOutOfWater);
                if (timeOutOfWater > Main.config.outOfWaterLifeTime * seconds)
                    fishToKill.Add(liveMixin);
            }
            foreach (LiveMixin lm in fishToKill)
            {
                fishOutOfWater.Remove(lm);
                KillFish(lm);
            }
        }

        static void KillFish(LiveMixin liveMixin)
        {
            //AddDebug("KillFish " + liveMixin.gameObject.name);
            fishInInventory.Remove(liveMixin);
            fishOutOfWater.Remove(liveMixin);
            liveMixin.health = 0f;
            liveMixin.tempDamage = 0f;
            liveMixin.SyncUpdatingState();
            AquariumFish af = liveMixin.GetComponent<AquariumFish>();
            if (af)
                UnityEngine.Object.Destroy(af);

            Locomotion locomotion = liveMixin.GetComponent<Locomotion>();
            locomotion.enabled = false;
            //CreatureDeath creatureDeath = liveMixin.GetComponent<CreatureDeath>();
            Eatable eatable = liveMixin.GetComponent<Eatable>();
            eatable.SetDecomposes(true);
            Rigidbody rb = liveMixin.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.None;
                WorldForces worldForces = liveMixin.GetComponent<WorldForces>();
                if (worldForces)
                    worldForces.handleDrag = false;

                rb.drag = Mathf.Max(rb.drag, 1f);
                rb.angularDrag = Mathf.Max(rb.angularDrag, 1f);
            }
            liveMixin.gameObject.EnsureComponent<EcoTarget>().SetTargetType(EcoTargetType.DeadMeat);

            PlayerTool playerTool = Inventory.main.GetHeldTool();
            if (playerTool && playerTool.gameObject.Equals(liveMixin.gameObject))
                Inventory.main.quickSlots.DeselectImmediate(); // prevent bug: equipped fish moving down
        }

        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            public static void DropPostfix(Pickupable __instance, Vector3 dropPosition)
            {
                if (Player.main.IsUnderwaterForSwimming())
                    return;

                LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                if (liveMixin && liveMixin.IsAlive())
                {
                    float t = DayNightCycle.main.timePassedAsFloat;
                    if (fishInInventory.ContainsKey(liveMixin))
                        t = fishInInventory[liveMixin];

                    //AddDebug("Pickupable Drop " + __instance.gameObject.name);
                    fishOutOfWater.Add(liveMixin, t);
                    //fishInInventory.Remove(liveMixin);
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Pickup")]
            public static void PickupPostfix(Pickupable __instance)
            {
                //AddDebug("Pickupable Pickup " + __instance.gameObject.name);
                LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                if (liveMixin && fishOutOfWater.ContainsKey(liveMixin))
                {
                    if (Player.main.IsUnderwater())
                    {
                        //AddDebug("reset time " + liveMixin.gameObject.name);
                        //fishOutOfWater.Remove(liveMixin);
                    }
                    //else
                    //    CheckFish(liveMixin);
                }
            }
        }

        [HarmonyPatch(typeof(Exosuit), "OnPilotModeEnd")]
        class SExosuit_OnPilotModeEnd_Patch
        {
            public static void Postfix(Exosuit __instance)
            {
                //AddDebug("OnPilotModeEnd " + __instance.IsUnderwater());
                if (!__instance.IsUnderwater() && __instance.storageContainer)
                {
                    foreach (InventoryItem item in __instance.storageContainer.container)
                    {
                        LiveMixin liveMixin = item.item.GetComponent<LiveMixin>();
                        if (liveMixin && liveMixin.IsAlive() && fishInInventory.ContainsKey(liveMixin))
                        {
                            //AddDebug("move fish " + liveMixin.name);
                            fishOutOfWater.Add(liveMixin, fishInInventory[liveMixin]);
                            fishInInventory.Remove(liveMixin);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnRemoveItem")]
        class Inventory_OnRemoveItem_Patch
        {
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("Inventory OnRemoveItem " + item.item.name);
                GameObject go = item.item.gameObject;
                if (IsEatableFish(go))
                {
                    LiveMixin liveMixin = go.GetComponent<LiveMixin>();
                    fishInInventory.Remove(liveMixin);
                }
            }
        }

        [HarmonyPatch(typeof(ItemsContainer), "NotifyAddItem")]
        class ItemsContainer_NotifyAddItem_Patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                if (Player.main.IsUnderwaterForSwimming() || item.item == null || __instance.Equals(Inventory.main.container))
                    return;

                GameObject go = item.item.gameObject;
                if (IsEatableFish(go))
                {
                    LiveMixin liveMixin = go.GetComponent<LiveMixin>();
                    if (liveMixin == null || !liveMixin.IsAlive())
                        return;

                    string parentName = __instance.tr.parent.gameObject.name;
                    if (parentName == "SeaTruckAquariumModule(Clone)" || parentName == "Aquarium(Clone)")
                        return;

                    //AddDebug(__instance.tr.name + " NotifyAddItem ");
                    //AddDebug(parentName + " NotifyAddItem ");
                    float t = DayNightCycle.main.timePassedAsFloat;
                    if (fishInInventory.ContainsKey(liveMixin))
                        t = fishInInventory[liveMixin];
                    else if (fishOutOfWater.ContainsKey(liveMixin))
                        return;

                    fishOutOfWater.Add(liveMixin, t);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "TrackTravelStats")]
        class Player_TrackTravelStats_Patch
        {
            public static void Postfix(Player __instance)
            {
                if (uGUI.isLoading)
                    return;
                //AddDebug("IsUnderwaterForSwimming " + Player.main.IsUnderwaterForSwimming());
                //AddDebug("IsUnderwater " + Player.main.IsUnderwater());
                CheckFish();
                CheckFishInInventory();
            }
        }

        //[HarmonyPatch(typeof(PDA))]
        public class PDA_Open_Patch
        {
            //[HarmonyPatch(nameof(PDA.Open))]
            public static void Postfix(PDA __instance, PDATab tab)
            {
                //AddDebug("tab " + tab);
                //AddDebug("usedStorage.Count " + Inventory.main.usedStorage.Count);
                //ItemsContainer container = GetOpenContainer();
                //if (container != null)
                //    AddFishToList(container);

                //if (!Player.main.IsUnderwater())
                //    Player.main.StartCoroutine(KillCoroutine());
            }
        }

    }
}