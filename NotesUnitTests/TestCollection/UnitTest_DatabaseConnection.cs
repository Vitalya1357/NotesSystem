using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Config;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_DatabaseConnection
    {
        [TestInitialize]
        public void Init()
        {
            AppConfig.UseAuthConnection();
        }

        [TestMethod]
        public void GetConnectionString_ReturnsNotEmptyString()
        {
            string connectionString = AppConfig.GetConnectionString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionString));
        }

        [TestMethod]
        public void GetConnectionString_ContainsDatabaseName()
        {
            string connectionString = AppConfig.GetConnectionString();

            Assert.IsTrue(connectionString.Contains("Database=NotesSystemDB"));
        }

        [TestMethod]
        public void GetConnectionString_ContainsHost()
        {
            string connectionString = AppConfig.GetConnectionString();

            Assert.IsTrue(connectionString.Contains("Host=localhost"));
        }

        [TestMethod]
        public void GetConnectionString_ContainsPort()
        {
            string connectionString = AppConfig.GetConnectionString();

            Assert.IsTrue(connectionString.Contains("Port=5432"));
        }

        [TestMethod]
        public void UseAuthConnection_SetsAuthUser()
        {
            AppConfig.UseAuthConnection();

            string connectionString = AppConfig.GetConnectionString();

            Assert.IsTrue(connectionString.Contains("Username=notes_auth"));
        }

        [TestMethod]
        public void UseConnectionString_SetsRuntimeConnectionString()
        {
            string testConnection =
                "Host=localhost;Port=5432;Database=NotesSystemDB;Username=test_user;Password=test_password";

            AppConfig.UseConnectionString(testConnection);

            string connectionString = AppConfig.GetConnectionString();

            Assert.AreEqual(testConnection, connectionString);
        }

        [TestMethod]
        public void ClearRuntimeConnectionString_ReturnsToAuthConnection()
        {
            string testConnection =
                "Host=localhost;Port=5432;Database=NotesSystemDB;Username=test_user;Password=test_password";

            AppConfig.UseConnectionString(testConnection);
            AppConfig.ClearRuntimeConnectionString();
            AppConfig.UseAuthConnection();

            string connectionString = AppConfig.GetConnectionString();

            Assert.IsTrue(connectionString.Contains("Username=notes_auth"));
        }
    }
}