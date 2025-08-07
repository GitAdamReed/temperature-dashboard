using MyHWMonitorWPFApp.Models;
using MyHWMonitorWPFApp.Services;
using Moq;
using LibreHardwareMonitor.Hardware;

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
    }
}
