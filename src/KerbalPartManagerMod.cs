using UnityEngine;
using KSP.Game;
using SpaceWarp.API.Mods;
using Screen = UnityEngine.Screen;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API;
using KSP.UI.Binding;
using KSP.Sim.impl;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP;
using KSP.Api.CoreTypes;
using System.Runtime.Serialization.Formatters.Binary;
using KSP.OAB;

namespace KerbalPartManager;


[MainMod]
public class KerbalPartManagerMod : Mod
{
    private int windowWidth = 700;
    private int windowHeight = 700;
    private Rect windowRectConfig;
    private Rect windowRectPartMenu;
    private static GUIStyle boxStyle;
    private bool showConfigUI = false;
    private bool showPartMenuUI = false;
    private GUISkin _spaceWarpUISkin;
    private static GameObject SelectedObject;
    private static int selectedItem = 0;
    private static bool basicmode = true;
    private static bool pinned = false;
    private static string SetWindowWidthStr = "700";
    private static IObjectAssemblyPart assemblyPart;

    public override void OnInitialized()
    {
        ResourceManager.TryGetAsset($"space_warp/swconsoleui/swconsoleUI/spacewarpConsole.guiskin", out _spaceWarpUISkin);
        SpaceWarpManager.RegisterAppButton(
            "Parts Menu Config",
            "BTN-PMC",
            SpaceWarpManager.LoadIcon(), ToggleButton);
        Logger.Info($"{Info.name} OnInitialized()");
    }
    void Awake()
    {
        GameManager.Instance.Game.PartsManager = null;
        windowRectConfig = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        windowRectPartMenu = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.Game.PartsManager = null;
            GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
            
            if (gameStateConfiguration.IsFlightMode)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    SimulationObjectView nullobj;
                    if (hit.rigidbody.gameObject.TryGetComponent<SimulationObjectView>(out nullobj))
                    {
                        GameManager.Instance.Game.PartsManager = null;
                        SelectedObject = hit.rigidbody.gameObject;

                        showPartMenuUI = true;
                    }
                    else if (pinned == false)
                    {
                        showPartMenuUI = false;
                    }
                }
                else if (pinned == false)
                {
                    showPartMenuUI = false;
                }
                
            }
            if (gameStateConfiguration.IsObjectAssembly)
            {
                var tempobj = GameObject.Find("OAB(Clone)");
                if(tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.PartsUnderCursor.Length > 0)
                {
                    assemblyPart = tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.PartsUnderCursor.First().Key;
                    SelectedObject = GameObject.Find("OAB(Clone)");
                    showPartMenuUI = true;
                }
                else if (pinned == false)
                {
                    showPartMenuUI = false;
                }
            }
            else if (pinned == false)
            {
                showPartMenuUI = false;
            }

        }
    }
    void ToggleButton(bool toggle)
    {
        showConfigUI = toggle;
        GameObject.Find("BTN-PMC")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        
    }
    void OnGUI()
    {
        GUI.skin = _spaceWarpUISkin;
        if (showConfigUI)
        {
            windowRectConfig = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRectConfig,
                FillConfigWindow,
                "Parts Menu Config",
                GUILayout.Height(0),
                GUILayout.Width(windowWidth));
        }
        if (showPartMenuUI)
        {
            GameManager.Instance.Game.PartsManager = null;
            windowRectPartMenu = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                windowRectPartMenu,
                FillPartWindow,
                "Parts Menu",
                GUILayout.Height(0),
                GUILayout.Width(windowWidth));
            
        }
    }
    private void FillConfigWindow(int windowID)
    {
        boxStyle = GUI.skin.GetStyle("Box");
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Window Width", GUILayout.Width(windowWidth / 2));
        SetWindowWidthStr = GUILayout.TextField($"{SetWindowWidthStr}");
        if (GUILayout.Button("set"))
        {
            windowWidth = int.Parse(SetWindowWidthStr);
            windowRectConfig = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
            windowRectPartMenu = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, windowWidth, 700));
    }
    private void FillPartWindow(int windowID)
    {
        GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
        
        string[] menuOptions = { "Basic", "Expanded" };
        selectedItem = GUILayout.SelectionGrid(selectedItem, menuOptions, 2);
        boxStyle = GUI.skin.GetStyle("Box");
        switch (selectedItem)
        {
            case 0:
                basicmode = true;
                break;
            case 1:
                basicmode = false;
                break;
            default:
                basicmode = true;
                break;
        }
        GUILayout.BeginVertical();
        if (GUI.Button(new Rect(windowWidth - 23, 6, 18, 18), "<b>x</b>", new GUIStyle(GUI.skin.button) { fontSize = 10, }))
        {
            showPartMenuUI = false;
        }
        if (pinned)
        {
            if (GUI.Button(new Rect(windowWidth - 76, 6, 40, 18), "<b><color=green>pin</color></b>", new GUIStyle(GUI.skin.button) { fontSize = 12 }))
            {
                pinned = !pinned;
            }
        }
        else
        {
            if (GUI.Button(new Rect(windowWidth - 76, 6, 40, 18), "<b><color=red>pin</color></b>", new GUIStyle(GUI.skin.button) { fontSize = 12 }))
            {
                pinned = !pinned;
            }
        }
        DictionaryValueList<Type, IPartModule> Modules = new DictionaryValueList<Type, IPartModule>();
        if (gameStateConfiguration.IsFlightMode)
        {
            GUILayout.Label($"<b>Selected Part: {SelectedObject.GetComponent<SimulationObjectView>().Part.GetDisplayName()}</b>");
            
            Modules = SelectedObject.GetComponent<SimulationObjectView>().Part.Modules;
        }
        if (gameStateConfiguration.IsObjectAssembly)
        {
            GUILayout.Label($"<b>Selected Part: {assemblyPart.Name}</b>");
            Modules = assemblyPart.Modules;
        }
        
        foreach (PartBehaviourModule module in Modules.Values){
            try
            {
                module.ModuleActions.ForEach(action => {
                    bool isManualSet = false;
                    List<string> ManualSet = new List<string> { "Control From Here" };
                    
                    if (ManualSet.Contains(action.DisplayName)) {
                        isManualSet = true;
                    }
                    Debug.Log($"|{action.ActionType}|{action.ActionState}|{action.DisplayName}");
                    switch (action.ActionType)
                    {
                        
                        case KSPActionType.Toggle:
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{action.DisplayName} ({action.ActionState})", GUILayout.Width(windowWidth / 2));
                            if (GUILayout.Button("Toggle"))
                            {
                                Debug.Log($"action.ActionState {action.ActionState} set to ${action.ActionState}");
                                
                                action.ActionState = !action.ActionState;
                                action.StateProperty.SetValue(action.ActionState);
                                Debug.Log($"{action.ActionState}");
                            }
                                
                            GUILayout.EndHorizontal();
                            break;
                        case KSPActionType.Event:
                            if(basicmode == false && isManualSet == false)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label($"{action.DisplayName}", GUILayout.Width(windowWidth / 2));
                                if (GUILayout.Button("Set"))
                                {
                                    Debug.Log($"action.ActionState {action.ActionState} set to ${action.ActionState}");
                                    action.ActionState = !action.ActionState;
                                    action.StateProperty.SetValue(action.ActionState);
                                    Debug.Log($"{action.ActionState}");
                                }
                                GUILayout.EndHorizontal();
                            }
                            else if(isManualSet == true)
                            {
                                switch (action.DisplayName)
                                {
                                    case "Control From Here":
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label($"{action.DisplayName}", GUILayout.Width(windowWidth / 2));
                                        if (GUILayout.Button("Set"))
                                        {
                                            module.part.SimObjectComponent.SetAsVesselControl();
                                        }
                                        GUILayout.EndHorizontal();
                                        break;
                                    default:
                                        break;
                                }
                                
                            }
                            else {
                                
                            }
                            break;
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, windowWidth,700));
    }
}