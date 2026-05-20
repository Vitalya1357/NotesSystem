using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_AuthService
    {
        [DataTestMethod]
        [DataRow("admin", "admin123", true)]
        [DataRow("user1", "admin123", true)]
        [DataRow("stat", "admin123", true)]
        [DataRow("admin", "wrong_password", false)]
        [DataRow("unknown_user", "admin123", false)]
        public void Login_WithInputParameters_ReturnsExpectedResult(
            string username,
            string password,
            bool expectedResult)
        {
            AuthService authService = new AuthService();

            bool actualResult = authService.Login(username, password);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [DataTestMethod]
        [DataRow("admin", "admin123", "admin")]
        [DataRow("user1", "admin123", "user")]
        [DataRow("stat", "admin123", "statistic")]
        public void Login_WithCorrectUser_ReturnsExpectedRole(
            string username,
            string password,
            string expectedRole)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);
            Assert.IsNotNull(authService.CurrentUser);
            Assert.AreEqual(expectedRole, authService.CurrentUser.Role);
        }

        [DataTestMethod]
        [DataRow("admin", "admin123", "admin", true)]
        [DataRow("admin", "admin123", "user", false)]
        [DataRow("admin", "admin123", "statistic", false)]
        [DataRow("user1", "admin123", "user", true)]
        [DataRow("user1", "admin123", "admin", false)]
        [DataRow("user1", "admin123", "statistic", false)]
        [DataRow("stat", "admin123", "statistic", true)]
        [DataRow("stat", "admin123", "admin", false)]
        [DataRow("stat", "admin123", "user", false)]
        public void HasRole_WithInputParameters_ReturnsExpectedResult(
            string username,
            string password,
            string checkedRole,
            bool expectedResult)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);

            bool actualResult = authService.HasRole(checkedRole);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [DataTestMethod]
        [DataRow("admin", "admin123")]
        [DataRow("user1", "admin123")]
        [DataRow("stat", "admin123")]
        public void Logout_AfterLogin_UserIsNotLoggedIn(
            string username,
            string password)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);
            Assert.IsTrue(authService.IsLoggedIn);
            Assert.IsNotNull(authService.CurrentUser);

            authService.Logout();

            Assert.IsFalse(authService.IsLoggedIn);
            Assert.IsNull(authService.CurrentUser);
        }
    }
}