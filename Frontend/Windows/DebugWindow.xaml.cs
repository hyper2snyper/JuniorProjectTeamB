using JuniorProject.Backend.Agents;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using JuniorProject.Backend;
using JuniorProject.Backend.Helpers;
using JuniorProject.Backend.WorldData;

namespace JuniorProject.Frontend.Windows
{
	/// <summary>
	/// Interaction logic for DebugWindow.xaml
	/// </summary>
	public partial class DebugWindow : Window
	{
		private static DebugWindow? _instance;
		static DebugWindow Instance { 
			get {
				if (_instance == null)
				{
					_instance = new DebugWindow();
					_instance.Closing += _instance_Closing;
				}
				return _instance;
			}}

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

		public static readonly Regex spawnUnitRegex = new Regex("^spawnUnit\\ *\\([a-zA-z]+,\\ *[0-9]+,\\ *[0-9]+\\,\\ *[a-zA-z]+\\)\\ *$");
		
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
				List<Match> matches = stringParam.Matches(Input.Text).ToList();
				string unitType = stringInstance.Match(matches[0].Value).Value;
				string unitName = stringInstance.Match(matches[1].Value).Value;

				matches = intParam.Matches(Input.Text).ToList();
				int x = int.Parse(intInstance.Match(matches[0].Value).Value);
				int y = int.Parse(intInstance.Match(matches[1].Value).Value);

				unitsCreated.TryAdd(unitName, new Unit(unitType, ClientCommunicator.GetData<World>("World"), new Vector2Int(x, y)));
				Console.Text += $"Unit spawned at {x},{y} of type [{unitType}] with name [{unitName}]\n";
				return;
			}

			if(identifier.IsMatch(Input.Text))
			{
				string unit = identifier.Match(Input.Text).Value;
				unit = unit.TrimEnd('.');
				if(!unitsCreated.ContainsKey(unit))
				{
					Console.Text += $"No unit created with name {unit}\n";
					return;
				}
				string command = function.Match(Input.Text).Value;
				command = command.TrimStart('.');
				command = command.TrimEnd('(');
				switch(command)
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
			}
		}

	}
}
