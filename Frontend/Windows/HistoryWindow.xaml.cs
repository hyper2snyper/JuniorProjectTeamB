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
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace JuniorProject.Frontend.Windows
{
    public class ViewModel : INotifyPropertyChanged
    {
        // Nation resource pie chart properties
        private List<PieSeries<int>> _nationResourceSlices = new();
        public List<PieSeries<int>> NationResourceSlices
        {
            get => _nationResourceSlices;
            set
            {
                _nationResourceSlices = value;
                OnPropertyChanged();
            }
        }

        public List<string> AvailableResources { get; set; } = new() { "Food", "Wood", "Iron", "Gold" };
        public List<string> AvailableNations { get; set; } = new();

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

        public ISeries[] NationResourceSeries => NationResourceSlices.ToArray();

        public ViewModel()
        {
            _selectedResource = AvailableResources[0];
            _selectedTabIndex = 0;
            _currentSeries = new ISeries[0];
        }

        public void UpdateLiveData()
        {
            var itemsHistory = ClientCommunicator.GetData<Dictionary<ulong, List<EconomyManager.Resource>>>("itemsHistory");
            var tradesHistory = ClientCommunicator.GetData<List<EconomyManager.Trade>>("tradesHistory");
            var nationResources = ClientCommunicator.GetData<Dictionary<string, Dictionary<string, int>>>("nationResources");

            if (itemsHistory != null)
            {
                var sorted = itemsHistory.OrderBy(kvp => kvp.Key);

                foreach (var line in ResourceLines.Values)
                {
                    line.Values = new List<int>();
                }

                foreach (var (_, resourceList) in sorted)
                {
                    foreach (var resource in resourceList)
                    {
                        if (ResourceLines.TryGetValue(resource.name, out var resourceLine) && resourceLine.Values is List<int> resourceValues)
                        {
                            resourceValues.Add(resource.totalResource);
                        }
                    }
                }
            }

            if (nationResources != null)
            {
                AvailableNations = nationResources.Keys.ToList();
                OnPropertyChanged(nameof(AvailableNations));

                NationResourceSlices.Clear();

                if (!string.IsNullOrEmpty(SelectedResource))
                {
                    foreach (var nationData in nationResources)
                    {
                        string nationName = nationData.Key;
                        var resources = nationData.Value;

                        if (resources.ContainsKey(SelectedResource))
                        {
                            int resourceAmount = resources[SelectedResource];

                            if (resourceAmount > 0)
                            {
                                var pieSeries = new PieSeries<int>
                                {
                                    Values = new[] { resourceAmount },
                                    Name = $"{nationName} ({resourceAmount})",
                                    Fill = new SolidColorPaint(nationName == "Red" ? SKColors.Red : nationName == "Yellow" ? SKColors.Yellow : SKColors.Green)
                                };
                                NationResourceSlices.Add(pieSeries);
                            }
                        }
                    }
                }

                OnPropertyChanged(nameof(NationResourceSlices));
                OnPropertyChanged(nameof(NationResourceSeries));
            }

            // Update trades
            if (tradesHistory != null)
            {
                foreach (var column in AcceptedTradeColumns.Values)
                {
                    column.Values = new List<int>();
                }

                var acceptedTradeCounts = new Dictionary<string, int>
                {
                    { "Food", 0 },
                    { "Wood", 0 },
                    { "Iron", 0 },
                };

                foreach (var trade in tradesHistory)
                {
                    if (trade.accepted)
                    {
                        if (acceptedTradeCounts.ContainsKey(trade.resource))
                        {
                            acceptedTradeCounts[trade.resource]++;
                        }
                    }
                }

                foreach (var kvp in acceptedTradeCounts)
                {
                    if (AcceptedTradeColumns.TryGetValue(kvp.Key, out var column) && column.Values is List<int> values)
                    {
                        values.Add(kvp.Value);
                    }
                }
            }
            UpdateCurrentSeries();
        }

        private void UpdateCurrentSeries()
        {
            switch (SelectedTabIndex)
            {
                case 0: // Nation Resources tab (formerly Prices tab)
                    CurrentSeries = NationResourceSeries;
                    break;
                case 1: // All resources tab
                    CurrentSeries = ResourceAmountSeries;
                    break;
                case 2: // Trades tab
                    CurrentSeries = TradesSeries;
                    break;
                default:
                    CurrentSeries = NationResourceSeries;
                    break;
            }
        }

        public Dictionary<string, ColumnSeries<int>> AcceptedTradeColumns { get; private set; } = new()
        {
            { "Food", new ColumnSeries<int> { Values = new List<int>(), Name = "Food Accepted" } },
            { "Wood", new ColumnSeries<int> { Values = new List<int>(), Name = "Wood Accepted" } },
            { "Iron", new ColumnSeries<int> { Values = new List<int>(), Name = "Iron Accepted" } },
            { "Gold", new ColumnSeries<int> { Values = new List<int>(), Name = "Gold Accepted" } }
        };

        public ISeries[] TradesSeries => AcceptedTradeColumns.Values.ToArray();

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