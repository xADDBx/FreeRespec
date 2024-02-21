using HarmonyLib;
using Kingmaker.UnitLogic;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace FreeRespec;

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;
    public static Settings settings;

    static bool Load(UnityModManager.ModEntry modEntry) {
        log = modEntry.Logger;
        settings = Settings.Load<Settings>(modEntry);
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    public enum Mode {
        [Description("Free")]
        Free,
        [Description("Always pay x")]
        StaticCost,
        [Description("Pay at most x")]
        AtMost,
        [Description("Normal Game Behaviour")]
        Normal
    }
    private static void OnSaveGUI(ModEntry modEntry) {
        settings.Save(modEntry);
    }

    private static void OnGUI(ModEntry modEntry) {
        // Mode selection buttons
        GUILayout.Label("Select Mode:");

        Mode[] modes = { Mode.Free, Mode.StaticCost, Mode.AtMost, Mode.Normal };
        int selectedIndex = Array.IndexOf(modes, settings.chosenMode);

        selectedIndex = GUILayout.SelectionGrid(selectedIndex, modes.Select(m => m.ToDescriptionString()).ToArray(), 4);

        settings.chosenMode = modes[selectedIndex];

        // Display additional controls based on the selected mode
        switch (settings.chosenMode) {
            case Mode.StaticCost:
                GUILayout.Label("Enter Static Cost:");
                int.TryParse(GUILayout.TextField(settings.val.ToString()), out settings.val);
                break;
            case Mode.AtMost:
                GUILayout.Label("Enter Maximum Cost:");
                int.TryParse(GUILayout.TextField(settings.val.ToString()), out settings.val);
                break;

            default:
                break;
        }
    }

    [HarmonyPatch(typeof(PartUnitProgression), nameof(PartUnitProgression.GetRespecCost))]
    public static class PartUnitProgression_GetRespecCost_Patch {
        [HarmonyPostfix]
        public static void GetRespecCost(ref int __result) {
            switch (settings.chosenMode) {
                case Mode.Normal: return;
                case Mode.Free: __result = 0; return;
                case Mode.StaticCost: __result = settings.val; return;
                case Mode.AtMost: __result = settings.val < __result ? settings.val : __result; return;
            }
        }
    }

    public static string ToDescriptionString(this Mode val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
}