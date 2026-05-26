using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotesUnitTests
{
    /// <summary>
    /// Проверяет операции с заметками без подключения к реальной БД.
    /// </summary>
    [TestClass]
    public sealed class NoteTests
    {
        /// <summary>
        /// Контекст выполнения теста.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Проверяет добавление, поиск, редактирование и удаление заметок по данным из XML.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Notes")]
        [DynamicData(nameof(XmlCaseLoader.NoteCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void NoteOperation_ByXmlData_ReturnsExpectedResult(XmlCase testCase)
        {
            LocalNoteStorage storage = new LocalNoteStorage();

            string action = testCase.Get("action");
            int userId = testCase.GetInt("userId");
            string text = testCase.Get("text");
            string search = testCase.Get("search");

            bool result = false;

            if (action == "add")
            {
                int noteId = storage.Add(userId, text);
                result = noteId > 0;
            }
            else if (action == "search")
            {
                storage.Add(userId, text);
                result = storage.Search(userId, search).Count > 0;
            }
            else if (action == "edit")
            {
                int noteId = storage.Add(userId, "Старый текст заметки");
                result = storage.Edit(noteId, userId, text);
            }
            else if (action == "delete")
            {
                int noteId = storage.Add(userId, text);
                result = storage.Delete(noteId, userId);
            }
            else
            {
                Assert.Fail("Неизвестное действие с заметкой: " + action);
            }

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("action: " + action);
            TestContext.WriteLine("userId: " + userId);
            TestContext.WriteLine("text: " + text);
            TestContext.WriteLine("search: " + search);
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет, что пользователь не может редактировать чужую заметку.
        /// </summary>
        [TestMethod]
        [TestCategory("Notes")]
        public void User_CannotEditAnotherUserNote()
        {
            LocalNoteStorage storage = new LocalNoteStorage();

            int ownerUserId = 1;
            int anotherUserId = 2;

            int noteId = storage.Add(ownerUserId, "Чужая заметка");
            bool result = storage.Edit(noteId, anotherUserId, "Попытка изменения");

            TestContext.WriteLine("noteId: " + noteId);
            TestContext.WriteLine("ownerUserId: " + ownerUserId);
            TestContext.WriteLine("anotherUserId: " + anotherUserId);
            TestContext.WriteLine("Результат редактирования: " + result);

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Проверяет, что пользователь не может удалить чужую заметку.
        /// </summary>
        [TestMethod]
        [TestCategory("Notes")]
        public void User_CannotDeleteAnotherUserNote()
        {
            LocalNoteStorage storage = new LocalNoteStorage();

            int ownerUserId = 1;
            int anotherUserId = 2;

            int noteId = storage.Add(ownerUserId, "Чужая заметка");
            bool result = storage.Delete(noteId, anotherUserId);

            TestContext.WriteLine("noteId: " + noteId);
            TestContext.WriteLine("ownerUserId: " + ownerUserId);
            TestContext.WriteLine("anotherUserId: " + anotherUserId);
            TestContext.WriteLine("Результат удаления: " + result);

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Локальная модель заметки для unit-тестов.
        /// </summary>
        private sealed class LocalNote
        {
            /// <summary>
            /// Идентификатор заметки.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Идентификатор владельца заметки.
            /// </summary>
            public int UserId { get; set; }

            /// <summary>
            /// Текст заметки.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Дата создания заметки.
            /// </summary>
            public DateTime CreatedAt { get; set; }
        }

        /// <summary>
        /// Локальное хранилище заметок для тестов без настоящей БД.
        /// </summary>
        private sealed class LocalNoteStorage
        {
            private readonly List<LocalNote> notes;
            private int nextId;

            /// <summary>
            /// Создает локальное хранилище заметок.
            /// </summary>
            public LocalNoteStorage()
            {
                notes = new List<LocalNote>();
                nextId = 1;
            }

            /// <summary>
            /// Добавляет заметку.
            /// </summary>
            /// <param name="userId">Идентификатор пользователя.</param>
            /// <param name="text">Текст заметки.</param>
            /// <returns>Идентификатор созданной заметки или 0.</returns>
            public int Add(int userId, string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return 0;
                }

                LocalNote note = new LocalNote();
                note.Id = nextId;
                note.UserId = userId;
                note.Text = text;
                note.CreatedAt = DateTime.Now;

                notes.Add(note);
                nextId++;

                return note.Id;
            }

            /// <summary>
            /// Выполняет поиск заметок пользователя.
            /// </summary>
            /// <param name="userId">Идентификатор пользователя.</param>
            /// <param name="searchText">Текст поиска.</param>
            /// <returns>Список найденных заметок.</returns>
            public List<LocalNote> Search(int userId, string searchText)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return new List<LocalNote>();
                }

                return notes
                    .Where(note => note.UserId == userId && note.Text.Contains(searchText))
                    .OrderBy(note => note.Id)
                    .ToList();
            }

            /// <summary>
            /// Редактирует заметку пользователя.
            /// </summary>
            /// <param name="noteId">Идентификатор заметки.</param>
            /// <param name="userId">Идентификатор пользователя.</param>
            /// <param name="newText">Новый текст заметки.</param>
            /// <returns>True, если редактирование выполнено.</returns>
            public bool Edit(int noteId, int userId, string newText)
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    return false;
                }

                LocalNote note = notes.FirstOrDefault(
                    item => item.Id == noteId && item.UserId == userId
                );

                if (note == null)
                {
                    return false;
                }

                note.Text = newText;
                return true;
            }

            /// <summary>
            /// Удаляет заметку пользователя.
            /// </summary>
            /// <param name="noteId">Идентификатор заметки.</param>
            /// <param name="userId">Идентификатор пользователя.</param>
            /// <returns>True, если удаление выполнено.</returns>
            public bool Delete(int noteId, int userId)
            {
                LocalNote note = notes.FirstOrDefault(
                    item => item.Id == noteId && item.UserId == userId
                );

                if (note == null)
                {
                    return false;
                }

                notes.Remove(note);
                return true;
            }
        }
    }
}