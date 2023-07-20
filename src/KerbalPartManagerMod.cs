using UnityEngine;
using KSP.Game;
using Screen = UnityEngine.Screen;
using KSP.UI.Binding;
using KSP.Sim.impl;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.OAB;
using KSP.Modules;
using KSP.Messages;
using ShadowUtilityLIB;
using Logger = ShadowUtilityLIB.logging.Logger;
using KSP.Modding;
using HarmonyLib;
using KSP.UI.Flight;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UitkForKsp2.API;
using DragManipulator = BetterPartsManager.UI.DragManipulator;
using ShadowUtilityLIB.UI;
using Position = UnityEngine.UIElements.Position;
using Button = UnityEngine.UIElements.Button;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace BetterPartsManager;
public class BetterPartsManagerMod : Mod
{
    public static string ModId = "BetterPartsManager";
    private Logger logger = new Logger("Better Parts Manager","");
    private static DragManipulator dragArea = new DragManipulator();
    public static Manager manager;
    private bool showPartMenuUI = false;

    public int menuHeight = 15;

    private static GameObject SelectedObject;
    private static int selectedItem = 0;
    private static bool basicmode = true;
    private static bool pinned = false;
    private static IObjectAssemblyPart assemblyPart = null;
    private static Dictionary<string,string> SelectedRecources = new Dictionary<string,string>();
    private static bool justClicked = false;
    public static bool IsDev = false;
    public static bool MouseButtonDownIS = false;
    public static PartUnderMouseChanged partUnderMouseChanged;
    public static PartManagerOpenedMessage partManagerOpenedMessage;


    void Awake()
    {
        Harmony.CreateAndPatchAll(typeof(BetterPartsManagerMod));
        manager = new Manager();
        GameManager.Instance.Game.Messages.Subscribe<PartUnderMouseChanged>(PartChangedUnderMouse);
        GenerateUI();
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
    void GenerateUI()
    {
        float spaceVector = 2;
        int fontsize = 7;
        int fontsizeLG = 10;
        VisualElement BetterPartsManagerUI = Element.Root("BetterPartsManagerUI");
        dragArea = new DragManipulator();
        dragArea.deadzone = new Vector2[2] { new Vector2(70, 15), new Vector2(200, 200) };
        BetterPartsManagerUI.AddManipulator(dragArea);


        logger.Debug($"width {Screen.width}");
        logger.Debug($"height {Screen.height}");
        BetterPartsManagerUI.style.position = Position.Absolute;
        BetterPartsManagerUI.style.width = 250;
        BetterPartsManagerUI.style.height = menuHeight;
        BetterPartsManagerUI.style.paddingBottom = 0;
        BetterPartsManagerUI.style.paddingLeft = 0;
        BetterPartsManagerUI.style.paddingRight = 0;
        BetterPartsManagerUI.style.paddingTop = 0;
        BetterPartsManagerUI.style.borderBottomWidth = 0;
        BetterPartsManagerUI.style.borderTopWidth = 0;
        BetterPartsManagerUI.style.borderLeftWidth = 0;
        BetterPartsManagerUI.style.borderRightWidth = 0;
        BetterPartsManagerUI.style.backgroundColor = new StyleColor(new Color32(195, 195, 195, 255));
        BetterPartsManagerUI.style.borderRightColor = new StyleColor(new Color32(195, 195, 195, 0));
        BetterPartsManagerUI.style.borderLeftColor = new StyleColor(new Color32(195, 195, 195, 0));
        BetterPartsManagerUI.style.borderTopColor = new StyleColor(new Color32(195, 195, 195, 0));
        BetterPartsManagerUI.style.borderBottomColor = new StyleColor(new Color32(195, 195, 195, 0));
        BetterPartsManagerUI.style.left = manager.WidthScaleLimit / 1.5f;
        BetterPartsManagerUI.style.top = manager.HeightScaleLimit / 4;

        VisualElement BetterPartsManagerUI_TitleBar = new VisualElement();
        BetterPartsManagerUI_TitleBar.style.position = Position.Absolute;
        BetterPartsManagerUI_TitleBar.style.width = 250;
        BetterPartsManagerUI_TitleBar.style.height = 15f;
        BetterPartsManagerUI_TitleBar.style.top = 0f;
        BetterPartsManagerUI_TitleBar.style.left = 0;
        BetterPartsManagerUI_TitleBar.style.paddingLeft = 0;
        BetterPartsManagerUI_TitleBar.style.backgroundColor = new StyleColor(new Color32(242,157,36, 255));
        BetterPartsManagerUI_TitleBar.name = "BetterPartsManagerUI_TitleBar";
        BetterPartsManagerUI.Add(BetterPartsManagerUI_TitleBar);

        Label BetterPartsManagerUI_TitleBar_Title = Element.Label("BetterPartsManagerUI_TitleBar_Title", "No part selected");
        BetterPartsManagerUI_TitleBar_Title.style.fontSize = fontsize;
        BetterPartsManagerUI.Add(BetterPartsManagerUI_TitleBar_Title);

        Button BetterPartsManagerUI_TitleBar_Close = Element.Button("BetterPartsManagerUI_TitleBar_Close", $"X");
        BetterPartsManagerUI_TitleBar_Close.style.position = Position.Absolute;
        BetterPartsManagerUI_TitleBar_Close.style.width = 10;
        BetterPartsManagerUI_TitleBar_Close.style.height = 10;
        BetterPartsManagerUI_TitleBar_Close.style.paddingTop = 2;
        BetterPartsManagerUI_TitleBar_Close.style.paddingBottom = 2;
        BetterPartsManagerUI_TitleBar_Close.style.paddingLeft = 2;
        BetterPartsManagerUI_TitleBar_Close.style.paddingRight = 2;
        BetterPartsManagerUI_TitleBar_Close.style.top = 3.5f;
        BetterPartsManagerUI_TitleBar_Close.style.right = 5;
        BetterPartsManagerUI_TitleBar_Close.style.fontSize = fontsizeLG;
        BetterPartsManagerUI_TitleBar_Close.style.color = Color.white;
        BetterPartsManagerUI_TitleBar_Close.style.backgroundColor = Color.red;
        BetterPartsManagerUI_TitleBar_Close.style.borderRightColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Close.style.borderLeftColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Close.style.borderTopColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Close.style.borderBottomColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Close.clickable = new Clickable(() => {
            showPartMenuUI = false;
            pinned = false;
            manager.Set("BetterPartsManager", showPartMenuUI);
            logger.Debug($"Button Toggle: {showPartMenuUI}");
        });
        BetterPartsManagerUI.Add(BetterPartsManagerUI_TitleBar_Close);

        Button BetterPartsManagerUI_TitleBar_Pin = Element.Button("BetterPartsManagerUI_TitleBar_Close", $"P");
        BetterPartsManagerUI_TitleBar_Pin.style.position = Position.Absolute;
        BetterPartsManagerUI_TitleBar_Pin.style.width = 10;
        BetterPartsManagerUI_TitleBar_Pin.style.height = 10;
        BetterPartsManagerUI_TitleBar_Pin.style.paddingTop = 2;
        BetterPartsManagerUI_TitleBar_Pin.style.paddingBottom = 2;
        BetterPartsManagerUI_TitleBar_Pin.style.paddingLeft = 2;
        BetterPartsManagerUI_TitleBar_Pin.style.paddingRight = 2;
        BetterPartsManagerUI_TitleBar_Pin.style.top = 3.5f;
        BetterPartsManagerUI_TitleBar_Pin.style.right = 20;
        BetterPartsManagerUI_TitleBar_Pin.style.fontSize = fontsizeLG;
        BetterPartsManagerUI_TitleBar_Pin.style.color = Color.white;
        BetterPartsManagerUI_TitleBar_Pin.style.backgroundColor = Color.red;
        BetterPartsManagerUI_TitleBar_Pin.style.borderRightColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Pin.style.borderLeftColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Pin.style.borderTopColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Pin.style.borderBottomColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_TitleBar_Pin.clickable = new Clickable(() => {
            pinned = !pinned;
            if (pinned)
            {
                BetterPartsManagerUI_TitleBar_Pin.style.backgroundColor = Color.green;
            }
            else
            {
                BetterPartsManagerUI_TitleBar_Pin.style.backgroundColor = Color.red;
            }
            
            logger.Debug($"pinned Toggle: {pinned}");
        });
        BetterPartsManagerUI.Add(BetterPartsManagerUI_TitleBar_Pin);

        VisualElement BetterPartsManagerUI_Labels = new VisualElement();
        BetterPartsManagerUI_Labels.style.position = Position.Absolute;
        BetterPartsManagerUI_Labels.style.width = 115f;
        BetterPartsManagerUI_Labels.style.height = menuHeight - 15;
        BetterPartsManagerUI_Labels.style.top = 15;
        BetterPartsManagerUI_Labels.style.left = 0;
        BetterPartsManagerUI_Labels.style.paddingLeft = 0;
        BetterPartsManagerUI_Labels.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_Labels.name = "BetterPartsManagerUI_Labels";
        BetterPartsManagerUI.Add(BetterPartsManagerUI_Labels);

        VisualElement BetterPartsManagerUI_Options = new VisualElement();
        BetterPartsManagerUI_Options.style.position = Position.Absolute;
        BetterPartsManagerUI_Options.style.width = 135f;
        BetterPartsManagerUI_Options.style.height = menuHeight - 15;
        BetterPartsManagerUI_Options.style.top = 15;
        BetterPartsManagerUI_Options.style.left = 116f;
        BetterPartsManagerUI_Options.style.paddingLeft = 10;
        BetterPartsManagerUI_Options.style.paddingRight = 20;
        BetterPartsManagerUI_Options.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 0));
        BetterPartsManagerUI_Options.name = "BetterPartsManagerUI_Options";
        BetterPartsManagerUI.Add(BetterPartsManagerUI_Options);

        UIDocument window = Window.CreateFromElement(BetterPartsManagerUI);
        manager.Add("BetterPartsManager", window, false);

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
                        FillPartWindow();
                        manager.Set("BetterPartsManager", showPartMenuUI);
                    }
                    else if (pinned == false)
                    {
                        showPartMenuUI = false;
                        manager.Set("BetterPartsManager", showPartMenuUI);
                    }
                }
                else if (pinned == false)
                {
                    showPartMenuUI = false;
                    manager.Set("BetterPartsManager", showPartMenuUI);
                }
                
            }
            if (gameStateConfiguration.IsObjectAssembly && GameManager.Instance.Game.OAB.Current.ActivePartTracker.partGrabbed == null)
            {
                var tempobj = GameObject.Find("OAB(Clone)");
                if(tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.PartsUnderCursor.Length > 0)
                {
                    MouseButtonDownIS = true;
                    assemblyPart = tempobj.GetComponent<ObjectAssemblyBuilderInstance>().ActivePartTracker.ClosestPartToCursor.Key;
                    SelectedObject = GameObject.Find("OAB(Clone)");
                    showPartMenuUI = true;
                    FillPartWindow();
                    manager.Set("BetterPartsManager", showPartMenuUI);
                }
                else if (pinned == false)
                {
                    showPartMenuUI = false;
                    manager.Set("BetterPartsManager", showPartMenuUI);
                }
            }
            

        }
    }
    public void CreateNewOAB()
    {
        OABProvider OAB = new OABProvider(GameManager.Instance.Game);
        
    }

    private void PartChangedUnderMouse(MessageCenterMessage message)
    {
        partUnderMouseChanged = message as PartUnderMouseChanged;
        Debug.Log("Part selected");

    }

    void FillPartWindow (int WindowID = 0) //will be used in future versions for multi window support
    {
        menuHeight = 15;
        int fontsizeLabel = 8;
        int fontsizeButton = 8;
        int buttonHeight = 9;
        int gapSize = 1;
        int borderRadius = 4;
        Button GenerateButton(string name = "", string text = "")
        {

            Button GeneratedButton = Element.Button(name, text);
            GeneratedButton.style.paddingTop = gapSize;
            GeneratedButton.style.paddingBottom = gapSize;
            GeneratedButton.style.height = buttonHeight;
            GeneratedButton.style.fontSize = fontsizeButton;
            GeneratedButton.style.marginTop = gapSize + 3.9f;
            GeneratedButton.style.borderBottomLeftRadius = borderRadius;
            GeneratedButton.style.borderBottomRightRadius = borderRadius;
            GeneratedButton.style.borderTopLeftRadius = borderRadius;
            GeneratedButton.style.borderTopRightRadius = borderRadius;
            return GeneratedButton;
        }
        Label GenerateLabel(string name = "", string text = "")
        {

            Label GeneratedLabel = Element.Label(name, text);
            GeneratedLabel.style.fontSize = fontsizeLabel;
            GeneratedLabel.style.marginTop = gapSize;
            GeneratedLabel.style.paddingTop = gapSize;
            GeneratedLabel.style.paddingBottom = gapSize;
            return GeneratedLabel;
        }
        GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
        DictionaryValueList<Type, IPartModule> Modules = new DictionaryValueList<Type, IPartModule>();
        VisualElement BetterPartsManagerUI = manager.Get("BetterPartsManager").rootVisualElement.Q<VisualElement>("BetterPartsManagerUI");
        VisualElement BetterPartsManagerUI_Labels = BetterPartsManagerUI.Q<VisualElement>("BetterPartsManagerUI_Labels");
        VisualElement BetterPartsManagerUI_Options = BetterPartsManagerUI.Q<VisualElement>("BetterPartsManagerUI_Options");

        if (gameStateConfiguration.IsFlightMode)
        {
            BetterPartsManagerUI.Q<Label>("BetterPartsManagerUI_TitleBar_Title").text = $"{SelectedObject.GetComponent<SimulationObjectView>().Part.GetDisplayName()}";
            Modules = SelectedObject.GetComponent<SimulationObjectView>().Part.Modules;
        }
        if (gameStateConfiguration.IsObjectAssembly)
        {
            BetterPartsManagerUI.Q<Label>("BetterPartsManagerUI_TitleBar_Title").text = $"{assemblyPart.Name}";
            Modules = assemblyPart.Modules;
        }
        int ModuleID = 0;
        foreach (PartBehaviourModule module in Modules.Values)
        {
            try
            {
                switch (module.GetModuleDisplayName())
                {
                    case "Command Module":
                        Module_Command module_Command = (Module_Command)Modules.Values.ToArray()[ModuleID];
                        Label BetterPartsManagerUI_Labels_ControlOrientation = GenerateLabel("BetterPartsManagerUI_Labels_ControlOrientation", "Control Orientation");
                        BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_ControlOrientation);
                        Data_Command data_Command = new Data_Command();
                        module.DataModules.TryGetByType<Data_Command>(out data_Command);
                        
                        var key = data_Command.ControlPoints.Values.Where(pair => pair.Id == data_Command.activeControlName.GetValue()).Select(pair => pair).FirstOrDefault();
                        int index = data_Command.ControlPoints.IndexOf(key);

                        string NextControlPoint = "";
                        if(data_Command.ControlPoints.Keys.Count == index + 1)
                        {
                            NextControlPoint = data_Command.ControlPoints.Values.First().Id;
                        }
                        else
                        {
                            NextControlPoint = data_Command.ControlPoints.Values.ElementAt(index + 1).Id;
                        }
                        Button BetterPartsManagerUI_Options_ControlOrientation = GenerateButton("BetterPartsManagerUI_Options_ControlOrientation", NextControlPoint);
                        BetterPartsManagerUI_Options_ControlOrientation.clickable = new Clickable(() =>
                        {
                            module_Command.SetControlPoint(NextControlPoint, true);
                        });
                        menuHeight += 15;
                        break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"{e.Message}|{e.InnerException}|{e.Source}|{e.StackTrace}|2");
            }
        }
        BetterPartsManagerUI.style.height = menuHeight;
        BetterPartsManagerUI_Labels.style.height = menuHeight - 15;
        BetterPartsManagerUI_Options.style.height = menuHeight - 15;
    }
}