using JuniorProject.Backend.Agents;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using JuniorProject.Backend;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Frontend.Windows
{
    public partial class DebugWindow : Window
    {
        private static DebugWindow? _instance;
        static DebugWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DebugWindow();
                    _instance.Closing += _instance_Closing;
                }
                return _instance;
            }
        }

        private static void _instance_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
        }

        List<string> usedCommands = new List<string>();
        Dictionary<string, Unit> total_units = new Dictionary<string, Unit>();
        int current_command;

        public DebugWindow()
        {
            InitializeComponent();
            Input.Focus();
        }

        public static void ShowWindow(Simulation simulation)
        {
            Instance.Show();
            Instance.KeyDown += Instance.KeyPressed;
            simulation.Unloaded += (object sender, RoutedEventArgs e) =>
            {
                Instance.Close();
            };
            Instance.total_units = ClientCommunicator.GetData<World>("World").GetAllUnits();
        }

        public static readonly Regex clearRegex = new Regex("^clear\\ *$");
        public static readonly Regex helpRegex = new Regex("^help\\ *$");

        public static readonly Regex spawnUnitRegex = new Regex("^spawnUnit\\ *\\([a-zA-z]+,\\ *[a-zA-z]+,\\ *[0-9]+,\\ *[0-9]+\\,\\ *[a-zA-z]+\\)\\ *$");
        public static readonly Regex deleteUnitRegex = new Regex("^deleteUnit\\ *\\([a-zA-z]+\\)\\ *$");
        public static readonly Regex printUnitsRegex = new Regex("^printUnits\\(\\)\\ *$");
        public static readonly Regex printPossibleSpritesRegex = new Regex("^printPossibleSprites\\(\\)\\ *$");

        public static readonly Regex deleteTileCover = new Regex("^deleteTileCover\\ *\\([0-9]+,\\ *[0-9]+\\)\\ *$");
        public static readonly Regex modifyTileCover = new Regex("^modifyTileCover\\ *\\([a-zA-z]+,\\ *[0-9]+,\\ *[0-9]+\\)\\ *$");
        public static readonly Regex expandTerritory = new Regex("^expandTerritory\\ *\\([a-zA-z]+\\)\\ *$");

        public static readonly Regex getSeed = new Regex("^seed$");

        public static readonly Regex stringParam = new Regex("\\([a-zA-z]+[0-9]*|,\\ *[a-zA-z]+[0-9]*");
        public static readonly Regex stringInstance = new Regex("[a-zA-z]+[0-9]*");
        public static readonly Regex intParam = new Regex("\\([0-9]+|,\\ *[0-9]+");
        public static readonly Regex intInstance = new Regex("[0-9]+");

        public static readonly Regex identifier = new Regex("^[a-zA-z]+.");
        public static readonly Regex function = new Regex("\\.[a-zA-Z]+\\(");

		private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
		{
            if(e.Key == Key.Up && usedCommands.Count != 0)
            {
				current_command--;
				if (current_command < 0) return;
				Input.Text = usedCommands[current_command];
                return;
			}
            if(e.Key == Key.Down && usedCommands.Count != 0)
            {
				current_command++;
				if (current_command >= usedCommands.Count)
				{
					current_command = usedCommands.Count;
					Input.Text = "";
					return;
				}
				Input.Text = usedCommands[current_command];
                return;
			}
		}

		void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
				current_command = usedCommands.Count;
                return;
            }

            if (spawnUnitRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                string unitType = stringInstance.Match(matches[0].Value).Value;
                string unitTeam = stringInstance.Match(matches[1].Value).Value;
                string unitName = stringInstance.Match(matches[2].Value).Value;
                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);
                World w = ClientCommunicator.GetData<World>("World");
                if (!w.nations.ContainsKey(unitTeam)) return;
                Unit u = new Unit(unitType, unitName, w.nations[unitTeam], w.map, w.map.getTile(new Vector2Int(x, y)));
                w.nations[unitTeam].AddUnit(u);
                total_units.Add(unitName, u);
                Console.Text += $"Unit spawned at {x},{y} of type [{unitType}] with name [{unitName}]\n";
				finishCommand();
				return;
            }

            if (printPossibleSpritesRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                Console.Text += "\nPossible sprites for units to spawn with: \n";
                Console.Text += "Take the team name, and combine it with a unit name\n\n";
                Console.Text += "Team Names: 'Yellow', 'Green', 'Red'\n";
                Console.Text += "\n";
                Console.Text += "Unit Types: 'Soldier', 'Archer'\n\n";

				finishCommand();
				return;
            }

            if (deleteUnitRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                string unitName = stringInstance.Match(matches[0].Value).Value;
                total_units[unitName].nation.RemoveUnit(total_units[unitName]);
                total_units.Remove(unitName);
                Console.Text += $"Attempted to remove unit with name {unitName}\n";

				finishCommand();
				return;
            }

            if (modifyTileCover.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                string team = stringInstance.Match(matches[0].Value).Value;

                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);

                World w = ClientCommunicator.GetData<World>("World");
				w.nations[team].AddTerritory(w.map.getTile(new Vector2Int(x,y)));

				finishCommand();
				return;
            }

            if (deleteTileCover.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);

				World w = ClientCommunicator.GetData<World>("World");
                w.map.getTile(new Vector2Int(x, y)).Owner = null;
				finishCommand();
				return;
            }

            if (expandTerritory.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                string team = stringInstance.Match(matches[0].Value).Value;

                World w = ClientCommunicator.GetData<World>("World");
                foreach (TileMap.Tile t in w.nations[team].GetBorderingTiles()) {
                    w.nations[team].AddTerritory(t);
                }

                finishCommand();
                return;
            }

            if (clearRegex.IsMatch(Input.Text))
            {
                Console.Text = "";
				finishCommand();
				return;
            }

            if (helpRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                Console.Text += "\nPossible commands (be mindful of Regex possibly not detecting input): \n\n";
                Console.Text += "spawnUnit(<Unit Type>, <Team>, <gridX>, <gridY>, <Name>) -> Spawns a unit\n";
                Console.Text += "deleteUnit(<Name>) -> Removes a unit\n";
                Console.Text += "printUnits() -> Prints current units\n";
                Console.Text += "\n";
                Console.Text += "printPossibleSprites() -> Prints possible sprites\n";
                Console.Text += "\n";
                Console.Text += "modifyTileCover(<Team>, <gridX>, <gridY>) -> Update tile cover team\n";
                Console.Text += "deleteTileCover(<gridX>, <gridY>) -> Removes tile cover\n";
                Console.Text += "expandTerritory(<Team>)\n";
                Console.Text += "\n";
                Console.Text += "clear -> clears console\n\n";

				finishCommand();
				return;
            }

            if(getSeed.IsMatch(Input.Text))
            {
                Console.Text += $"World Seed: {ClientCommunicator.GetData<World>("World").map.seed}\n";
                finishCommand();
            }


            if (printUnitsRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";
                Console.Text += "\nName\t\tType\t\tTeam\t\tGridPosition:\n";
                foreach (var u in total_units.Values)
                {
                    Console.Text += $"{u.name}\t\t{u.unitType.name}\t\t{u.nation.name}\t\t[{u.PosVector.X}, {u.PosVector.Y}]\n";
                }

				finishCommand();
				return;
            }

            if (identifier.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                string unit = identifier.Match(Input.Text).Value;
                unit = unit.TrimEnd('.');
                
                if (!total_units.ContainsKey(unit))
                {
                    Console.Text += $"No unit created with name {unit}\n";
					finishCommand();
					return;
                }
                string command = function.Match(Input.Text).Value;
                command = command.TrimStart('.');
                command = command.TrimEnd('(');
                switch (command)
                {
                    case "MoveTo":
                        {
                            List<Match> parameters = intParam.Matches(Input.Text).ToList();
                            int x = int.Parse(intInstance.Match(parameters[0].Value).Value);
                            int y = int.Parse(intInstance.Match(parameters[1].Value).Value);
                            World w = ClientCommunicator.GetData<World>("World");
                            total_units[unit].MoveTo(w.map.getTile(new Vector2Int(x,y)));
                            break;
                        }
                    case "Follow":
                        {
                            List<Match> parameters = stringParam.Matches(Input.Text).ToList();
                            string mob = stringInstance.Match(parameters[0].Value).Value;
                            
                            if (!total_units.ContainsKey(mob)) {
                                Console.Text += $"No mob found with name {mob} to follow\n";
                                finishCommand();
                                return;
                            }

                            World w = ClientCommunicator.GetData<World>("World");
                            total_units[unit].Follow(total_units[mob]);
                            break;
                        }
                }
                finishCommand();
            }
        }

        void finishCommand()
        {
            ClientCommunicator.GetData<World>("World").RedrawAction?.Invoke(); // Need this to see changes after calling command, otherwise you need to press 'step'
            usedCommands.Add(Input.Text);
            current_command = usedCommands.Count;
			Input.Text = "";
		}
	}
}
