using LibreHardwareMonitor.Hardware;
using Moq;
using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Services;
using System.Management;

namespace MyHWMonitorWPFAppUnitTests
{
    public class HardwareServiceUnitTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void HardwareService_Constructs()
        {
            var computer = new Computer();
            var hwService = new HardwareService(computer);
            Assert.That(hwService, Is.InstanceOf<HardwareService>());
        }

        [Test]
        public void GetCpuName_WhenHardwareIsCpu_ReturnsCorrectName()
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(HardwareType.Cpu);
            string stubCpuName = "Stub CPU";
            stubHardware.SetupGet(n => n.Name).Returns(stubCpuName);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.CpuName, Is.EqualTo(stubCpuName));
        }

        [Test]
        public void GetCpuName_WhenHardwareIsNotCpu_ReturnsDefaultName()
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(new HardwareType());

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.CpuName, Is.EqualTo("CPU"));
        }

        [TestCase(HardwareType.GpuNvidia)]
        [TestCase(HardwareType.GpuAmd)]
        [TestCase(HardwareType.GpuIntel)]
        public void GetGpuName_WhenHardwareIsValidGpuType_ReturnsCorrectName(HardwareType gpuType)
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(gpuType);
            string stubGpuName = "Stub GPU";
            stubHardware.SetupGet(n => n.Name).Returns(stubGpuName);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.GpuName, Is.EqualTo(stubGpuName));
        }

        [Test]
        public void GetGpuName_WhenHardwareIsInvalidGpuType_ReturnsDefaultName()
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(new HardwareType());

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.GpuName, Is.EqualTo("GPU"));
        }

        [Test]
        public void GetCpuSensorData_WhenCpuSensorsHaveReadings_ReturnsCorrectData()
        {
            var stubSensor = new Mock<ISensor>();
            stubSensor.SetupGet(s => s.SensorType).Returns(SensorType.Temperature);
            stubSensor.SetupGet(s => s.Name).Returns("CPU Package");
            float stubSensorValue = 50f;
            float stubSensorMin = 25f;
            float stubSensorMax = 75f;
            stubSensor.SetupGet(s => s.Value).Returns(stubSensorValue);
            stubSensor.SetupGet(s => s.Min).Returns(stubSensorMin);
            stubSensor.SetupGet(s => s.Max).Returns(stubSensorMax);

            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(HardwareType.Cpu);
            stubHardware.SetupGet(s => s.Sensors).Returns([stubSensor.Object]);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            var expectedSensorItem = new SensorItem
            {
                Name = "CPU Package",
                Value = $"{stubSensorValue:F1}",
                Min = $"{stubSensorMin:F1}",
                Max = $"{stubSensorMax:F1}"
            };
            var (sensorItems, currentTemp) = hwService.GetCpuSensorData();

            Assert.Multiple(() =>
            {
                Assert.That(currentTemp, Is.EqualTo(50f));
                Assert.That(sensorItems[0].Name, Is.EqualTo(expectedSensorItem.Name));
                Assert.That(sensorItems[0].Value, Is.EqualTo(expectedSensorItem.Value));
                Assert.That(sensorItems[0].Min, Is.EqualTo(expectedSensorItem.Min));
                Assert.That(sensorItems[0].Max, Is.EqualTo(expectedSensorItem.Max));
            });
        }

        [Test]
        public void GetCpuSensorData_WhenCpuIsNotDetected_ThrowsException()
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(HardwareType.GpuNvidia);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.GetCpuSensorData, Throws.Exception.Message.Contains("Cannot detect CPU."));
        }

        [Test]
        public void GetGpuSensorData_WhenGpuSensorsHaveReadings_ReturnsCorrectData()
        {
            var stubSensor = new Mock<ISensor>();
            stubSensor.SetupGet(s => s.SensorType).Returns(SensorType.Temperature);
            stubSensor.SetupGet(s => s.Name).Returns("GPU Core");
            float stubSensorValue = 40f;
            float stubSensorMin = 20f;
            float stubSensorMax = 55f;
            stubSensor.SetupGet(s => s.Value).Returns(stubSensorValue);
            stubSensor.SetupGet(s => s.Min).Returns(stubSensorMin);
            stubSensor.SetupGet(s => s.Max).Returns(stubSensorMax);

            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(HardwareType.GpuNvidia);
            stubHardware.SetupGet(s => s.Sensors).Returns([stubSensor.Object]);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            var expectedSensorItem = new SensorItem
            {
                Name = "GPU Core",
                Value = $"{stubSensorValue:F1}",
                Min = $"{stubSensorMin:F1}",
                Max = $"{stubSensorMax:F1}"
            };

            var (sensorItems, currentTemp) = hwService.GetGpuSensorData();

            Assert.Multiple(() =>
            {
                Assert.That(currentTemp, Is.EqualTo(40f));
                Assert.That(sensorItems[0].Name, Is.EqualTo(expectedSensorItem.Name));
                Assert.That(sensorItems[0].Value, Is.EqualTo(expectedSensorItem.Value));
                Assert.That(sensorItems[0].Min, Is.EqualTo(expectedSensorItem.Min));
                Assert.That(sensorItems[0].Max, Is.EqualTo(expectedSensorItem.Max));
            });
        }

        [Test]
        public void GetGpuSensorData_WhenGpuIsNotDetected_ThrowsException()
        {
            var stubHardware = new Mock<IHardware>();
            stubHardware.SetupGet(h => h.HardwareType).Returns(HardwareType.Cpu);

            var stubComputer = new Mock<IComputer>();
            stubComputer.SetupGet(c => c.Hardware).Returns([stubHardware.Object]);

            var hwService = new HardwareService(stubComputer.Object);

            Assert.That(hwService.GetGpuSensorData, Throws.Exception.Message.Contains("Cannot detect GPU."));
        }
    }
}
