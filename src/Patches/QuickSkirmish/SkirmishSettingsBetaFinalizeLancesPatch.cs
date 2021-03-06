using Harmony;

using BattleTech;
using BattleTech.UI;
using BattleTech.Save;

namespace MissionControl.Patches {
  [HarmonyPatch(typeof(SkirmishSettings_Beta), "FinalizeLances")]
  public class SkirmishSettingsBetaFinalizeLancesPatch {
    public static string UNLIMITED_LANCE_COST = "SP-999999999";
    public static string WAR_LANCE_COST = "SP-25000000";
    public static string BATTLE_LANCE_COST = "SP-20000000";
    public static string CLASH_LANCE_COST = "SP-15000000";

    static bool Prefix(SkirmishSettings_Beta __instance, ref LanceConfiguration __result) {
      if (UiManager.Instance.ClickedQuickSkirmish) {
        Main.Logger.Log($"[SkirmishSettingsBetaFinalizeLancesPatch Prefix] Patching FinalizeLances");
        CloudUserSettings playerSettings = ActiveOrDefaultSettings.CloudSettings;
        LastUsedLances lastUsedLances = playerSettings.LastUsedLances;

        if (lastUsedLances.ContainsKey(UNLIMITED_LANCE_COST)) {
          __result = lastUsedLances[UNLIMITED_LANCE_COST]; 
        } else if (lastUsedLances.ContainsKey(WAR_LANCE_COST)) {
          __result = lastUsedLances[WAR_LANCE_COST]; 
        } else if (lastUsedLances.ContainsKey(BATTLE_LANCE_COST)) {
          __result = lastUsedLances[BATTLE_LANCE_COST]; 
        } else if (lastUsedLances.ContainsKey(CLASH_LANCE_COST)) {
          __result = lastUsedLances[CLASH_LANCE_COST]; 
        } else {
          Main.Logger.LogError("[Quick Skirmish] Quick Skirmish cannot be used without a prevously used lance. Go into skirmish and launch at least once");
        }
        return false;
      }
      return true;
    }
  }
}