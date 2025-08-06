using LibreHardwareMonitor.Hardware;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Services;
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
        private readonly ChartValues<ObservablePoint> _cpuPoints = [];
        private readonly ChartValues<ObservablePoint> _gpuPoints = [];

        public string CpuName { get; init; }
        public string GpuName { get; init; }
        
        public ObservableCollection<SensorItem> CpuSensors { get; set; } = new ObservableCollection<SensorItem>();
        public ObservableCollection<SensorItem> GpuSensors { get; set; } = new ObservableCollection<SensorItem>();
        public SeriesCollection CpuChartSeries { get; set; }
        public SeriesCollection GpuChartSeries { get; set; }

        public double CpuXAxisMin => _cpuPoints.Any() ? _cpuPoints.Min(p => p.X) : 0;
        public double CpuXAxisMax => _cpuPoints.Any() ? _cpuPoints.Max(p => p.X) : 60;
        public double GpuXAxisMin => _gpuPoints.Any() ? _gpuPoints.Min(p => p.X) : 0;
        public double GpuXAxisMax => _gpuPoints.Any() ? _gpuPoints.Max(p => p.X) : 60;

        public Func<double, string> TimeFormatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CpuSensorListView.ItemsSource = CpuSensors;
            GpuSensorListView.ItemsSource = GpuSensors;
            CpuChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "CPU Temp",
                    Values = _cpuPoints,
                    PointGeometry = null, // optional for smoother line
                    LineSmoothness = 0
                }
            };

            GpuChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "GPU Temp",
                    Values = _gpuPoints,
                    PointGeometry = null, // optional for smoother line
                    LineSmoothness = 0
                }
            };

            DataContext = this;

            TimeFormatter = value => $"{value:F0}s";

            var computer = new Computer
            {
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
                var (cpuSensorItems, currentCpuPackageTemp) = _hwService.GetCpuSensorData();
                var (gpuSensorItems, currentGpuCoreTemp) = _hwService.GetGpuSensorData();

                // UI thread
                // Dispatch sensor updates to UI
                Dispatcher.Invoke(() =>
                {
                    CpuSensors.Clear();
                    foreach (var item in cpuSensorItems)
                        CpuSensors.Add(item);

                    GpuSensors.Clear();
                    foreach (var item in gpuSensorItems)
                        GpuSensors.Add(item);

                    var now = DateTime.Now;
                    double elapsedSeconds = (now - _startTime).TotalSeconds;

                    if (currentCpuPackageTemp.HasValue)
                    {
                        _cpuPoints.Add(new ObservablePoint(elapsedSeconds, currentCpuPackageTemp.Value));
                    }
                    while (_cpuPoints.Any() && _cpuPoints[0].X < elapsedSeconds - maxSeconds)
                        _cpuPoints.RemoveAt(0);

                    if (currentGpuCoreTemp.HasValue) 
                    {
                        _gpuPoints.Add(new ObservablePoint(elapsedSeconds, currentGpuCoreTemp.Value));
                    }
                    while (_gpuPoints.Any() && _gpuPoints[0].X < elapsedSeconds - maxSeconds)
                        _gpuPoints.RemoveAt(0);

                    OnPropertyChanged(nameof(CpuXAxisMin));
                    OnPropertyChanged(nameof(CpuXAxisMax));
                    OnPropertyChanged(nameof(GpuXAxisMin));
                    OnPropertyChanged(nameof(GpuXAxisMax));
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}