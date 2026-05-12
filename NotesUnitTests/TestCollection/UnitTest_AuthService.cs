using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_AuthService
    {
        [TestMethod]
        public void Login_AdminWithCorrectPassword_ReturnsTrue()
        {
            AuthService authService = new AuthService();

            bool result = authService.Login("admin", "admin123");

            Assert.IsTrue(result);
            Assert.IsNotNull(authService.CurrentUser);
            Assert.AreEqual("admin", authService.CurrentUser.Username);
            Assert.AreEqual("admin", authService.CurrentUser.Role);
        }

        [TestMethod]
        public void Login_UserWithCorrectPassword_ReturnsTrue()
        {
            AuthService authService = new AuthService();

            bool result = authService.Login("user1", "admin123");

            Assert.IsTrue(result);
            Assert.IsNotNull(authService.CurrentUser);
            Assert.AreEqual("user1", authService.CurrentUser.Username);
            Assert.AreEqual("user", authService.CurrentUser.Role);
        }

        [TestMethod]
        public void Login_StatisticWithCorrectPassword_ReturnsTrue()
        {
            AuthService authService = new AuthService();

            bool result = authService.Login("stat", "admin123");

            Assert.IsTrue(result);
            Assert.IsNotNull(authService.CurrentUser);
            Assert.AreEqual("stat", authService.CurrentUser.Username);
            Assert.AreEqual("statistic", authService.CurrentUser.Role);
        }

        [TestMethod]
        public void Login_WrongPassword_ReturnsFalse()
        {
            AuthService authService = new AuthService();

            bool result = authService.Login("admin", "wrong_password");

            Assert.IsFalse(result);
            Assert.IsNull(authService.CurrentUser);
        }
    }
}