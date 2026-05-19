using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Models;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_NoteService
    {
        [TestMethod]
        public void GetLocalStats_ReturnsDeviceName()
        {
            SystemMetricService service = new SystemMetricService();

            SystemMetric metric = service.GetLocalStats();

            Assert.IsNotNull(metric);
            Assert.IsFalse(string.IsNullOrWhiteSpace(metric.DeviceName));
        }

        [TestMethod]
        public void GetLocalStats_ReturnsCorrectPercentRanges()
        {
            SystemMetricService service = new SystemMetricService();

            SystemMetric metric = service.GetLocalStats();

            Assert.IsTrue(metric.CpuUsage >= 0 && metric.CpuUsage <= 100);
            Assert.IsTrue(metric.RamUsage >= 0 && metric.RamUsage <= 100);
            Assert.IsTrue(metric.HddUsage >= 0 && metric.HddUsage <= 100);
        }

        [TestMethod]
        public void GetLocalStats_ReturnsCreatedAt()
        {
            SystemMetricService service = new SystemMetricService();

            SystemMetric metric = service.GetLocalStats();

            Assert.AreNotEqual(default(System.DateTime), metric.CreatedAt);
        }
    }
}