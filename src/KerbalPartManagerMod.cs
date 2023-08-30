using UnityEngine;
using KSP.Game;
using KSP.Sim.impl;
using KSP.OAB;
using KSP.Messages;
using KSP.Modding;
using HarmonyLib;
using System.Reflection;
using KSP.Modules;
using Newtonsoft.Json;
using TMPro;
using KSP.UI.Binding.Core;
using KSP.Api;
using KSP.UI.Binding;
using UnityEngine.UI;
using System.Xml.Linq;

namespace BetterPartsManager;
public class ModConfig
{
    //public float scale {  get; set; }
    public ModConfig()
    {
        //this.scale = 0.5f;
    }
    public void SaveConfig()
    {
        File.WriteAllText($"./Configs/{BetterPartsManagerMod.ModId}.config", JsonConvert.SerializeObject(this));
    }
}
public class BetterPartsManagerMod : Mod
{
    public static string ModId = "BetterPartsManager";
    public static bool IsDev = true;
    public static ModConfig config;
    public static bool AllowUpdate = true;
    //void SetScaleSetting(float scale)
    //{
    //    config.scale = scale;
    //    config.SaveConfig();
    //}
    void Awake()
    {
        if (!Directory.Exists("./Configs"))
        {
            Directory.CreateDirectory("./Configs");
        }
        if (File.Exists(ModId))
        {
            config = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText($"./Configs/{BetterPartsManagerMod.ModId}.config"));
        }
        else
        {
            config = new ModConfig();
            config.SaveConfig();
        }
        GameManager.Instance.Game.Messages.Subscribe<GameStateChangedMessage>(GameStateChanged);
        Harmony.CreateAndPatchAll(typeof(BetterPartsManagerMod));
    }
    void RefreshPartsManager()
    {
        AllowUpdate = true;
        GameManager.Instance.Game.PartsManager.IsVisible = false;
    }
    void GameStateChanged(MessageCenterMessage messageCenterMessage)
    {
        GameStateChangedMessage gameStateChangedMessage = messageCenterMessage as GameStateChangedMessage;
        AllowUpdate = true;
        GameManager.Instance.Game.PartsManager.IsVisible = true;
        if (GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Parts Manager Window(Clone)/Root/UIPanel/GRP-Header (1)/Main Row/BTN-Ref"))
        {

        }
        else
        {
            GameObject BTNC = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Parts Manager Window(Clone)/Root/UIPanel/GRP-Header (1)/Main Row/BTN-Close");
            GameObject BTNR = GameObject.Instantiate(BTNC, BTNC.transform.parent.transform);
            BTNR.GetComponent<UIAction_Void_Button>().enabled = false;
            BTNR.GetComponent<ButtonExtended>().onClick.RemoveAllListeners();
            BTNR.GetComponent<ButtonExtended>().onClick.AddListener(RefreshPartsManager);
            BTNR.transform.localPosition = new Vector3(BTNR.transform.localPosition.x - 20, BTNR.transform.localPosition.y, BTNR.transform.localPosition.z);
            Texture2D refreshIcon = new Texture2D(512, 512);
            refreshIcon.LoadImage(File.ReadAllBytes($"./GameData/Mods/BetterPartsManager/assets/images/refresh.png"));
            BTNR.GetChild("Icon").GetComponent<Image>().sprite = Sprite.Create(refreshIcon , new Rect(0,0,512,512), BTNR.GetChild("Icon").GetComponent<Image>().sprite.pivot);
            BTNR.name = "BTN-Ref";
        }
        GameManager.Instance.Game.PartsManager.IsVisible = false;
        //AllowUpdate = true;
        //if(gameStateChangedMessage.CurrentState == GameState.VehicleAssemblyBuilder) {
        //    GameObject MaskPP = GameObject.Find("OAB(Clone)/HUDSpawner/HUD/widget_PartsPicker/mask_PartsPicker");
        //    GameObject bgpanel = GameObject.Find("OAB(Clone)/HUDSpawner/HUD/widget_PartsPicker/mask_PartsPicker/BG-panel");
        //    GameObject pipanel = GameObject.Find("OAB(Clone)/HUDSpawner/HUD/UI-Editor_Screen-Panel-Foreground/Part info tooltip (modalWidget)");
        //    MaskPP.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        //    MaskPP.transform.localPosition = new Vector3(0, -500, 0);
        //    RectTransform MaskPPRT = MaskPP.GetComponent<RectTransform>();
        //    MaskPPRT.sizeDelta = new Vector2(MaskPPRT.sizeDelta.x, 2048);
        //    RectTransform bgpanelRT = bgpanel.GetComponent<RectTransform>();
        //    bgpanelRT.sizeDelta = new Vector2(bgpanelRT.sizeDelta.x, 2048);
        //    pipanel.transform.localPosition = new Vector3(-710, 485, 0);
        //}
    }
    [HarmonyPatch(typeof(PartsManagerPartsList))]
    [HarmonyPatch("MarkDirty")]
    [HarmonyPrefix]
    public static bool PartsManagerPartsList_MarkDirty(PartsManagerPartsList __instance)
    {
        if (AllowUpdate)
        {
            AllowUpdate = false;
            return true;
        }
        return AllowUpdate;
    }
    //[HarmonyPatch(typeof(UIValue_WriteNumber_Slider))]
    //[HarmonyPatch("Slider_DragPositionChanged")]
    //[HarmonyPostfix]
    //public static void UIValue_WriteNumber_Slider_Slider_DragPositionChanged(UIValue_WriteNumber_Slider __instance)
    //{
    //    if (__instance.gameObject.transform.parent.transform.parent.transform.parent.name == "UserInterfaceSettingsMenu")
    //    {
    //        config.scale = (float)__instance.GetValue();
    //        config.SaveConfig();
    //    }
    //}
}