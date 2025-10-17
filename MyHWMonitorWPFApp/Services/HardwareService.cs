using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Interfaces;

namespace MyHWMonitorWPFApp.Services
{
    public class HardwareService : IHardwareService
    {
        private readonly IComputer _computer;
        private readonly HardwareType[] _gpuTypes = [HardwareType.GpuNvidia, HardwareType.GpuAmd, HardwareType.GpuIntel];

        public string CpuName { get; init; }
        public string GpuName { get; init; }

        public HardwareService(IComputer computer)
        {
            _computer = computer;
            CpuName = GetCpuName();
            GpuName = GetGpuName();
        }

        public (List<SensorItem> sensorItems, float? currentTemp) GetCpuTempSensorData()
        {
            var sensorList = new List<SensorItem>();
            float? packageTemp = null;

            IHardware cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu) ?? throw new Exception("Cannot detect CPU.");
            
            cpu.Update();

            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && !sensor.Name.Contains("TjMax"))
                {
                    sensorList.Add(new SensorItem
                    {
                        Name = sensor.Name,
                        Value = $"{sensor.Value.Value:F1}", // °C
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

        public (List<SensorItem> sensorItems, Dictionary<string, decimal> fanSpeed, decimal? cpuFanSpeed) GetMoboFanSpeed()
        {
            var sensorList = new List<SensorItem>();
            var fanSpeedDict = new Dictionary<string, decimal>(); // Use decimal for fan speed as it is more accurate when rounding

            IHardware mobo = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard) ?? throw new Exception("Cannot detect Motherboard.");

            mobo.Update();

            foreach (var sensor in mobo.Sensors)
            {
                if (sensor.SensorType == SensorType.Fan && sensor.Value.HasValue && sensor.Value.Value > 0)
                {
                    string fanName = sensor.Name.Equals("FAN #1", StringComparison.CurrentCultureIgnoreCase) ? "CPU" : sensor.Name;
                    sensorList.Add(new SensorItem
                    {
                        Name = fanName,
                        Value = $"{sensor.Value.Value:F1}",
                        Min = $"{sensor.Min:F1}",
                        Max = $"{sensor.Max:F1}"
                    });
                    fanSpeedDict.Add(fanName, Math.Round((decimal)sensor.Value.Value, 1));
                }
            }

            foreach (var subHW in mobo.SubHardware)
            {
                subHW.Update();

                foreach (var sensor in subHW.Sensors)
                {
                    if (sensor.SensorType == SensorType.Fan && sensor.Value.HasValue && sensor.Value.Value > 0)
                    {
                        string fanName = sensor.Name.Equals("FAN #1", StringComparison.CurrentCultureIgnoreCase) ? "CPU" : sensor.Name;
                        sensorList.Add(new SensorItem
                        {
                            Name = fanName,
                            Value = $"{sensor.Value.Value:F1}",
                            Min = $"{sensor.Min:F1}",
                            Max = $"{sensor.Max:F1}"
                        });
                        fanSpeedDict.Add(fanName, Math.Round((decimal)sensor.Value.Value, 1));
                    }
                }
            }

            // Calculate average fan speed
            //float? averageSpeed = fanSpeedList.Count > 0 ? float.Round(fanSpeedList.Average(), 1) : null;

            // Get CPU fan speed
            // Identify CPU fan or fall back to first available motherboard fan
            decimal? cpuFanSpeed = fanSpeedDict[fanSpeedDict.Keys.FirstOrDefault(k => k.Contains("CPU"), fanSpeedDict.Keys.First())];

            return (sensorList, fanSpeedDict, cpuFanSpeed);
        }

        public (List<SensorItem> sensorItems, float? currentTemp) GetGpuSensorData()
        {
            var sensorList = new List<SensorItem>();
            float? coreTemp = null;

            IHardware gpu = _computer.Hardware.FirstOrDefault(h => _gpuTypes.Any(t => t == h.HardwareType)) ?? throw new Exception("Cannot detect GPU.");
            
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
