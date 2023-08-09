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
using UnityEngine.UIElements;
using UitkForKsp2.API;
using DragManipulator = BetterPartsManager.UI.DragManipulator;
using ShadowUtilityLIB.UI;
using Position = UnityEngine.UIElements.Position;
using Button = UnityEngine.UIElements.Button;

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
            GeneratedLabel.style.color = new StyleColor(new Color32(0, 0, 0, 255));
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
        BetterPartsManagerUI_Labels.Clear();
        BetterPartsManagerUI_Options.Clear();
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
                logger.Log(module.GetModuleDisplayName());
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
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_ControlOrientation);
                        menuHeight += 15;
                        break;
                    case "Lit Part":
                        break;
                    case "Light":
                        Label BetterPartsManagerUI_Labels_Color = GenerateLabel("BetterPartsManagerUI_Labels_Color", "Color");
                        BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_Color);
                        Data_Light data_Light = new Data_Light();
                        module.DataModules.TryGetByType<Data_Light>(out data_Light);

                        Slider BetterPartsManagerUI_Options_LightColor_Slider_Red = Element.Slider("BetterPartsManagerUI_Options_LightColor_Slider_Red", 0f, 1f, data_Light.lightColorR.GetValue());
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.style.height = buttonHeight;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.style.fontSize = fontsizeButton;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.style.marginTop = gapSize;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragElement.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragElement.style.marginTop = -2;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragElement.style.width = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragElement.style.height = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragBorderElement.style.backgroundColor = new StyleColor(new Color32(50, 50, 50, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragBorderElement.style.height = buttonHeight;
                        menuHeight += 35;
                        Slider BetterPartsManagerUI_Options_LightColor_Slider_Green = Element.Slider("BetterPartsManagerUI_Options_LightColor_Slider_Green", 0f, 1f, data_Light.lightColorR.GetValue());
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.style.height = buttonHeight;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.style.fontSize = fontsizeButton;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.style.marginTop = gapSize;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragElement.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragElement.style.marginTop = -2;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragElement.style.width = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragElement.style.height = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragBorderElement.style.backgroundColor = new StyleColor(new Color32(50, 50, 50, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragBorderElement.style.height = buttonHeight;
                        menuHeight += 35;
                        Slider BetterPartsManagerUI_Options_LightColor_Slider_Blue = Element.Slider("BetterPartsManagerUI_Options_LightColor_Slider_Blue", 0f, 1f, data_Light.lightColorR.GetValue());
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.style.height = buttonHeight;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.style.fontSize = fontsizeButton;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.style.marginTop = gapSize;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragElement.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragElement.style.marginTop = -2;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragElement.style.width = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragElement.style.height = 15;
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragBorderElement.style.backgroundColor = new StyleColor(new Color32(50, 50, 50, 255));
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragBorderElement.style.height = buttonHeight;
                        menuHeight += 35;
                        BetterPartsManagerUI_Options_LightColor_Slider_Red.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));

                        BetterPartsManagerUI_Options_LightColor_Slider_Red.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));

                        BetterPartsManagerUI_Options_LightColor_Slider_Red.RegisterValueChangedCallback((evt) =>
                        {
                            UpdateColor();
                        });
                        BetterPartsManagerUI_Options_LightColor_Slider_Green.RegisterValueChangedCallback((evt) =>
                        {
                            UpdateColor();
                        });
                        BetterPartsManagerUI_Options_LightColor_Slider_Blue.RegisterValueChangedCallback((evt) =>
                        {
                            UpdateColor();
                        });

                        void UpdateColor()
                        {
                            
                            BetterPartsManagerUI_Options_LightColor_Slider_Red.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                            BetterPartsManagerUI_Options_LightColor_Slider_Green.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                            BetterPartsManagerUI_Options_LightColor_Slider_Blue.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));

                            BetterPartsManagerUI_Options_LightColor_Slider_Red.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                            BetterPartsManagerUI_Options_LightColor_Slider_Green.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));
                            BetterPartsManagerUI_Options_LightColor_Slider_Blue.dragContainer.style.backgroundColor = new StyleColor(new Color(BetterPartsManagerUI_Options_LightColor_Slider_Red.value, BetterPartsManagerUI_Options_LightColor_Slider_Green.value, BetterPartsManagerUI_Options_LightColor_Slider_Blue.value));

                            data_Light.lightColorR.SetValue(BetterPartsManagerUI_Options_LightColor_Slider_Red.value);
                            data_Light.lightColorG.SetValue(BetterPartsManagerUI_Options_LightColor_Slider_Green.value);
                            data_Light.lightColorB.SetValue(BetterPartsManagerUI_Options_LightColor_Slider_Blue.value);
                        }
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightColor_Slider_Red);
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightColor_Slider_Green);
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightColor_Slider_Blue);

                        Label BetterPartsManagerUI_Labels_blinkRate = GenerateLabel("BetterPartsManagerUI_Labels_blinkRate", "blink Rate");
                        BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_blinkRate);
                        BetterPartsManagerUI_Labels_blinkRate.style.marginTop = gapSize + 80;

                        Slider BetterPartsManagerUI_Options_LightBlinkRate = Element.Slider("BetterPartsManagerUI_Options_LightBlinkRate", 0f, 100f, data_Light.blinkRate.GetValue());
                        BetterPartsManagerUI_Options_LightBlinkRate.style.height = buttonHeight;
                        BetterPartsManagerUI_Options_LightBlinkRate.style.fontSize = fontsizeButton;
                        BetterPartsManagerUI_Options_LightBlinkRate.style.marginTop = gapSize;
                        BetterPartsManagerUI_Options_LightBlinkRate.dragElement.style.marginTop = -2;
                        BetterPartsManagerUI_Options_LightBlinkRate.dragElement.style.width = 15;
                        BetterPartsManagerUI_Options_LightBlinkRate.dragElement.style.height = 15;
                        BetterPartsManagerUI_Options_LightBlinkRate.dragBorderElement.style.height = buttonHeight;
                        BetterPartsManagerUI_Options_LightBlinkRate.RegisterValueChangedCallback((evt) =>
                        {
                            data_Light.blinkRate.SetValue(BetterPartsManagerUI_Options_LightBlinkRate.value);
                        });
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightBlinkRate);
                        menuHeight += 35;
                        if (data_Light.canRotate)
                        {
                            Label BetterPartsManagerUI_Labels_rotationAngle = GenerateLabel("BetterPartsManagerUI_Labels_rotationAngle", "rotation Angle");
                            BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_rotationAngle);
                            BetterPartsManagerUI_Labels_rotationAngle.style.marginTop = gapSize;

                            Slider BetterPartsManagerUI_Options_LightrotationAngle = Element.Slider("BetterPartsManagerUI_Options_LightrotationAngle", 0f, 360f, data_Light.rotationAngle.GetValue());
                            BetterPartsManagerUI_Options_LightrotationAngle.style.height = buttonHeight;
                            BetterPartsManagerUI_Options_LightrotationAngle.style.fontSize = fontsizeButton;
                            BetterPartsManagerUI_Options_LightrotationAngle.style.marginTop = gapSize;
                            BetterPartsManagerUI_Options_LightrotationAngle.dragElement.style.marginTop = -2;
                            BetterPartsManagerUI_Options_LightrotationAngle.dragElement.style.width = 15;
                            BetterPartsManagerUI_Options_LightrotationAngle.dragElement.style.height = 15;
                            BetterPartsManagerUI_Options_LightrotationAngle.dragBorderElement.style.height = buttonHeight;
                            BetterPartsManagerUI_Options_LightrotationAngle.RegisterValueChangedCallback((evt) =>
                            {
                                data_Light.rotationAngle.SetValue(BetterPartsManagerUI_Options_LightrotationAngle.value);
                            });
                            BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightrotationAngle);
                            menuHeight += 25;
                        }
                        if (data_Light.canPitch)
                        {
                            Label BetterPartsManagerUI_Labels_pitchAngle = GenerateLabel("BetterPartsManagerUI_Labels_pitchAngle", "pitch Angle");
                            BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_pitchAngle);
                            BetterPartsManagerUI_Labels_pitchAngle.style.marginTop = gapSize;

                            Slider BetterPartsManagerUI_Options_LightpitchAngle = Element.Slider("BetterPartsManagerUI_Options_LightpitchAngle", 0f, 360f, data_Light.pitchAngle.GetValue());
                            BetterPartsManagerUI_Options_LightpitchAngle.style.height = buttonHeight;
                            BetterPartsManagerUI_Options_LightpitchAngle.style.fontSize = fontsizeButton;
                            BetterPartsManagerUI_Options_LightpitchAngle.style.marginTop = gapSize;
                            BetterPartsManagerUI_Options_LightpitchAngle.dragElement.style.marginTop = -2;
                            BetterPartsManagerUI_Options_LightpitchAngle.dragElement.style.width = 15;
                            BetterPartsManagerUI_Options_LightpitchAngle.dragElement.style.height = 15;
                            BetterPartsManagerUI_Options_LightpitchAngle.dragBorderElement.style.height = buttonHeight;
                            BetterPartsManagerUI_Options_LightpitchAngle.RegisterValueChangedCallback((evt) =>
                            {
                                data_Light.pitchAngle.SetValue(BetterPartsManagerUI_Options_LightpitchAngle.value);
                            });
                            BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_LightpitchAngle);
                        }
                        break;
                    case "Reaction Wheel":
                        Data_ReactionWheel data_ReactionWheel = new Data_ReactionWheel();
                        module.DataModules.TryGetByType<Data_ReactionWheel>(out data_ReactionWheel);

                        Label BetterPartsManagerUI_Labels_WheelActuatorMode = GenerateLabel("BetterPartsManagerUI_Labels_WheelActuatorMode", "Torque Mode");
                        BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_WheelActuatorMode);
                        BetterPartsManagerUI_Labels_WheelActuatorMode.style.marginTop = gapSize;

                        var modeLst = Enum.GetNames(typeof(Data_ReactionWheel.ActuatorModes));

                        var currentModeInx = Array.IndexOf(modeLst, data_ReactionWheel.WheelActuatorMode.GetValue());

                        if (modeLst.Length == currentModeInx)
                        {
                            currentModeInx = 0;
                        }
                        else
                        {
                            currentModeInx++;
                        }
                        Button BetterPartsManagerUI_Options_WheelActuatorMode = GenerateButton("BetterPartsManagerUI_Options_WheelActuatorMode", modeLst[currentModeInx]);
                        BetterPartsManagerUI_Options_WheelActuatorMode.clickable = new Clickable(() =>
                        {
                            BetterPartsManagerUI_Options_WheelActuatorMode.text = modeLst[currentModeInx];
                            data_ReactionWheel.WheelActuatorMode.SetValue((Data_ReactionWheel.ActuatorModes)Enum.Parse(typeof(Data_ReactionWheel.ActuatorModes), modeLst[currentModeInx]));
                        });
                        BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_WheelActuatorMode);
                        menuHeight += 25;
                        try
                        {
                            Label BetterPartsManagerUI_Labels_WheelAuthority = GenerateLabel("BetterPartsManagerUI_Labels_WheelAuthority", "Wheel Authority");
                            BetterPartsManagerUI_Labels.Add(BetterPartsManagerUI_Labels_WheelAuthority);
                            BetterPartsManagerUI_Labels_WheelActuatorMode.style.marginTop = gapSize;
                            Slider BetterPartsManagerUI_Options_WheelAuthority = Element.Slider("BetterPartsManagerUI_Options_LightColor_Slider_Red", 0f, 1f, (float)data_ReactionWheel.WheelAuthority.GetObject());
                            BetterPartsManagerUI_Options_WheelAuthority.style.height = buttonHeight;
                            BetterPartsManagerUI_Options_WheelAuthority.style.fontSize = fontsizeButton;
                            BetterPartsManagerUI_Options_WheelAuthority.style.marginTop = gapSize;
                            BetterPartsManagerUI_Options_WheelAuthority.dragElement.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 255));
                            BetterPartsManagerUI_Options_WheelAuthority.dragElement.style.marginTop = -2;
                            BetterPartsManagerUI_Options_WheelAuthority.dragElement.style.width = 15;
                            BetterPartsManagerUI_Options_WheelAuthority.dragElement.style.height = 15;
                            BetterPartsManagerUI_Options_WheelAuthority.dragBorderElement.style.backgroundColor = new StyleColor(new Color32(0, 0, 0, 0));
                            BetterPartsManagerUI_Options_WheelAuthority.dragBorderElement.style.height = buttonHeight;
                            
                            BetterPartsManagerUI_Options_WheelAuthority.RegisterValueChangedCallback((evt) =>
                            {
                                data_ReactionWheel.WheelAuthority.SetValue(BetterPartsManagerUI_Options_WheelAuthority.value);
                            });
                            menuHeight += 35;
                            BetterPartsManagerUI_Options.Add(BetterPartsManagerUI_Options_WheelAuthority);
                        }
                        catch{}

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