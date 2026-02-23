using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;
using UImGui;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

// Jess @ 27/01/2026
// <summary>
// IMGUI Debug Window opened with F1 (can be changed in inspector) which allows quick access to game variables and functions.
// Commands can be registered in other scripts using RegisterExternalCommand.
// This is done by calling: DebugWindow.Instance.RegisterExternalCommand(string command_name, string command_description, Action<string[]> command_action);
// Ensure that the method which calls this runs in start and not awake to ensure the DebugWindow instance is initialized first. Otherwise the command will receive a null reference error.
// Adding a new section to the debug window requires modifying this script. Follow the existing structure for adding new sections or ask me and ill implement it.
// </summary>

namespace UImGui
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private KeyCode toggle_key = KeyCode.F1;
        [SerializeField] protected StressTest stresstest;

        private int scrap_input = 0;
        private int cheese_input = 0;
        private string console_command_input = "";

        private List<string> console_log = new List<string>();

        private Dictionary<string, (Action<string[]> action, string description)> command_registry;

        public static DebugWindow Instance { get; private set; }

        private bool isDebugWindow = false;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            else
            {
                Destroy(gameObject);
            }

            command_registry = new Dictionary<string, (Action<string[]> action, string description)>();
            RegisterCommands();
            UImGuiUtility.Layout += DrawImGui;
            LogToConsole("Game Started");
        }

        private void OnDestroy()
        {
            UImGuiUtility.Layout -= DrawImGui;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggle_key))
            {
                isDebugWindow = !isDebugWindow;
            }
        }

        private void DrawImGui(UImGui uImGui)
        {
            if (!isDebugWindow) return;

            ImGui.Begin("Mousechitect Dev Window");

            if (ImGui.BeginTabBar("Main Tabs"))
            {
                if (ImGui.BeginTabItem("Game Managers"))
                {
                    DrawSaveSection();
                    ImGui.Separator();
                    DrawVariablesSection();
                    ImGui.Separator();
                    DrawCameraSection();
                    ImGui.Separator();
                    DrawPopulationTab();
                    ImGui.Separator();
                    DrawMoraleTab();
                    ImGui.Separator();
                    DrawAudioSection();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Buildings"))
                {
                    DrawBuildingsLogisticsTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Console"))
                {
                    DrawConsoleTab();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            ImGui.End();
        }

        private void DrawSaveSection()
        {
            if (ImGui.CollapsingHeader("Save/Load Manager"))
            {
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 1f, 1f), "File:" + SaveLoadManager.Instance.save_file_name);

                if (ImGui.Button("Force Save Game"))
                {
                    SaveLoadManager.Instance.SaveGame();
                }

                ImGui.SameLine();

                if (ImGui.Button("Force Load Game"))
                {
                    SaveLoadManager.Instance.LoadGame();
                }

                if (ImGui.Button("Open Save Location"))
                {
                    Application.OpenURL(Application.persistentDataPath);
                }
            }
        }

        private void DrawVariablesSection()
        {
            if (ImGui.CollapsingHeader("Resource Manager"))
            {
                ImGui.Text($"Total milk in system: {MilkManager.Instance.GetTotalMilk()}");
                ImGui.Text($"Current Scrap: {ResourceManager.instance.Scrap}");
                ImGui.Text($"Current Cheese: {ResourceManager.instance.Total_cheese}");

                ImGui.Separator();

                ImGui.InputInt("Scrap Amount", ref scrap_input);

                if (ImGui.Button("Add Resources"))
                {
                    ResourceManager.instance.AddResources(scrap_input);
                }

                ImGui.SameLine();

                if (ImGui.Button("Spend Resources"))
                {
                    ResourceManager.instance.SpendResources(scrap_input);
                }

                if (ImGui.CollapsingHeader($"Cheese"))
                {
                    ImGui.InputInt("Cheese Amount", ref cheese_input);

                    foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
                    {
                        if (ImGui.Button($"Add {c}"))
                        {
                            ResourceManager.instance.AddResources(c, cheese_input);
                        }

                        ImGui.SameLine();

                        if (ImGui.Button($"Subtract {c}"))
                        {
                            ResourceManager.instance.SpendResources(c, cheese_input);
                        }
                    }
                }
            }
        }

        private void DrawAudioSection()
        {
            if (ImGui.CollapsingHeader("Audio Manager"))
            {
                if (ImGui.Button("Play Sample Track 1") && !AudioHandler.instance.is_music_playing)
                {
                    AudioHandler.instance.music_track_1.Play();
                }

                if (ImGui.Button("Play Sample Track 2") && !AudioHandler.instance.is_music_playing)
                {
                    AudioHandler.instance.music_track_2.Play();
                }

                if (ImGui.Button("Play Sample Track 3") && !AudioHandler.instance.is_music_playing)
                {
                    AudioHandler.instance.music_track_3.Play();
                }
                ImGui.Separator();
                if (!AudioHandler.instance.is_music_playing)
                {
                    ImGui.Text("No music is currently playing");
                }
                else
                {
                    ImGui.Text("Music is currently playing");
                }

                if (ImGui.Button("Stop Music"))
                {
                    AudioHandler.instance.music_track_1.Stop();
                    AudioHandler.instance.music_track_2.Stop();
                    AudioHandler.instance.music_track_3.Stop();
                }
            }
        }

        private void DrawCameraSection()
        {
            if (ImGui.CollapsingHeader("Camera Manager"))
            {
                ImGui.Text($"Camera Speed ");
                ImGui.SliderFloat($"Camera Sensitivity", ref PerspectiveCameraController.Instance.orbit_speed, 0.1f, 10f);
                ImGui.SliderFloat($"WASD Movement Speed", ref PerspectiveCameraController.Instance.wasd_pan_speed, 0.1f, 1f);
                ImGui.SliderFloat($"Edge Movement Speed", ref PerspectiveCameraController.Instance.edge_pan_speed, 0.1f, 1f);
                ImGui.SliderFloat($"Border Size", ref PerspectiveCameraController.Instance.edge_pan_size, 0.1f, 1f);
                ImGui.Checkbox($"Enable Edge Pan", ref PerspectiveCameraController.Instance.is_edge_pan_enabled);

            }
        }

        private void DrawConsoleTab()
        {
            ImGui.BeginChild("ScrollingRegion", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()), ImGuiChildFlags.Border, ImGuiWindowFlags.HorizontalScrollbar);
            foreach (var log in console_log)
            {
                if (log.Contains("Help Command:"))
                {
                    string command_name = log.Replace("Help Command:", "");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);
                    ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f), command_name);
                }
                else if (log.Contains("Description:- "))
                {
                    string description = log.Replace("Description:- ", "");
                    ImGui.Indent(40f);
                    ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1f), description);
                    ImGui.Unindent(40f);
                    ImGui.Spacing();
                }
                else
                {
                    ImGui.TextUnformatted(log);
                }
            }

            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }
            ImGui.EndChild();

            bool isReclaimFocus = false;

            if (ImGui.InputText("Command", ref console_command_input, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                ExecuteConsoleCommand(console_command_input);
                console_command_input = "";
                isReclaimFocus = true;
            }

            ImGui.SetItemDefaultFocus();
            if (isReclaimFocus)
            {
                ImGui.SetKeyboardFocusHere(-1);
            }
        }

        private void Upgrade(ParentBuilding building)
        {
            if (ImGui.Button("Upgrade building"))
            {
                building.UpdradeFactory();
            }
            ImGui.SameLine();
            ImGui.Text($"Tier: {building.Tier}");
        }
        private void MilkLogic(IMilkContainer container)
        {
            int current = container.CURRENT_MILK_AMOUNT;
            if (ImGui.SliderInt("stored milk", ref current, 0, container.MAX_MILK_CAPACITY))
            {
                container.CURRENT_MILK_AMOUNT = current;
            }

            int max = container.MAX_MILK_CAPACITY;
            if (ImGui.InputInt("Max Capacity", ref max))
            {
                container.MAX_MILK_CAPACITY = max;
            }
        }

        private void DrawBuildingsLogisticsTab()
        {
            ParentBuilding[] buildings = FindObjectsOfType(typeof(ParentBuilding)) as ParentBuilding[];

            ImGui.Text($"Total buildings in system: {buildings.Length}");

            ImGui.Separator();

            foreach (var building in buildings)
            {
                ImGui.PushID(building.GetHashCode());

                string building_type = string.Empty;

                switch (building.Building_type)
                {
                    case BuildingType.residental:
                        building_type = "[Residental]";
                        break;
                    case BuildingType.factory:
                        building_type = "[Factory]";
                        break;
                    case BuildingType.market:
                        building_type = "[Commercial]";
                        break;
                    case BuildingType.research:
                        building_type = "[Research]";
                        break;
                    case BuildingType.tank:
                        building_type = "[Tank]";
                        break;
                    case BuildingType.collector:
                        building_type = "[Collector]";
                        break;
                }

                if (ImGui.CollapsingHeader($"{building_type} {building.name}"))
                {
                    ImGui.Text($"Mice in building: {building.Mouse_occupants.Count}");

                    if (ImGui.Button("Kick mouse"))
                    {
                        if(building.Mouse_occupants.Count > 0)
                            building.MouseLeave(building.Mouse_occupants[0]);
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Move a mouse to building"))
                    {
                        MouseTemp mouse = FindObjectOfType(typeof(MouseTemp), true) as MouseTemp;

                        if (building != null && mouse != null)
                        {
                            if (mouse.Home != null)
                                mouse.Home.MouseLeave(mouse);

                            if(mouse.Moving == false)
                                stresstest.MoveMouse(mouse, building);
                        }
                    }

                    ImGui.Separator();

                    switch (building.Building_type)
                    {
                        case BuildingType.residental:
                            ResidentialBuilding home = (ResidentialBuilding)building;
                            Upgrade(home);
                            ImGui.Text($"Building quality: {home.Quality}");
                            break;
                        case BuildingType.factory:
                            FactoryBuilding factory = (FactoryBuilding)building;
                            Upgrade(factory);
                            MilkLogic(factory);

                            if (ImGui.CollapsingHeader($"Cheese")) 
                            {
                                ImGui.Text($"Current cheese: {factory.Cheese_type}");

                                CheeseTypes[] cheeses = factory.GetAvalibleCheese();

                                foreach (CheeseTypes cheese in cheeses)
                                {
                                    if (ImGui.Button($"Select cheese: {cheese}"))
                                        factory.SelectCheese(cheese);
                                    ImGui.SameLine();
                                }

                                ImGui.NewLine();
                                ImGui.Text($"Producing cheese: {factory.Produce_cheese}");

                                if (ImGui.Button("Turn factory On/Off"))
                                    factory.ProduceCheeseSwitch();
                            }
                            break;
                        case BuildingType.market:
                            CommercialBuilding market = (CommercialBuilding)building;
                            foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
                            {
                                ImGui.Text($"{c} popularity: {market.Cheese_popularity[(int)c]}");
                            }
                            break;
                        case BuildingType.tank:
                            MilkTank tank = (MilkTank)building;
                            Upgrade(tank);
                            MilkLogic(tank);
                            break;
                        case BuildingType.collector:
                            MilkCollector collector = (MilkCollector)building;
                            MilkLogic(collector);

                            float rate = collector.production_interval;
                            if (ImGui.DragFloat("Production Interval", ref rate, 0.1f, 0.5f, 60.0f))
                            {
                                collector.production_interval = rate;
                            }

                            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), collector.GetStatus());
                            break;
                    }
                }
                ImGui.PopID();
            }
        }

        private void DrawPopulationTab()
        {
            if (ImGui.CollapsingHeader("Population Manager"))
            {
                int population = PopulationManager.instance.current_population;
                float arrival_interval = PopulationManager.instance.base_arrival_interval;
                if (ImGui.SliderInt("Current Population", ref population, 0, 100))
                {
                    PopulationManager.instance.current_population = population;
                }

                if (ImGui.DragFloat("Base Arrival Interval", ref arrival_interval, 0.1f, 1f, 60f))
                {
                    PopulationManager.instance.base_arrival_interval = arrival_interval;
                }

                ImGui.Separator();

                ImGui.Text($"Total Housing Capacity: {PopulationManager.instance.total_housing_capacity}");
                ImGui.Text($"Current Visual Capacity: {PopulationManager.instance.current_visual_capacity}");
            }
        }

        private void DrawMoraleTab()
        {
            
            bool is_overidden = MoraleSystem.Instance.is_debug_override;

            
            float morale = MoraleSystem.Instance.GetMoraleScore();
            float productivity = MoraleSystem.Instance.GetProductivityModifier();
            float retention = MoraleSystem.Instance.GetRetentionModifier();
            float arrival = MoraleSystem.Instance.GetArrivalChanceModifier();

            if (ImGui.CollapsingHeader("Morale Debug"))
            {
                if (ImGui.Checkbox("Override Morale Values", ref is_overidden))
                {
                    MoraleSystem.Instance.is_debug_override = is_overidden;
                }

                ImGui.Separator();

                if (ImGui.SliderFloat("Global Morale", ref morale, -1f, 1f))
                {
                    MoraleSystem.Instance.SetMoraleScore(morale);
                }

                if (ImGui.SliderFloat("Productivity Modifier", ref productivity, 0.5f, 2f))
                {
                    MoraleSystem.Instance.SetProductivityModifier(productivity);
                }

                if (ImGui.SliderFloat("Retention Modifier", ref retention, 0.5f, 2f))
                {
                    MoraleSystem.Instance.SetRetentionModifier(retention);
                }

                if (ImGui.SliderFloat("Arrival Chance Modifier", ref arrival, 0.5f, 2f))
                {
                    MoraleSystem.Instance.SetArrivalChanceModifier(arrival);
                }
            }
        }

        // This is to register default commands that arent directly tied to another function
        private void RegisterCommands()
        {
            command_registry.Add("quit", ( (args) => Application.Quit(), " - Quits the game"));
            command_registry.Add("clear", ( (args) => console_log.Clear(), " - Clears the console screen"));
            command_registry.Add("help", ( (args) =>
            {
                LogToConsole("Available Commands:");
                foreach (var command in command_registry)
                {
                    LogToConsole($"Help Command:{command.Key}", false);
                    LogToConsole($"Description:- {command.Value.description}", false);
                }
            }, " - Lists all available commands"));
        }

        public void RegisterExternalCommand(string command_name, string command_description, Action<string[]> command_action)
        {
            command_name = command_name.ToLower();
            // prevents duplicate commands
            if (!command_registry.ContainsKey(command_name))
            {
                command_registry.Add(command_name, (command_action, command_description));
            }
        }

        private void ExecuteConsoleCommand(string input)
        {
            // string manipulation to separate command and args
            string[] parts = input.Trim().Split(' ');
            string command_name = parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts[1..] : new string[0];

            LogToConsole($"> {input}");

            if (command_registry.ContainsKey(command_name))
            {
                command_registry[command_name].action.Invoke(args);
            }
            else
            {
                LogToConsole($"Unknown command: {command_name}. Type 'help' for a list of available commands.");
            }
        }

        public static void LogToConsole(string message, bool is_timestamped = true)
        {
            string final_message = is_timestamped ? $"[{System.DateTime.Now:HH:mm:ss}] {message}" : message;
            Instance.console_log.Add(final_message);

            if (Instance?.console_log.Count > 50)
            {
                Instance.console_log.RemoveAt(0);
            }
        }
    }
}

