using JuniorProject.Backend.Agents;
using JuniorProject.Backend.WorldData;
using JuniorProject.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JuniorProject.Backend;

namespace JuniorProject.Frontend.Windows
{
    public class ViewModel : INotifyPropertyChanged
    {
        public Dictionary<string, ISeries[]> AllPricesSeries { get; set; } = new()
        {
            { "Resource Alpha", new ISeries[] { new LineSeries<double> { Values = new double[] { }, Name = "Alpha Price" } } },
            { "Resource Beta", new ISeries[] { new LineSeries<double> { Values = new double[] { }, Name = "Beta Price" } } },
            { "Resource Gamma", new ISeries[] { new LineSeries<double> { Values = new double[] { }, Name = "Gamma Price" } } },
            { "Resource Delta", new ISeries[] { new LineSeries<double> { Values = new double[] { }, Name = "Delta Price" } } }
        };

        public List<string> AvailableResources { get; set; } = new() { "Resource Alpha", "Resource Beta", "Resource Gamma", "Resource Delta" };

        private string _selectedResource;
        public string SelectedResource
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                OnPropertyChanged();
                UpdateCurrentSeries();
            }
        }

        private ISeries[] _currentSeries;
        public ISeries[] CurrentSeries
        {
            get => _currentSeries;
            set
            {
                _currentSeries = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
                UpdateCurrentSeries();
            }
        }

        public Dictionary<string, LineSeries<int>> ResourceLines { get; private set; } = new()
        {
            { "Food", new LineSeries<int> { Values = new List<int>(), Name = "Food" } },
            { "Iron", new LineSeries<int> { Values = new List<int>(), Name = "Iron" } },
            { "Wood", new LineSeries<int> { Values = new List<int>(), Name = "Wood" } },
            { "Gold", new LineSeries<int> { Values = new List<int>(), Name = "Gold" } }
        };
        public ISeries[] ResourceAmountSeries => ResourceLines.Values.ToArray();

        public ViewModel()
        {
            _selectedResource = AvailableResources[0];
            _selectedTabIndex = 0;
            _currentSeries = AllPricesSeries[_selectedResource];
        }

        public void UpdateLiveData()
        {
            var itemsHistory = ClientCommunicator.GetData<Dictionary<ulong, List<EconomyManager.Resource>>>("itemsHistory");
            if (itemsHistory == null) return;

            var sorted = itemsHistory.OrderBy(kvp => kvp.Key);

            foreach (var line in ResourceLines.Values)
            {
                line.Values = new List<int>();
            }

            foreach (var (_, resourceList) in sorted)
            {
                foreach (var resource in resourceList)
                {
                    if (ResourceLines.TryGetValue(resource.name, out var line) && line.Values is List<int> values)
                    {
                        values.Add(resource.totalResource);
                    }
                }
            }

            if (SelectedTabIndex == 1)
            {
                CurrentSeries = ResourceAmountSeries;
            }
        }

        private void UpdateCurrentSeries()
        {
            switch (SelectedTabIndex)
            {
                case 0:
                    if (AllPricesSeries.ContainsKey(SelectedResource))
                        CurrentSeries = AllPricesSeries[SelectedResource];
                    else
                        CurrentSeries = AllPricesSeries[AvailableResources[0]];
                    break;
                case 1:
                    CurrentSeries = ResourceAmountSeries;
                    break;
                case 2:
                    CurrentSeries = TradesSeries;
                    break;
                default:
                    CurrentSeries = AllPricesSeries[AvailableResources[0]];
                    break;
            }
        }

        public ISeries[] TradesSeries { get; set; } =
        [
            new LineSeries<int>
            {
                Values = new int[] { 5, 8, 3, 10, 7, 12 },
                Name = "Buy Trades"
            },
            new LineSeries<int>
            {
                Values = new int[] { 4, 6, 2, 9, 6, 10 },
                Name = "Sell Trades"
            }
        ];

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public partial class HistoryWindow : Window
    {
        private static HistoryWindow? _instance;
        public static HistoryWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HistoryWindow();
                    _instance.Closing += _instance_Closing;
                }
                return _instance;
            }
        }

        private static void _instance_Closing(object? sender, CancelEventArgs e)
        {
            _instance = null;
        }

        private readonly DispatcherTimer _timer;
        private readonly ViewModel _viewModel;

        public HistoryWindow()
        {
            InitializeComponent();
            _viewModel = new ViewModel();
            DataContext = _viewModel;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => _viewModel.UpdateLiveData();
            _timer.Start();
        }
    }
}
