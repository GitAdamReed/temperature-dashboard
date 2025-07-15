using LibreHardwareMonitor.Hardware;
using System.Collections.ObjectModel;
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
    public partial class MainWindow : Window
    {
        private Computer _computer;
        private DispatcherTimer _timer;
        public ObservableCollection<SensorItem> CpuSensors { get; set; } = new ObservableCollection<SensorItem>();
        public ObservableCollection<SensorItem> GpuSensors { get; set; } = new ObservableCollection<SensorItem>();

        public MainWindow()
        {
            InitializeComponent();
            CpuSensorListView.ItemsSource = CpuSensors;
            GpuSensorListView.ItemsSource = GpuSensors;

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
            CpuSensors.Clear();
            GpuSensors.Clear();

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
                            CpuSensors.Add(new SensorItem
                            {
                                Name = sensor.Name,
                                Value = $"{sensor.Value.Value:F1}",
                                Min = $"{sensor.Min:F1}",
                                Max = $"{sensor.Max:F1}"
                            });
                        }
                        else if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                        {
                            GpuSensors.Add(new SensorItem
                            {
                                Name = sensor.Name,
                                Value = $"{sensor.Value.Value:F1}",
                                Min = $"{sensor.Min:F1}",
                                Max = $"{sensor.Max:F1}"
                            });
                        }
                    }
                }
            }
        }
        public class SensorItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
        }
    }
}