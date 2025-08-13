using MyHWMonitorWPFApp.Utilities;

namespace MyHWMonitorWPFAppUnitTests;

public class UtilitiesUnitTests
{
    [TestCase(20, "20s")]
    [TestCase(60, "1m")]
    [TestCase(100, "1m40s")]
    [TestCase(120, "2m")]
    [TestCase(3600, "1h")]
    [TestCase(3800, "1h3m20s")]
    [TestCase(9000, "2h30m")]
    public void FormatTotalSeconds_WhenGivenValidArgument_ReturnsExpectedString(double seconds, string expectedResult)
    {
        Assert.That(TimeFormatter.FormatTotalSeconds(seconds), Is.EqualTo(expectedResult));
    }
}
