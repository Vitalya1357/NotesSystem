using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Models;
using NotesShared.Services;
using System;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_UpdateService
    {
        [DataTestMethod]
        [DataRow("0.9.0", "1.0.0", true)]
        [DataRow("1.0.0", "1.0.0", false)]
        [DataRow("1.0.1", "1.0.0", false)]
        [DataRow("1.0.0", "1.0.1", true)]
        [DataRow("2.0.0", "1.9.9", false)]
        public void IsNewVersionAvailable_WithInputParameters_ReturnsExpectedResult(
            string currentVersion,
            string latestVersion,
            bool expectedResult)
        {
            UpdateService updateService = new UpdateService();

            bool actualResult = updateService.IsNewVersionAvailable(currentVersion, latestVersion);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [DataTestMethod]
        [DataRow("{ \"version\": \"1.0.1\", \"url\": \"https://github.com/test.zip\" }", "1.0.1")]
        [DataRow("{ \"version\": \"2.0.0\", \"url\": \"https://github.com/test.zip\" }", "2.0.0")]
        public void ParseUpdateInfo_WithCorrectJson_ReturnsExpectedVersion(
            string json,
            string expectedVersion)
        {
            UpdateService updateService = new UpdateService();

            UpdateInfo updateInfo = updateService.ParseUpdateInfo(json);

            Assert.AreEqual(expectedVersion, updateInfo.Version);
        }

        [DataTestMethod]
        [DataRow("{ \"version\": \"1.0.1\", \"url\": \"https://github.com/test.zip\" }", "https://github.com/test.zip")]
        [DataRow("{ \"version\": \"1.0.2\", \"url\": \"https://github.com/archive.zip\" }", "https://github.com/archive.zip")]
        public void ParseUpdateInfo_WithCorrectJson_ReturnsExpectedDownloadUrl(
            string json,
            string expectedDownloadUrl)
        {
            UpdateService updateService = new UpdateService();

            UpdateInfo updateInfo = updateService.ParseUpdateInfo(json);

            Assert.AreEqual(expectedDownloadUrl, updateInfo.DownloadUrl);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("{ }")]
        [DataRow("{ \"version\": \"1.0.1\" }")]
        [DataRow("{ \"url\": \"https://github.com/test.zip\" }")]
        public void ParseUpdateInfo_WithIncorrectJson_ThrowsException(
            string json)
        {
            UpdateService updateService = new UpdateService();

            Assert.ThrowsException<Exception>(() =>
            {
                updateService.ParseUpdateInfo(json);
            });
        }

        [DataTestMethod]
        [DataRow("{ \"version\": \"1.0.1\" }", "version", "1.0.1")]
        [DataRow("{ \"url\": \"https://github.com/test.zip\" }", "url", "https://github.com/test.zip")]
        [DataRow("{ \"version\": \"1.0.1\" }", "url", "")]
        public void ExtractJsonValue_WithInputParameters_ReturnsExpectedResult(
            string json,
            string key,
            string expectedResult)
        {
            UpdateService updateService = new UpdateService();

            string actualResult = updateService.ExtractJsonValue(json, key);

            Assert.AreEqual(expectedResult, actualResult);
        }
    }
}