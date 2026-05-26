using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotesUnitTests
{
    /// <summary>
    /// Выполняет единообразный вывод информации в результатах тестов.
    /// </summary>
    public static class TestLog
    {
        /// <summary>
        /// Выводит заголовок тестового случая.
        /// </summary>
        /// <param name="context">Контекст теста.</param>
        /// <param name="testCase">Тестовый случай.</param>
        public static void CaseInfo(TestContext context, XmlCase testCase)
        {
            context.WriteLine("ID: " + testCase.Id);
            context.WriteLine("Группа: " + testCase.Group);
            context.WriteLine("Название: " + testCase.Title);
        }

        /// <summary>
        /// Выводит ожидаемый и фактический результат.
        /// </summary>
        /// <param name="context">Контекст теста.</param>
        /// <param name="expected">Ожидаемый результат.</param>
        /// <param name="actual">Фактический результат.</param>
        public static void Result(TestContext context, bool expected, bool actual)
        {
            context.WriteLine("Ожидалось: " + expected);
            context.WriteLine("Получено: " + actual);
        }

        /// <summary>
        /// Выводит произвольное сообщение.
        /// </summary>
        /// <param name="context">Контекст теста.</param>
        /// <param name="message">Сообщение.</param>
        public static void Message(TestContext context, string message)
        {
            context.WriteLine("Сообщение: " + message);
        }
    }
}