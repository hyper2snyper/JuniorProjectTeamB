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
        public Dictionary<string, LineSeries<double>> PriceLines { get; private set; } = new()
        {
            { "Food", new LineSeries<double> { Values = new List<double>(), Name = "Food" } },
            { "Wood", new LineSeries<double> { Values = new List<double>(), Name = "Wood" } },
            { "Iron", new LineSeries<double> { Values = new List<double>(), Name = "Iron" } },
            { "Gold", new LineSeries<double> { Values = new List<double>(), Name = "Gold" } }
        };

        public Dictionary<string, ISeries[]> AllPricesSeries { get; set; } = new();

        public List<string> AvailableResources { get; set; } = new() { "Food", "Wood", "Iron", "Gold" };

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

            foreach (var resource in AvailableResources)
            {
                AllPricesSeries[resource] = new ISeries[] { PriceLines[resource] };
            }

            _currentSeries = AllPricesSeries[_selectedResource];
        }

        public void UpdateLiveData()
        {
            var itemsHistory = ClientCommunicator.GetData<Dictionary<ulong, List<EconomyManager.Resource>>>("itemsHistory");
            if (itemsHistory == null) return;

            var sorted = itemsHistory.OrderBy(kvp => kvp.Key);

            // Clear existing data
            foreach (var line in ResourceLines.Values)
            {
                line.Values = new List<int>();
            }

            foreach (var line in PriceLines.Values)
            {
                line.Values = new List<double>();
            }

            foreach (var (_, resourceList) in sorted)
            {
                foreach (var resource in resourceList)
                {
                    if (ResourceLines.TryGetValue(resource.name, out var resourceLine) && resourceLine.Values is List<int> resourceValues)
                    {
                        resourceValues.Add(resource.totalResource);
                    }

                    if (PriceLines.TryGetValue(resource.name, out var priceLine) && priceLine.Values is List<double> priceValues)
                    {
                        priceValues.Add(resource.price);
                    }
                }
            }


            UpdateCurrentSeries();
        }

        private void UpdateCurrentSeries()
        {
            switch (SelectedTabIndex)
            {
                case 0: // Prices tab
                    if (AllPricesSeries.ContainsKey(SelectedResource))
                        CurrentSeries = AllPricesSeries[SelectedResource];
                    else
                        CurrentSeries = AllPricesSeries[AvailableResources[0]];
                    break;
                case 1: // All resources tab
                    CurrentSeries = ResourceAmountSeries;
                    break;
                case 2: // Trades tab
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