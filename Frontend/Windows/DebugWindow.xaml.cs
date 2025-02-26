using JuniorProject.Backend.Agents;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using JuniorProject.Backend;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;
using JuniorProject.Backend.WorldData.Managers;

namespace JuniorProject.Frontend.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
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

        Simulation simulationPage;
        Dictionary<string, Unit> unitsCreated = new Dictionary<string, Unit>();

        private static void _instance_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
        }

        public DebugWindow()
        {
            InitializeComponent();
        }

        public static void ShowWindow(Simulation simulation)
        {
            Instance.Show();
            Instance.simulationPage = simulation;
            Instance.KeyDown += Instance.KeyPressed;
        }

        public static readonly Regex clearRegex = new Regex("^clear\\ *$");
        public static readonly Regex helpRegex = new Regex("^help\\ *$");

        public static readonly Regex spawnUnitRegex = new Regex("^spawnUnit\\ *\\([a-zA-z]+,\\ *[0-9]+,\\ *[0-9]+\\)\\ *$");
        public static readonly Regex deleteUnitRegex = new Regex("^deleteUnit\\ *\\(\\ *[0-9]+,\\ *[0-9]+\\)\\ *$");
        public static readonly Regex printUnitsRegex = new Regex("^printUnits\\(\\)\\ *$");
        public static readonly Regex printPossibleSpritesRegex = new Regex("^printPossibleSprites\\(\\)\\ *$");

        public static readonly Regex deleteTileCover = new Regex("^deleteTileCover\\ *\\([0-9]+,\\ *[0-9]+\\)\\ *$");
        public static readonly Regex modifyTileCover = new Regex("^modifyTileCover\\ *\\([a-zA-z]+,\\ *[0-9]+,\\ *[0-9]+\\)\\ *$");

        public static readonly Regex stringParam = new Regex("\\([a-zA-z]+|,\\ *[a-zA-z]+");
        public static readonly Regex stringInstance = new Regex("[a-zA-z]+");
        public static readonly Regex intParam = new Regex("\\([0-9]+|,\\ *[0-9]+");
        public static readonly Regex intInstance = new Regex("[0-9]+");

        public static readonly Regex identifier = new Regex("^[a-zA-z]+.");
        public static readonly Regex function = new Regex("\\.[a-zA-Z]+\\(");
        void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (spawnUnitRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                string sprite = stringInstance.Match(matches[0].Value).Value;

                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);
                ClientCommunicator.GetData<DrawableManager>("DrawableManager").AddUnit(new Vector2Int(x, y), sprite);
                Console.Text += $"Unit spawned at {x},{y} with sprite [{sprite}]\n";

                Input.Text = "";
                return;
            }

            if (printPossibleSpritesRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                Console.Text += "\nPossible sprites for units to spawn with: \n";
                Console.Text += "Take the team name, and prepend it to the unit name\n";
                Console.Text += "Ex: 'RedArcher'\n\n";
                Console.Text += "Team Names: 'Yellow', 'Green', 'Red'\n";
                Console.Text += "\n";
                Console.Text += "Unit Types: 'Soldier', 'Archer'\n\n";

                Input.Text = "";
                return;
            }

            if (deleteUnitRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);
                ClientCommunicator.GetData<DrawableManager>("DrawableManager").RemoveUnit(new Vector2Int(x, y));
                Console.Text += $"Attempted to remove unit with at {x}, {y}\n";

                Input.Text = "";
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

                ClientCommunicator.GetData<TileMap>("TileMap").convertTile(new Vector2Int(x, y), team);

                Input.Text = "";
                return;
            }

            if (deleteTileCover.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                List<Match> matches = stringParam.Matches(Input.Text).ToList();
                matches = intParam.Matches(Input.Text).ToList();
                int x = int.Parse(intInstance.Match(matches[0].Value).Value);
                int y = int.Parse(intInstance.Match(matches[1].Value).Value);

                ClientCommunicator.GetData<TileMap>("TileMap").convertTile(new Vector2Int(x, y), String.Empty);

                Input.Text = "";
                return;
            }

            if (clearRegex.IsMatch(Input.Text))
            {
                Console.Text = "";
                Input.Text = "";
                return;
            }

            if (helpRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                Console.Text += "\nPossible commands (be mindful of Regex possibly not detecting input): \n\n";
                Console.Text += "spawnUnit(<Sprite>, <gridX>, <gridY>) -> Spawns a unit\n";
                Console.Text += "deleteUnit(<gridX>, <gridY>) -> Removes a unit\n";
                Console.Text += "printUnits() -> Prints current units\n";
                Console.Text += "\n";
                Console.Text += "printPossibleSprites() -> Prints possible sprites\n";
                Console.Text += "\n";
                Console.Text += "modifyTileCover(<Team>, <gridX>, <gridY>) -> Update tile cover team\n";
                Console.Text += "deleteTileCover(<gridX>, <gridY>) -> Removes tile cover\n";
                Console.Text += "\n";
                Console.Text += "clear -> clears console\n\n";

                Input.Text = "";
                return;
            }

            if (printUnitsRegex.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";
                Console.Text += "\nGridPosition\tSprite:\n";
                foreach (var u in ClientCommunicator.GetData<DrawableManager>("DrawableManager").drawables)
                {
                    Console.Text += $"[{u.Key.Item1}, {u.Key.Item2}]\t{u.Value}\n";
                }

                Input.Text = "";
                return;
            }

            if (clearRegex.IsMatch(Input.Text))
            {
                Console.Text = "";
                return;
            }

            if (identifier.IsMatch(Input.Text))
            {
                Console.Text += $"---> {Input.Text}\n";

                string unit = identifier.Match(Input.Text).Value;
                unit = unit.TrimEnd('.');
                if (!unitsCreated.ContainsKey(unit))
                {
                    Console.Text += $"No unit created with name {unit}\n";
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
                            unitsCreated[unit].MoveTo(new Vector2Int(x, y));
                            break;
                        }
                }

                Input.Text = "";
            }
        }

    }
}
