using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_RoleAccess
    {
        [TestMethod]
        public void HasRole_Admin_HasAdminAccess()
        {
            AuthService authService = new AuthService();

            authService.Login("admin", "admin123");

            Assert.IsTrue(authService.HasRole("admin"));
            Assert.IsTrue(authService.HasRole("admin", "user"));
            Assert.IsTrue(authService.HasRole("admin", "statistic"));
        }

        [TestMethod]
        public void HasRole_User_HasOnlyUserAccess()
        {
            AuthService authService = new AuthService();

            authService.Login("user1", "admin123");

            Assert.IsTrue(authService.HasRole("user"));
            Assert.IsFalse(authService.HasRole("admin"));
            Assert.IsFalse(authService.HasRole("statistic"));
        }

        [TestMethod]
        public void HasRole_Statistic_HasOnlyStatisticAccess()
        {
            AuthService authService = new AuthService();

            authService.Login("stat", "admin123");

            Assert.IsTrue(authService.HasRole("statistic"));
            Assert.IsTrue(authService.HasRole("admin", "statistic"));
            Assert.IsFalse(authService.HasRole("user"));
        }

        [TestMethod]
        public void HasRole_NotLoggedIn_ReturnsFalse()
        {
            AuthService authService = new AuthService();

            Assert.IsFalse(authService.HasRole("admin"));
            Assert.IsFalse(authService.HasRole("user"));
            Assert.IsFalse(authService.HasRole("statistic"));
        }
    }
}