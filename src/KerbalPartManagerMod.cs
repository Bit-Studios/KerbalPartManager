using UnityEngine;
using KSP.Game;
using SpaceWarp.API.Mods;
using Screen = UnityEngine.Screen;
using KSP.UI.Binding;
using KSP.Sim.impl;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.OAB;
using KSP.Modules;
using Shapes;
using SpaceWarp.API.Assets;
using SpaceWarp;
using BepInEx;
using SpaceWarp.API.UI.Appbar;
using KSP.Messages;
using Steamworks;
using UnityEngine.InputSystem;
using System.Reflection;
using KSP.Input;
using KSP.Logging;
using HarmonyLib;

namespace KerbalPartManager;
[BepInPlugin("com.shadowdev.partsmanager", "Part Manager", "1.0.1")]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class KerbalPartManagerMod : BaseSpaceWarpPlugin
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
    private static string SetWindowWidthStr = "500";
    private static IObjectAssemblyPart assemblyPart = null;
    private static Dictionary<string,string> SelectedRecources = new Dictionary<string,string>();
    private static bool justClicked = false;
    public static bool IsDev = true;
    public static bool MouseButtonDownIS = false;
    public static PartUnderMouseChanged partUnderMouseChanged;
    public static PartManagerOpenedMessage partManagerOpenedMessage;
    public override void OnInitialized()
    {
        Harmony.CreateAndPatchAll(typeof(KerbalPartManagerMod));
        Appbar.RegisterAppButton(
           "Parts Menu Config",
            "BTN-PMC",
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"), ToggleButton);
        GameManager.Instance.Game.Messages.Subscribe<PartUnderMouseChanged>(PartChangedUnderMouse);
    }
    void Awake()
    {
        windowRectConfig = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        windowRectPartMenu = new Rect((Screen.width * 0.85f) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), 0, 0);
        
    }
    [HarmonyPatch(typeof(PartsManagerCore))]
    [HarmonyPatch("IsVisible",MethodType.Setter)]
    [HarmonyPrefix]
    public static bool GameInstance_IsVisible()
    {
        if (Input.GetMouseButtonDown(1) || MouseButtonDownIS == true)
        {
            MouseButtonDownIS = false;
            return false;
        }
        return true;


    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            
            justClicked = true;
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
                        MouseButtonDownIS = true;
                        SelectedObject = partUnderMouseChanged.newPartUnderMouse.Rigidbody.gameObject;
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
            if (gameStateConfiguration.IsObjectAssembly && GameManager.Instance.Game.OAB.Current.ActivePartTracker.partGrabbed == null)
            {
                var tempobj = GameObject.Find("OAB(Clone)");
                if(tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.PartsUnderCursor.Length > 0)
                {
                    MouseButtonDownIS = true;
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

    private void PartChangedUnderMouse(MessageCenterMessage message)
    {
        partUnderMouseChanged = message as PartUnderMouseChanged;

    }
    void ToggleButton(bool toggle)
    {
        showConfigUI = toggle;
        GameObject.Find("BTN-PMC")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(toggle);
        
    }
    void OnGUI()
    {
        GUI.skin = SpaceWarp.API.UI.Skins.ConsoleSkin;
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
                        break;
                    case "Light":
                        Data_Light data_Light = new Data_Light();
                        module.DataModules.TryGetByType<Data_Light>(out data_Light);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Red ({data_Light.lightColorR.GetValue()})", GUILayout.Width(windowWidth / 2));
                        data_Light.lightColorR.SetValue(GUILayout.HorizontalSlider(data_Light.lightColorR.GetValue(), 0.0f, 1.0f));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Green ({data_Light.lightColorG.GetValue()})", GUILayout.Width(windowWidth / 2));
                        data_Light.lightColorG.SetValue(GUILayout.HorizontalSlider(data_Light.lightColorG.GetValue(), 0.0f, 1.0f));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Blue ({data_Light.lightColorB.GetValue()})", GUILayout.Width(windowWidth / 2));
                        data_Light.lightColorB.SetValue(GUILayout.HorizontalSlider(data_Light.lightColorB.GetValue(), 0.0f, 1.0f));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"blink Rate ({data_Light.blinkRate.GetValue()})", GUILayout.Width(windowWidth / 2));
                        data_Light.blinkRate.SetValue(GUILayout.HorizontalSlider(data_Light.blinkRate.GetValue(), 0.0f, 100.0f));
                        GUILayout.EndHorizontal();

                        if (data_Light.canRotate)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"rotation Angle ({data_Light.rotationAngle.GetValue()})", GUILayout.Width(windowWidth / 2));
                            data_Light.rotationAngle.SetValue(GUILayout.HorizontalSlider(data_Light.rotationAngle.GetValue(), 0.0f, 360f));
                            GUILayout.EndHorizontal();
                        }
                        if (data_Light.canPitch)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"pitch Angle ({data_Light.pitchAngle.GetValue()})", GUILayout.Width(windowWidth / 2));
                            data_Light.pitchAngle.SetValue(GUILayout.HorizontalSlider(data_Light.pitchAngle.GetValue(), 0.0f, 360f));
                            GUILayout.EndHorizontal();
                        }
                        break;
                    case "Reaction Wheel":
                        Data_ReactionWheel data_ReactionWheel = new Data_ReactionWheel();
                        module.DataModules.TryGetByType<Data_ReactionWheel>(out data_ReactionWheel);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"> Torque Mode", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("All"))
                        {
                            
                            data_ReactionWheel.WheelActuatorMode.SetValue(Data_ReactionWheel.ActuatorModes.All);
                        }
                        if (GUILayout.Button("ManualOnly"))
                        {

                            data_ReactionWheel.WheelActuatorMode.SetValue(Data_ReactionWheel.ActuatorModes.ManualOnly);
                        }
                        if (GUILayout.Button("SASOnly"))
                        {

                            data_ReactionWheel.WheelActuatorMode.SetValue(Data_ReactionWheel.ActuatorModes.SASOnly);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Wheel Authority ({(float)data_ReactionWheel.WheelAuthority.GetObject()})", GUILayout.Width(windowWidth / 2));
                        data_ReactionWheel.WheelAuthority.SetValue(GUILayout.HorizontalSlider((float)data_ReactionWheel.WheelAuthority.GetObject(), 0.0f,1.0f));
                        GUILayout.EndHorizontal();
                        break;
                    case "RCS Thruster":
                        Data_RCS data_RCS = new Data_RCS();
                        module.DataModules.TryGetByType<Data_RCS>(out data_RCS);
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Thrust Limiter ({(float)data_RCS.thrustPercentage.GetObject()})", GUILayout.Width(windowWidth / 2));
                        data_RCS.thrustPercentage.SetValue(GUILayout.HorizontalSlider((float)data_RCS.thrustPercentage.GetObject(), 0.0f, 100.0f));
                        GUILayout.EndHorizontal();
                        break;
                    case "Engine Gimbal":
                        Data_Gimbal data_Gimbal = new Data_Gimbal();
                        module.DataModules.TryGetByType<Data_Gimbal>(out data_Gimbal);
                        break;
                    case "Engine":
                        Data_Engine data_Engine = new Data_Engine();
                        module.DataModules.TryGetByType<Data_Engine>(out data_Engine);
                        Module_Engine module_Engine = (Module_Engine)module;
                        Debug.Log($"EngineModeString {data_Engine.EngineModeString} {data_Engine.currentEngineModeIndex}");
                        if(data_Engine.engineModes.Length > 1) {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Engine Mode ({data_Engine.EngineModeString.GetValue()})", GUILayout.Width(windowWidth / 1.5f));
                            if (GUILayout.Button("Change"))
                            {
                                module_Engine.ChangeEngineMode();
                            }
                            GUILayout.EndHorizontal();
                        }
                        if (data_Engine.IndependentThrottle.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Thrust Limiter ({(float)data_Engine.IndependentThrottlePercentage.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_Engine.IndependentThrottlePercentage.SetValue(GUILayout.HorizontalSlider((float)data_Engine.IndependentThrottlePercentage.GetObject(), 0.0f, 100.0f));
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
                            data_SolarPanel.toggleExtend.SetValue(!data_SolarPanel.toggleExtend.GetValue());
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
                            data_WheelBase.AutoFriction.SetValue(!data_WheelBase.AutoFriction.GetValue());
                        }
                        GUILayout.EndHorizontal();
                        if (!data_WheelBase.AutoFriction.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Friction ({(float)data_WheelBase.FrictionMultiplier.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_WheelBase.FrictionMultiplier.SetValue(GUILayout.HorizontalSlider((float)data_WheelBase.FrictionMultiplier.GetObject(), 0.0f, 10.0f));
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
                            data_WheelMotor.autoTorque.SetValue(!data_WheelMotor.autoTorque.GetValue());
                        }
                        GUILayout.EndHorizontal();
                        if (!data_WheelMotor.autoTorque.GetValue())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Traction ({(float)data_WheelMotor.tractionControlScale.GetObject()})", GUILayout.Width(windowWidth / 2));
                            data_WheelMotor.tractionControlScale.SetValue(GUILayout.HorizontalSlider((float)data_WheelMotor.tractionControlScale.GetObject(), 0.0f, 10.0f));
                            GUILayout.EndHorizontal();
                        }

                        break;
                    case "Module_DockingNode":
                        Data_DockingNode data_DockingNode = new Data_DockingNode();
                        module.DataModules.TryGetByType<Data_DockingNode>(out data_DockingNode);
                        if(data_DockingNode.CurrentState == Data_DockingNode.DockingState.Docked)
                        {
                            if (GUILayout.Button("Undock"))
                            {
                                Module_DockingNode.UndockModule(data_DockingNode.DockedPartId.Guid.ToString());
                            }
                                
                        }
                        break;
                    case "Fairing":
                        Data_Fairing data_Fairing = new Data_Fairing();
                        module.DataModules.TryGetByType<Data_Fairing>(out data_Fairing);
                        GUILayout.Label($"Length ({(float)data_Fairing.Length.GetObject()})", GUILayout.Width(windowWidth / 2));
                        if (gameStateConfiguration.IsFlightMode) { } else
                        {
                            data_Fairing.Length.SetValue(GUILayout.HorizontalSlider((float)data_Fairing.Length.GetObject(), 0.0f, data_Fairing.LengthEditMaximum));
                        }
                        break;
                    case "Module_ToggleCrossfeed":
                        Module_ToggleCrossfeed module_ToggleCrossfeed = (Module_ToggleCrossfeed)Modules.Values.ToArray()[ModuleID];
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Toggle Crossfeed ({module.part.Model.FuelCrossfeed})", GUILayout.Width(windowWidth / 2));
                        if (GUILayout.Button("Toggle"))
                        {

                            module_ToggleCrossfeed.ToggleCrossFeed();
                        }
                        GUILayout.EndHorizontal();
                        break;
                    case "Decoupler":
                        Data_Decouple data_Decouple = new Data_Decouple();
                        module.DataModules.TryGetByType<Data_Decouple>(out data_Decouple);
                        GUILayout.Label($"Ejection Impulse ({(float)data_Decouple.EjectionImpulse.GetObject()})", GUILayout.Width(windowWidth / 2));
                        data_Decouple.EjectionImpulse.SetValue(GUILayout.HorizontalSlider((float)data_Decouple.EjectionImpulse.GetObject(), 0.0f, 100f));
                        break;
                    default: break;
                }
                module.ModuleActions.ForEach(action => {
                    if (IsDev)
                    {
                        Debug.Log($"|{module.GetModuleDisplayName()}|{action.ActionType}|{action.ActionState}|{action.DisplayName}");
                    }
                    List<string> disabledTypes = new List<string> { "Module_WheelBase", "Module_WheelMotor" };
                    if (action.ActionType == KSPActionType.Toggle)
                    {
                        if (disabledTypes.Contains(module.GetModuleDisplayName())) { } else
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{action.DisplayName} ({action.StateProperty.GetValue()})", GUILayout.Width(windowWidth / 2));
                            if (GUILayout.Button("Toggle"))
                            {
                                action.ActionState = !action.ActionState;
                                action.StateProperty.SetValue(!action.StateProperty.GetValue());
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