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
using KSP.Modules;
using static KSP.Modules.Data_ReactionWheel;
using Shapes;
using Mono.Cecil;
using KSP.Sim.ResourceSystem;

namespace KerbalPartManager;


[MainMod]
public class KerbalPartManagerMod : Mod
{
    private int windowWidth = 500;
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
    private static IObjectAssemblyPart assemblyPart = null;
    private static Dictionary<string,string> SelectedRecources = new Dictionary<string,string>();
    private static bool justClicked = false;
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
        
        windowRectConfig = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        windowRectPartMenu = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            justClicked = true;
            //GameManager.Instance.Game.PartsManager = null;
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
                        //GameManager.Instance.Game.PartsManager = null;
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
                    assemblyPart = tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.PartsUnderCursor.Last().Key;
                    SelectedObject = GameObject.Find("OAB(Clone)");
                    showPartMenuUI = true;
                }
                else if (pinned == false)
                {
                    showPartMenuUI = false;
                }
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
            //GameManager.Instance.Game.PartsManager = null;
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
        int x = 0;
        GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
        boxStyle = GUI.skin.GetStyle("Box");
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
        int ModuleID = 0;
        foreach (PartBehaviourModule module in Modules.Values){
            try
            {
                

                switch (module.GetModuleDisplayName())
                {
                    case "Command Module":
                        Module_Command module_Command = (Module_Command)Modules.Values.ToArray()[ModuleID];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"> Control Orientation", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Default"))
                        {
                            module_Command.SetControlPoint("Default", true);
                        }
                        if (GUILayout.Button("Reversed"))
                        {
                            module_Command.SetControlPoint("Reversed", true);
                        }
                        GUILayout.EndHorizontal();
                        break;
                    case "Lit Part":
                        //none
                        break;
                    case "Reaction Wheel":
                        Data_ReactionWheel data_ReactionWheel = new Data_ReactionWheel();
                        module.DataModules.TryGetByType<Data_ReactionWheel>(out data_ReactionWheel);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"> Torque Mode", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("All"))
                        {
                            
                            data_ReactionWheel.WheelActuatorMode = new ModuleProperty<Data_ReactionWheel.ActuatorModes>(Data_ReactionWheel.ActuatorModes.All);
                        }
                        if (GUILayout.Button("ManualOnly"))
                        {

                            data_ReactionWheel.WheelActuatorMode = new ModuleProperty<Data_ReactionWheel.ActuatorModes>(Data_ReactionWheel.ActuatorModes.ManualOnly);
                        }
                        if (GUILayout.Button("SASOnly"))
                        {

                            data_ReactionWheel.WheelActuatorMode = new ModuleProperty<Data_ReactionWheel.ActuatorModes>(Data_ReactionWheel.ActuatorModes.SASOnly);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Wheel Authority ({(float)data_ReactionWheel.WheelAuthority.GetObject()})", GUILayout.Width(windowWidth / 2));
                        data_ReactionWheel.WheelAuthority = new ModuleProperty<float>(GUILayout.HorizontalSlider((float)data_ReactionWheel.WheelAuthority.GetObject(), 0.0f,1.0f));
                        GUILayout.EndHorizontal();
                        break;
                    case "RCS Thruster":
                        Data_RCS data_RCS = new Data_RCS();
                        module.DataModules.TryGetByType<Data_RCS>(out data_RCS);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Thrust Limiter ({(float)data_RCS.thrustPercentage.GetObject()})", GUILayout.Width(windowWidth / 2));
                        data_RCS.thrustPercentage = new ModuleProperty<float>(GUILayout.HorizontalSlider((float)data_RCS.thrustPercentage.GetObject(), 0.0f, 100.0f));
                        GUILayout.EndHorizontal();
                        break;
                    case "Engine Gimbal":
                        Data_Gimbal data_Gimbal = new Data_Gimbal();
                        module.DataModules.TryGetByType<Data_Gimbal>(out data_Gimbal);
                        break;
                    case "Engine":
                        Data_Engine data_Engine = new Data_Engine();
                        module.DataModules.TryGetByType<Data_Engine>(out data_Engine);
                        
                        if (data_Engine.IndependentThrottle.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Thrust Limiter ({(float)data_Engine.IndependentThrottlePercentage.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_Engine.IndependentThrottlePercentage = new ModuleProperty<float>(GUILayout.HorizontalSlider((float)data_Engine.IndependentThrottlePercentage.GetObject(), 0.0f, 100.0f));
                            GUILayout.EndHorizontal();
                        }
                        break;
                    case "Solar Panel":
                        Data_Deployable data_SolarPanel = new Data_Deployable();
                        module.DataModules.TryGetByType<Data_Deployable>(out data_SolarPanel);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Extended ({data_SolarPanel.toggleExtend.GetValue()})", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Toggle"))
                        {
                            data_SolarPanel.toggleExtend = new ModuleProperty<bool>(!data_SolarPanel.toggleExtend.GetValue());
                        }
                        GUILayout.EndHorizontal();
                        break;
                    case "Module_WheelBase":
                        Data_WheelBase data_WheelBase = new Data_WheelBase();
                        module.DataModules.TryGetByType<Data_WheelBase>(out data_WheelBase);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Auto Friction ({data_WheelBase.AutoFriction.GetValue()})", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Toggle"))
                        {
                            data_WheelBase.AutoFriction = new ModuleProperty<bool>(!data_WheelBase.AutoFriction.GetValue());
                        }
                        GUILayout.EndHorizontal();
                        if (!data_WheelBase.AutoFriction.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Friction ({(float)data_WheelBase.FrictionMultiplier.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_WheelBase.FrictionMultiplier = new ModuleProperty<float>(GUILayout.HorizontalSlider((float)data_WheelBase.FrictionMultiplier.GetObject(), 0.0f, 10.0f));
                            GUILayout.EndHorizontal();
                        }
                        
                        break;
                    case "Module_WheelMotor":
                        Data_WheelMotor data_WheelMotor = new Data_WheelMotor();
                        module.DataModules.TryGetByType<Data_WheelMotor>(out data_WheelMotor);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Auto Traction ({data_WheelMotor.autoTorque.GetValue()})", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Toggle"))
                        {
                            data_WheelMotor.autoTorque = new ModuleProperty<bool>(!data_WheelMotor.autoTorque.GetValue());
                        }
                        GUILayout.EndHorizontal();
                        if (!data_WheelMotor.autoTorque.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Traction ({(float)data_WheelMotor.tractionControlScale.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_WheelMotor.tractionControlScale = new ModuleProperty<float>(GUILayout.HorizontalSlider((float)data_WheelMotor.tractionControlScale.GetObject(), 0.0f, 10.0f));
                            GUILayout.EndHorizontal();
                        }

                        break;
                    case "Module_DockingNode":
                        Data_DockingNode data_DockingNode = new Data_DockingNode();
                        module.DataModules.TryGetByType<Data_DockingNode>(out data_DockingNode);
                        break;
                    default: break;
                }
                module.ModuleActions.ForEach(action => {
                    Debug.Log($"|{module.GetModuleDisplayName()}|{action.ActionType}|{action.ActionState}|{action.DisplayName}");
                    List<string> disabledTypes = new List<string> { "Module_WheelBase", "Module_WheelMotor" };
                    if (action.ActionType == KSPActionType.Toggle)
                    {
                        if (disabledTypes.Contains(module.GetModuleDisplayName())) { } else
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{action.DisplayName} ({action.ActionState})", GUILayout.Width(windowWidth / 2));
                            if (GUILayout.Button("Toggle"))
                            {
                                action.ActionState = !action.ActionState;
                                action.StateProperty.SetValue(action.ActionState);
                            }
                            GUILayout.EndHorizontal();
                        }
                        
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        try
        {
            if (gameStateConfiguration.IsFlightMode)
            {
                SelectedObject.GetComponent<SimulationObjectView>().Part.Model.PartResourceContainer.GetAllResourcesContainedData().ForEach(resource =>
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName}", GUILayout.Width(windowWidth / 2));
                    GUILayout.Label($"Stored {resource.StoredUnits} | Max {resource.CapacityUnits}", GUILayout.Width(windowWidth / 2));
                    GUILayout.EndHorizontal();


                });
            }
            else
            {
                if (justClicked)
                {
                    SelectedRecources = new Dictionary<string, string>();
                }
                assemblyPart.Containers.ForEach(resourceCD =>
                {
                    resourceCD.ForEach(resourceID =>
                    {
                        var resource = resourceCD.GetResourceContainedData(resourceID);

                        if (justClicked)
                        {
                            SelectedRecources.Add(GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName, $"{resource.StoredUnits}");
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName}", GUILayout.Width(windowWidth / 3));
                        GUILayout.Label($"Stored {SelectedRecources[GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName]} | Max {resource.CapacityUnits}", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Set"))
                        {

                            resourceCD.SetResourceStoredUnits(resourceID, double.Parse(SelectedRecources[GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName]));

                        }
                        GUILayout.EndHorizontal();

                        SelectedRecources[GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName] = $"{GUILayout.HorizontalSlider(float.Parse(SelectedRecources[GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resource.ResourceID).DisplayName].ToString()), 0.0f, (float)resource.CapacityUnits)}";


                    });



                });

            }
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, windowWidth,700));
        justClicked = false;
    }
}