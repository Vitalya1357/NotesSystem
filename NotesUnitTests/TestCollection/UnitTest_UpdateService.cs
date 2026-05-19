using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Config;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_UpdateService
    {
        [TestMethod]
        public void AppVersion_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(AppConfig.AppVersion));
        }

        [TestMethod]
        public void UpdateInfoUrl_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(AppConfig.UpdateInfoUrl));
        }

        [TestMethod]
        public void UpdateInfoUrl_ContainsGithub()
        {
            Assert.IsTrue(AppConfig.UpdateInfoUrl.Contains("github"));
        }

        [TestMethod]
        public void UpdateService_CanBeCreated()
        {
            UpdateService updateService = new UpdateService();

            Assert.IsNotNull(updateService);
        }
    }
}