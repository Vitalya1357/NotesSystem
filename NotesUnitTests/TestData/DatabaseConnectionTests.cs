using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NotesUnitTests
{
    /// <summary>
    /// Проверяет строки подключения, запрет postgres и параметры установщика.
    /// </summary>
    [TestClass]
    public sealed class DatabaseConnectionTests
    {
        /// <summary>
        /// Контекст выполнения теста.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Проверяет корректность строк подключения из XML.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("DatabaseConnection")]
        [DynamicData(nameof(XmlCaseLoader.ConnectionCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void ConnectionString_ByXmlData_IsValidated(XmlCase testCase)
        {
            string connection = testCase.Get("connection");

            bool result = IsConnectionStringValid(connection);

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("connection: " + connection);
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет, что рабочие операции не должны использовать пользователя postgres.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("DatabaseConnection")]
        [DynamicData(nameof(XmlCaseLoader.PostgresBanCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void PostgresUser_ByXmlData_IsForbidden(XmlCase testCase)
        {
            string connection = testCase.Get("connection");

            bool result = !IsPostgresUser(connection);

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("connection: " + connection);
            TestContext.WriteLine("postgres запрещен: " + IsPostgresUser(connection));
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет, что некорректные строки подключения отклоняются до реального подключения к БД.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("DatabaseConnection")]
        [DynamicData(nameof(XmlCaseLoader.ConnectionErrorCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void InvalidConnectionString_ByXmlData_IsRejectedBeforeDatabase(XmlCase testCase)
        {
            string connection = testCase.Get("connection");

            bool result = IsConnectionStringValid(connection);

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("connection: " + connection);
            TestContext.WriteLine("Реальное подключение к PostgreSQL не выполняется.");
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет параметры установщика.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Installer")]
        [DynamicData(nameof(XmlCaseLoader.InstallerCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void InstallerParameters_ByXmlData_AreValidated(XmlCase testCase)
        {
            string installPath = testCase.Get("path");
            string databaseName = testCase.Get("database");

            bool result = !string.IsNullOrWhiteSpace(installPath) &&
                          !string.IsNullOrWhiteSpace(databaseName);

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("path: " + installPath);
            TestContext.WriteLine("database: " + databaseName);
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет, что строка подключения содержит обязательные параметры.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        /// <returns>True, если строка подключения корректна.</returns>
        private static bool IsConnectionStringValid(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            Dictionary<string, string> parts = ParseConnectionString(connectionString);

            return parts.ContainsKey("Host") &&
                   parts.ContainsKey("Port") &&
                   parts.ContainsKey("Database") &&
                   parts.ContainsKey("Username") &&
                   parts.ContainsKey("Password");
        }

        /// <summary>
        /// Проверяет, используется ли в строке подключения пользователь postgres.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        /// <returns>True, если используется postgres.</returns>
        private static bool IsPostgresUser(string connectionString)
        {
            Dictionary<string, string> parts = ParseConnectionString(connectionString);

            if (!parts.ContainsKey("Username"))
            {
                return false;
            }

            return parts["Username"].Equals("postgres", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Разбирает строку подключения на пары ключ-значение.
        /// </summary>
        /// <param name="connectionString">Строка подключения.</param>
        /// <returns>Словарь параметров подключения.</returns>
        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return result;
            }

            string[] items = connectionString.Split(';');

            foreach (string item in items)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                int separatorIndex = item.IndexOf('=');

                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = item.Substring(0, separatorIndex).Trim();
                string value = item.Substring(separatorIndex + 1).Trim();

                if (!result.ContainsKey(key))
                {
                    result.Add(key, value);
                }
            }

            return result;
        }
    }
}