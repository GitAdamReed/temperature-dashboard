using LibreHardwareMonitor.Hardware;
using Moq;
using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Services;
using System.Management;

namespace MyHWMonitorWPFAppUnitTests
{
    public class Tests
    {
        //private HardwareService _hwService;
        
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
    }
}
