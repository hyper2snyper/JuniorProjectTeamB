using JuniorProject.Backend.Agents;
using System.Collections.Generic; // Required for List
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JuniorProject.Frontend.Windows
{
    public class ViewModel : INotifyPropertyChanged
    {
        // Data for "Prices" tab - now a dictionary to hold different resource prices
        public Dictionary<string, ISeries[]> AllPricesSeries { get; set; } = new Dictionary<string, ISeries[]>()
        {
            {
                "Resource Alpha", new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = new double[] { 10, 20, 15, 25, 22, 30 },
                        Name = "Alpha Price"
                    },
                    new LineSeries<double>
                    {
                        Values = new double[] { 8, 15, 12, 20, 18, 25 },
                        Name = "Alpha Trend"
                    }
                }
            },
            {
                "Resource Beta", new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = new double[] { 50, 45, 55, 48, 60, 52 },
                        Name = "Beta Price"
                    },
                    new LineSeries<double>
                    {
                        Values = new double[] { 40, 38, 42, 35, 48, 40 },
                        Name = "Beta Trend"
                    }
                }
            },
            {
                "Resource Gamma", new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = new double[] { 100, 95, 110, 105, 120, 115 },
                        Name = "Gamma Price"
                    },
                    new LineSeries<double>
                    {
                        Values = new double[] { 90, 88, 100, 95, 110, 100 },
                        Name = "Gamma Trend"
                    }
                }
            },
            {
                "Resource Delta", new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = new double[] { 5, 7, 6, 8, 7, 9 },
                        Name = "Delta Price"
                    },
                    new LineSeries<double>
                    {
                        Values = new double[] { 3, 5, 4, 6, 5, 7 },
                        Name = "Delta Trend"
                    }
                }
            }
        };

        // Collection for the ComboBox items
        public List<string> AvailableResources { get; set; } = new List<string>
        {
            "Resource Alpha",
            "Resource Beta",
            "Resource Gamma",
            "Resource Delta"
        };

        private string _selectedResource;
        public string SelectedResource
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                OnPropertyChanged();
                UpdateCurrentSeries(); // Update chart data when selected resource changes
            }
        }

        // Data for "Resource Amount" tab (unchanged)
        public ISeries[] ResourceAmountSeries { get; set; } =
        [
            new ColumnSeries<int>
            {
                Values = new int[] { 100, 150, 120, 180, 160 },
                Name = "Resource X"
            },
            new ColumnSeries<int>
            {
                Values = new int[] { 80, 110, 90, 130, 100 },
                Name = "Resource Y"
            }
        ];

        // Data for "Trades" tab (unchanged)
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
                UpdateCurrentSeries(); // Update chart data when tab changes
            }
        }

        public ViewModel()
        {
            // Initialize selected resource and current series
            _selectedResource = AvailableResources[0]; // Default to the first resource
            _currentSeries = AllPricesSeries[_selectedResource]; // Set initial chart data based on default resource
            _selectedTabIndex = 0; // Initialize selected tab to Prices
        }

        private void UpdateCurrentSeries()
        {
            switch (SelectedTabIndex)
            {
                case 0: // Prices tab
                    // If the Prices tab is selected, use the data for the selected resource
                    if (AllPricesSeries.ContainsKey(SelectedResource))
                    {
                        CurrentSeries = AllPricesSeries[SelectedResource];
                    }
                    else
                    {
                        // Fallback if selected resource not found
                        CurrentSeries = AllPricesSeries[AvailableResources[0]];
                    }
                    break;
                case 1: // Resource Amount tab
                    CurrentSeries = ResourceAmountSeries;
                    break;
                case 2: // Trades tab
                    CurrentSeries = TradesSeries;
                    break;
                default:
                    CurrentSeries = AllPricesSeries[AvailableResources[0]]; // Fallback
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class HistoryWindow : Window
    {
        private static HistoryWindow? _instance;
        static HistoryWindow Instance
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

        private static void _instance_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
        }

        public HistoryWindow()
        {
            InitializeComponent();
        }
    }
}