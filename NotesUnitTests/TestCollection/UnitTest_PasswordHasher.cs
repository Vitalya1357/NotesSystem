using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Utils;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_PasswordHasher
    {
        [TestMethod]
        public void Hash_Admin123_ReturnsCorrectSha256()
        {
            string password = "admin123";

            string hash = PasswordHasher.Hash(password);

            Assert.AreEqual(
                "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9",
                hash
            );
        }

        [TestMethod]
        public void Hash_SamePassword_ReturnsSameHash()
        {
            string hash1 = PasswordHasher.Hash("admin123");
            string hash2 = PasswordHasher.Hash("admin123");

            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Hash_DifferentPasswords_ReturnDifferentHashes()
        {
            string hash1 = PasswordHasher.Hash("admin123");
            string hash2 = PasswordHasher.Hash("wrong_password");

            Assert.AreNotEqual(hash1, hash2);
        }
    }
}