using MyHWMonitorWPFApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHWMonitorWPFApp.Interfaces
{
    public interface IHardwareService
    {
        (List<SensorItem> sensorItems, float? currentTemp) GetCpuSensorData();
        (List<SensorItem> sensorItems, float? currentTemp) GetGpuSensorData();
    }
}
