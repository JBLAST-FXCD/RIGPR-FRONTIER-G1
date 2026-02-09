using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;
using UImGui;
using UnityEngine.UIElements;
using System;

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

        private int scrap_input = 0;
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
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Milk Logistics"))
                {
                    DrawMilkLogisticsTab();
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

        private void DrawMilkLogisticsTab()
        {
            if (MilkManager.Instance == null)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Milk Manager instance not found");
                return;
            }

            ImGui.Text($"Total milk in system: {MilkManager.Instance.GetTotalMilk()}");

            if (ImGui.Button("Force logistic refresh"))
            {
                MilkManager.Instance.RefreshRankings();
            }

            ImGui.Separator();

            foreach (var container in MilkManager.Instance.all_containers)
            {
                ImGui.PushID(container.GetHashCode());

                string container_type = container.IS_TANK ? "[Tank]" : "[Collector]";
                if (ImGui.CollapsingHeader($"{container_type} {container.CONTAINER_GAME_OBJECT.name}"))
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

                    if (container is MilkCollector collector)
                    {
                        float rate = collector.production_interval;
                        if (ImGui.DragFloat("Production Interval", ref rate, 0.1f, 0.5f, 60.0f))
                        {
                            collector.production_interval = rate;
                        }

                        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), collector.GetStatus());
                    }
                }
                ImGui.PopID();
            }
        }

        // This is to register default commands that arent directly tied to another function
        private void RegisterCommands()
        {
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

