using Hardware.Info;
using System.Net.NetworkInformation;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace MyHWMonitorApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //IHardwareInfo hardwareInfo = new HardwareInfo();
            //PrintHWInfo(hardwareInfo);
            GetCPUTemp();
        }

        static void PrintHWInfo(IHardwareInfo hw)
        {
            try
            {
                //hardwareInfo.RefreshOperatingSystem();
                //hardwareInfo.RefreshMemoryStatus();
                //hardwareInfo.RefreshBatteryList();
                //hardwareInfo.RefreshBIOSList();
                //hardwareInfo.RefreshComputerSystemList();
                //hardwareInfo.RefreshCPUList();
                //hardwareInfo.RefreshDriveList();
                //hardwareInfo.RefreshKeyboardList();
                //hardwareInfo.RefreshMemoryList();
                //hardwareInfo.RefreshMonitorList();
                //hardwareInfo.RefreshMotherboardList();
                //hardwareInfo.RefreshMouseList();
                //hardwareInfo.RefreshNetworkAdapterList();
                //hardwareInfo.RefreshPrinterList();
                //hardwareInfo.RefreshSoundDeviceList();
                //hardwareInfo.RefreshVideoControllerList();

                hw.RefreshAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine(hw.OperatingSystem);

            Console.WriteLine(hw.MemoryStatus);

            foreach (var hardware in hw.BatteryList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.BiosList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.ComputerSystemList)
                Console.WriteLine(hardware);

            foreach (var cpu in hw.CpuList)
            {
                Console.WriteLine(cpu);

                foreach (var cpuCore in cpu.CpuCoreList)
                    Console.WriteLine(cpuCore);
            }

            foreach (var drive in hw.DriveList)
            {
                Console.WriteLine(drive);

                foreach (var partition in drive.PartitionList)
                {
                    Console.WriteLine(partition);

                    foreach (var volume in partition.VolumeList)
                        Console.WriteLine(volume);
                }
            }

            foreach (var hardware in hw.KeyboardList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.MemoryList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.MonitorList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.MotherboardList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.MouseList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.NetworkAdapterList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.PrinterList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.SoundDeviceList)
                Console.WriteLine(hardware);

            foreach (var hardware in hw.VideoControllerList)
                Console.WriteLine(hardware);

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Ethernet, OperationalStatus.Up))
                Console.WriteLine(address);

            Console.WriteLine();

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(NetworkInterfaceType.Wireless80211))
                Console.WriteLine(address);

            Console.WriteLine();

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses(OperationalStatus.Up))
                Console.WriteLine(address);

            Console.WriteLine();

            foreach (var address in HardwareInfo.GetLocalIPv4Addresses())
                Console.WriteLine(address);

            Console.ReadLine();
        }

        static void GetCPUTemp()
        {
            var computer = new Computer();
            computer.IsCpuEnabled = true;
            computer.Open();

            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();

                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && !sensor.Name.Contains("TjMax"))
                        {
                            Console.WriteLine($"{sensor.Name} ({sensor.SensorType}): {(sensor.Value.HasValue ? $"{sensor.Value.Value} °C" : "No value")}");
                        }
                    }
                }
            }
        }
    }
}
