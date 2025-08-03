using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHWMonitorWPFApp.Models;

namespace MyHWMonitorWPFApp.Services
{
    public class HardwareService
    {
        private readonly Computer _computer;
        private readonly HardwareType[] _gpuTypes = [HardwareType.GpuNvidia, HardwareType.GpuAmd, HardwareType.GpuIntel];

        public string CpuName { get; init; }
        public string GpuName { get; init; }

        public HardwareService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            _computer.Open();

            CpuName = GetCpuName();
            GpuName = GetGpuName();
        }

        public (List<SensorItem> sensorItems, float? currentTemp) GetCpuSensorData()
        {
            var sensorList = new List<SensorItem>();
            float? packageTemp = null;

            IHardware? cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu) ?? throw new Exception("Cannot detect CPU.");
            
            cpu.Update();

            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && !sensor.Name.Contains("TjMax"))
                {
                    sensorList.Add(new SensorItem
                    {
                        Name = sensor.Name,
                        Value = $"{sensor.Value.Value:F1}",
                        Min = $"{sensor.Min:F1}",
                        Max = $"{sensor.Max:F1}"
                    });

                    if (sensor.Name.Contains("CPU Package"))
                    {
                        packageTemp ??= sensor.Value.Value;
                    }
                }
            }

            return (sensorList, packageTemp);
        }

        public (List<SensorItem> sensorItems, float? currentTemp) GetGpuSensorData()
        {
            var sensorList = new List<SensorItem>();
            float? coreTemp = null;

            IHardware? gpu = _computer.Hardware.FirstOrDefault(h => _gpuTypes.Any(t => t == h.HardwareType)) ?? throw new Exception("Cannot detect GPU.");
            
            gpu.Update();

            foreach (var sensor in gpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    sensorList.Add(new SensorItem
                    {
                        Name = sensor.Name,
                        Value = $"{sensor.Value.Value:F1}",
                        Min = $"{sensor.Min:F1}",
                        Max = $"{sensor.Max:F1}"
                    });

                    if (sensor.Name.Contains("GPU Core"))
                    {
                        coreTemp ??= sensor.Value.Value;
                    }
                }
            }

            return (sensorList, coreTemp);
        }

        private string GetCpuName()
        {
            var cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            return cpu == null ? "CPU" : cpu.Name;
        }

        private string GetGpuName()
        {
            var gpu = _computer.Hardware.FirstOrDefault(h => _gpuTypes.Any(t => t == h.HardwareType));
            return gpu == null ? "GPU" : gpu.Name;
        }
    }
}
