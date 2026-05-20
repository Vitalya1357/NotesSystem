using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesShared.Services;

namespace NotesUnitTests.TestCollection
{
    [TestClass]
    public class UnitTest_NoteService
    {
        [DataTestMethod]
        [DataRow("admin", "admin123", "Заметка от admin", true)]
        [DataRow("user1", "admin123", "Заметка от user", true)]
        public void AddNote_WithCorrectInput_ReturnsExpectedResult(
            string username,
            string password,
            string noteText,
            bool expectedResult)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);

            NoteService noteService = new NoteService();

            int noteId = noteService.AddNote(
                authService.CurrentUser.Id,
                noteText);

            bool actualResult = noteId > 0;

            Assert.AreEqual(expectedResult, actualResult);
        }

        [DataTestMethod]
        [DataRow("admin", "admin123", "Заметка для удаления от admin")]
        [DataRow("user1", "admin123", "Заметка для удаления от user")]
        public void DeleteNote_WithCorrectInput_DoesNotThrowException(
            string username,
            string password,
            string noteText)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);

            NoteService noteService = new NoteService();

            int noteId = noteService.AddNote(
                authService.CurrentUser.Id,
                noteText);

            noteService.DeleteNote(
                noteId,
                authService.CurrentUser.Id,
                authService.CurrentUser.Role);

            Assert.IsTrue(true);
        }

        [DataTestMethod]
        [DataRow("admin", "admin123", "Заметка до изменения admin", "Заметка после изменения admin")]
        [DataRow("user1", "admin123", "Заметка до изменения user", "Заметка после изменения user")]
        public void EditNote_WithCorrectInput_DoesNotThrowException(
            string username,
            string password,
            string oldText,
            string newText)
        {
            AuthService authService = new AuthService();

            bool loginResult = authService.Login(username, password);

            Assert.IsTrue(loginResult);

            NoteService noteService = new NoteService();

            int noteId = noteService.AddNote(
                authService.CurrentUser.Id,
                oldText);

            noteService.EditNote(
                noteId,
                authService.CurrentUser.Id,
                newText,
                authService.CurrentUser.Role);

            Assert.IsTrue(true);
        }
    }
}