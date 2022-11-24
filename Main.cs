
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Fish_Out_Of_Water
{
    [QModCore]
    public class Main
    {
        internal static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
        //public static PDA pda;

        public static void Log(string str, QModManager.Utility.Logger.Level lvl = QModManager.Utility.Logger.Level.Info)
        {
            QModManager.Utility.Logger.Log(lvl, str);
        }

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
                //ErrorMessage.AddDebug("QuitGameAsync " + quitToDesktop);
                if (!quitToDesktop)
                {
                    Fish_Out_Of_Water.fishOutOfWater = new Dictionary<LiveMixin, float>();
                    Fish_Out_Of_Water.fishInInventory = new Dictionary<LiveMixin, float>();
                }
            }
        }

        [QModPatch]
        public static void Load()
        {

            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
        }
    }
}