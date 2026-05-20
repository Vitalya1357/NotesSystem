using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_DatabaseConnection
    {
        [DataTestMethod]
        [DataRow("Host=localhost;Port=5432;Database=NotesSystemDB;Username=test;", true)]
        [DataRow("", false)]
        [DataRow("Host=localhost;", false)]
        [DataRow("Database=NotesSystemDB;", false)]
        public void ConnectionString_WithInputParameters_ReturnsExpectedResult(string connectionString,bool expectedResult)
        {
            bool actualResult =
                !string.IsNullOrWhiteSpace(connectionString) &&
                connectionString.Contains("Host=") &&
                connectionString.Contains("Port=") &&
                connectionString.Contains("Database=") &&
                connectionString.Contains("Username=");

            Assert.AreEqual(expectedResult, actualResult);
        }
    }
}