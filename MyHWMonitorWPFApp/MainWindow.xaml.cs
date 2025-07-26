using LibreHardwareMonitor.Hardware;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
        private Computer _computer;
        private DispatcherTimer _timer;
        private DateTime _startTime = DateTime.Now;
        private ChartValues<ObservablePoint> _cpuPoints = new();
        private ChartValues<ObservablePoint> _gpuPoints = new();

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

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            _computer.Open();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += UpdateSensors;
            _timer.Start();
        }

        private void UpdateSensors(object sender, EventArgs e)
        {
            var cpuList = new List<SensorItem>();
            var gpuList = new List<SensorItem>();
            float? latestCpuTemp = null;
            float? latestGpuTemp = null;
            int maxSeconds = 60; // Keep last 60 seconds of data

            // Background thread
            Task.Run(() => 
            {
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Cpu
                        || hardware.HardwareType == HardwareType.GpuNvidia
                        || hardware.HardwareType == HardwareType.GpuAmd
                        || hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        hardware.Update();

                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && !sensor.Name.Contains("TjMax") && hardware.HardwareType == HardwareType.Cpu)
                            {
                                cpuList.Add(new SensorItem
                                {
                                    Name = sensor.Name,
                                    Value = $"{sensor.Value.Value:F1}",
                                    Min = $"{sensor.Min:F1}",
                                    Max = $"{sensor.Max:F1}"
                                });

                                if (sensor.Name.Contains("CPU Package"))
                                {
                                    latestCpuTemp ??= sensor.Value.Value;
                                }
                            }
                            else if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd))
                            {
                                gpuList.Add(new SensorItem
                                {
                                    Name = sensor.Name,
                                    Value = $"{sensor.Value.Value:F1}",
                                    Min = $"{sensor.Min:F1}",
                                    Max = $"{sensor.Max:F1}"
                                });

                                if (sensor.Name.Contains("GPU Core"))
                                {
                                    latestGpuTemp ??= sensor.Value.Value;
                                }
                            }
                        }
                    }
                }

                // UI thread
                // Dispatch sensor updates to UI
                Dispatcher.Invoke(() =>
                {
                    CpuSensors.Clear();
                    foreach (var item in cpuList)
                        CpuSensors.Add(item);

                    GpuSensors.Clear();
                    foreach (var item in gpuList)
                        GpuSensors.Add(item);

                    var now = DateTime.Now;
                    double elapsedSeconds = (now - _startTime).TotalSeconds;

                    if (latestCpuTemp.HasValue)
                    {
                        _cpuPoints.Add(new ObservablePoint(elapsedSeconds, latestCpuTemp.Value));
                    }
                    while (_cpuPoints.Any() && _cpuPoints[0].X < elapsedSeconds - maxSeconds)
                        _cpuPoints.RemoveAt(0);

                    if (latestGpuTemp.HasValue) 
                    {
                        _gpuPoints.Add(new ObservablePoint(elapsedSeconds, latestGpuTemp.Value));
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

        public class SensorItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
        }
    }
}