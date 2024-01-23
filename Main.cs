
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;
using BepInEx;
using Nautilus.Handlers;

namespace Fish_Out_Of_Water
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        private const string
            MODNAME = "Fish can not live out of water",
            GUID = "qqqbbb.subnautica.fishOutOfWater",
            VERSION = "3.0.01";

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        public static void Setup()
        {
            //Player.main.isUnderwaterForSwimming.changedEvent.AddHandler(Player.main, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(Fish_Out_Of_Water.OnPlayerIsUnderwaterForSwimmingChanged));
            //pda = Player.main.GetPDA();
        }

        //[HarmonyPatch(typeof(uGUI_SceneLoading), "End")]
        internal class uGUI_SceneLoading_End_Patch
        { // when loading savegame runs more than once
            public static void Postfix(uGUI_SceneLoading __instance)
            {
                if (!uGUI.main.hud.active)
                {
                    //AddDebug(" Loading");
                    return;
                }
                Setup();
            }
        }

        [HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                //AddDebug("QuitGameAsync " + quitToDesktop);
                //BepInEx.Logging.Logger.CreateLogSource("IngameMenu_QuitGameAsync_Patch").Log(BepInEx.Logging.LogLevel.Error, "QuitGameAsync" + quitToDesktop);

                if (!quitToDesktop)
                {
                    Fish_Out_Of_Water.fishOutOfWater = new Dictionary<LiveMixin, float>();
                    Fish_Out_Of_Water.fishInInventory = new Dictionary<LiveMixin, float>();
                }
            }
        }

        private void Start()
        {
            //AddDebug("Mono Start ");
            //Logger.LogInfo("Mono Start");
            //config.Load();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

    }
}