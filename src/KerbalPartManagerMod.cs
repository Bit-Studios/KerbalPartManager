using UnityEngine;
using KSP.Game;
using KSP.Sim.impl;
using KSP.OAB;
using KSP.Messages;
using KSP.Modding;
using HarmonyLib;
using System.Reflection;
using KSP.Modules;

namespace BetterPartsManager;
public class BetterPartsManagerMod : Mod
{
    public static string ModId = "BetterPartsManager";
    public static bool IsDev = true;

    public static bool AllowUpdate = true;
    void Awake()
    {
        GameManager.Instance.Game.Messages.Subscribe<GameStateChangedMessage>(GameStateChanged);
        Harmony.CreateAndPatchAll(typeof(BetterPartsManagerMod));
    }
    void GameStateChanged(MessageCenterMessage messageCenterMessage)
    {
        GameStateChangedMessage gameStateChangedMessage = messageCenterMessage as GameStateChangedMessage;
        AllowUpdate = true;
        GameManager.Instance.Game.PartsManager.IsVisible = true;
        GameManager.Instance.Game.PartsManager.IsVisible = false;
        //AllowUpdate = true;
    }
    [HarmonyPatch(typeof(PartsManagerPartsList))]
    [HarmonyPatch("MarkDirty")]
    [HarmonyPrefix]
    public static bool PartsManagerPartsList_MarkDirty(PartsManagerPartsList __instance)
    {
        //var _allParts = (Dictionary<IGGuid, IInteractivePart>)GameManager.Instance.Game.PartsManager.PartsList.GetType().GetField("_allParts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GameManager.Instance.Game.PartsManager.PartsList);
        //ClearAllUI();
        //try
        //{
        //    logger.Log($"LastPartID: {LastPartID}");
        //    logger.Log($"_allParts: {_allParts.Keys}");
        //    AddAllUIForPart(_allParts[LastPartID]);
        //}
        //catch (Exception ex)
        //{
        //    logger.Log($"{ex}");
        //}
        if (AllowUpdate)
        {
            AllowUpdate = false;
            return true;
        }
        return AllowUpdate;
    }
}