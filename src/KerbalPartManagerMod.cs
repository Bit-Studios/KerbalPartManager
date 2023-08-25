using KSP.Game;
using KSP.Messages;
using HarmonyLib;
using BepInEx;

namespace BetterPartsManager;
[BepInPlugin("computer.shadow.mods.bpm", "BetterPartsManager", "2.0.0")]
public class BetterPartsManagerMod : BaseUnityPlugin
{
    public static string ModId = "BetterPartsManager";
    public static bool IsDev = true;

    public static bool AllowUpdate = true;
    void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(LegacyMode));
        GameManager.Instance.Game.Messages.Subscribe<GameStateChangedMessage>(GameStateChanged);
    }
    void GameStateChanged(MessageCenterMessage messageCenterMessage)
    {
        GameStateChangedMessage gameStateChangedMessage = messageCenterMessage as GameStateChangedMessage;
        AllowUpdate = true;
        GameManager.Instance.Game.PartsManager.IsVisible = true;
        GameManager.Instance.Game.PartsManager.IsVisible = false;
        //AllowUpdate = true;
    }
}
public class LegacyMode
{
    [HarmonyPatch(typeof(PartsManagerPartsList))]
    [HarmonyPatch("MarkDirty")]
    [HarmonyPrefix]
    public static bool PartsManagerPartsList_MarkDirty(PartsManagerPartsList __instance)
    {
        if (BetterPartsManagerMod.AllowUpdate)
        {
            BetterPartsManagerMod.AllowUpdate = false;
            return true;
        }
        return BetterPartsManagerMod.AllowUpdate;
    }
}