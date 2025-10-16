using LibreHardwareMonitor.Hardware;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Services;
using MyHWMonitorWPFApp.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyHWMonitorWPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly HardwareService _hwService;
        private readonly DispatcherTimer _timer;
        private readonly DateTime _startTime = DateTime.Now;
        private readonly ChartValues<ObservablePoint> _cpuTempPoints = [];
        private readonly ChartValues<ObservablePoint> _gpuTempPoints = [];
        private readonly Dictionary<string, ChartValues<ObservablePoint>> _moboFanPoints = []; // Should this be a dictionary of chart values for each fan line series?

        public string CpuName { get; init; }
        public string GpuName { get; init; }
        
        public ObservableCollection<SensorItem> CpuSensors { get; set; } = [];
        public ObservableCollection<SensorItem> GpuSensors { get; set; } = [];
        public ObservableCollection<SensorItem> MoboSensors { get; set; } = [];
        public SeriesCollection TempChartSeries { get; set; }
        public SeriesCollection GpuChartSeries { get; set; }
        public SeriesCollection FanChartSeries { get; set; }

        public double TempXAxisMin => _cpuTempPoints.Any() ? _cpuTempPoints.Min(p => p.X) : 0;
        public double TempXAxisMax => _cpuTempPoints.Any() ? _cpuTempPoints.Max(p => p.X) : 60;
        public double FanXAxisMin => _moboFanPoints.Count > 0 ? _moboFanPoints.First().Value.Min(p => p.X) : 0;
        public double FanXAxisMax => _moboFanPoints.Count > 0 ? _moboFanPoints.First().Value.Max(p => p.X) : 60;

        public Func<double, string> TimeElapsed { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CpuSensorListView.ItemsSource = CpuSensors;
            GpuSensorListView.ItemsSource = GpuSensors;
            MoboSensorListView.ItemsSource = MoboSensors;
            TempChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "CPU Temp",
                    Values = _cpuTempPoints,
                    PointGeometry = null, // optional for smoother line
                    LineSmoothness = 0,
                    Fill = Brushes.Transparent
                },
                new LineSeries
                {
                    Title = "GPU Temp",
                    Values = _gpuTempPoints,
                    PointGeometry = null, // optional for smoother line
                    LineSmoothness = 0,
                    Fill = Brushes.Transparent
                }
            };

            // Should have multiple line series for each mobo fan and average GPU fan speed
            FanChartSeries = new SeriesCollection();

            DataContext = this;

            TimeElapsed = TimeFormatter.FormatTotalSeconds;

            var computer = new Computer
            {
                IsMotherboardEnabled = true,
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            computer.Open();
            _hwService = new HardwareService(computer);

            CpuName = _hwService.CpuName;
            GpuName = _hwService.GpuName;
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += UpdateSensors;
            _timer.Start();
        }

        private void UpdateSensors(object sender, EventArgs e)
        {
            int maxSeconds = 60; // Keep last 60 seconds of data

            // Background thread
            Task.Run(() => 
            {
                var (cpuSensorItems, currentCpuPackageTemp) = _hwService.GetCpuTempSensorData();
                var (gpuSensorItems, currentGpuCoreTemp) = _hwService.GetGpuSensorData();
                var (moboSensorItems, fanSpeedDict, currentCpuFanSpeed) = _hwService.GetMoboFanSpeed();

                // UI thread
                // Dispatch sensor updates to UI
                Dispatcher.Invoke(() =>
                {
                    MoboSensors.Clear();
                    foreach (var item in moboSensorItems)
                        MoboSensors.Add(item);

                    CpuSensors.Clear();
                    foreach (var item in cpuSensorItems)
                        CpuSensors.Add(item);

                    GpuSensors.Clear();
                    foreach (var item in gpuSensorItems)
                        GpuSensors.Add(item);

                    var now = DateTime.Now;
                    double elapsedSeconds = (now - _startTime).TotalSeconds;

                    foreach (var kvp in fanSpeedDict)
                    {
                        if (_moboFanPoints.Any(x => x.Key == kvp.Key))
                        {
                            _moboFanPoints[kvp.Key].Add(new ObservablePoint(elapsedSeconds, (double)kvp.Value));
                        }
                        else
                        {
                            var point = new ObservablePoint(elapsedSeconds, (double)kvp.Value);
                            _moboFanPoints.Add(kvp.Key, new ChartValues<ObservablePoint>() { point });
                            
                            var series = new LineSeries
                            {
                                Title = kvp.Key,
                                Values = _moboFanPoints[kvp.Key],
                                PointGeometry = null, // optional for smoother line
                                LineSmoothness = 0,
                                Fill = Brushes.Transparent
                            };
                            FanChartSeries.Add(series);
                        }
                        while (_moboFanPoints[kvp.Key].Any() && _moboFanPoints[kvp.Key].First().X < elapsedSeconds - maxSeconds)
                            _moboFanPoints[kvp.Key].RemoveAt(0);
                    }

                    if (currentCpuPackageTemp.HasValue)
                    {
                        _cpuTempPoints.Add(new ObservablePoint(elapsedSeconds, currentCpuPackageTemp.Value));
                    }
                    while (_cpuTempPoints.Any() && _cpuTempPoints[0].X < elapsedSeconds - maxSeconds)
                        _cpuTempPoints.RemoveAt(0);

                    if (currentGpuCoreTemp.HasValue) 
                    {
                        _gpuTempPoints.Add(new ObservablePoint(elapsedSeconds, currentGpuCoreTemp.Value));
                    }
                    while (_gpuTempPoints.Any() && _gpuTempPoints[0].X < elapsedSeconds - maxSeconds)
                        _gpuTempPoints.RemoveAt(0);

                    OnPropertyChanged(nameof(TempXAxisMin));
                    OnPropertyChanged(nameof(TempXAxisMax));
                    OnPropertyChanged(nameof(FanXAxisMin));
                    OnPropertyChanged(nameof(FanXAxisMax));
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}