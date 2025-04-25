using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using UnityEngine;
using UnityModManagerNet;

namespace ArmyManager
{
    // ReSharper disable InconsistentNaming
    // [EnableReloading]
    public class Main
    {
        public static Settings ModSettings = new Settings();

        public static bool Load(UnityModManager.ModEntry entry)
        {
            var harmony = new Harmony(entry.Info.Id);
            harmony.PatchAll();

            ModSettings = UnityModManager.ModSettings.Load<Settings>(entry);

            entry.OnSaveGUI = OnSaveGUI;
            entry.OnGUI = OnGUI;
            entry.OnToggle = OnToggle;

            entry.Hotkey = new KeyBinding();
            entry.Hotkey.Change(KeyCode.A, true, false, false);
            return true;
        }
        public static bool OnToggle(UnityModManager.ModEntry entry, bool value)
        {
            ModSettings.Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!ModSettings.Enabled || !IsInGame) return;

            if (GameStateManager.IsValid())
            {
                ArmiesUI.Update();
            }

            ArmiesUI.OnGUI();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry entry)
        {
            Main.ModSettings.Save(entry);
        }

        public static bool IsInGame => GameControl.initialized;
    }
}
