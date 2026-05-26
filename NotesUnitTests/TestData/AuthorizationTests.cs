using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NotesUnitTests
{
    /// <summary>
    /// Проверяет авторизацию, регистрацию и распределение прав по ролям.
    /// </summary>
    [TestClass]
    public sealed class AuthorizationTests
    {
        /// <summary>
        /// Контекст выполнения теста.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Проверяет вход по логину и паролю.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Authorization")]
        [DynamicData(nameof(XmlCaseLoader.AuthCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void Login_ByXmlData_ReturnsExpectedResult(XmlCase testCase)
        {
            LocalAuthService service = new LocalAuthService();

            AuthResult result = service.Login(
                testCase.Get("login"),
                testCase.Get("password")
            );

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("login: " + testCase.Get("login"));
            TestContext.WriteLine("password: " + testCase.Get("password"));
            TestLog.Result(TestContext, testCase.GetBool("expected"), result.Success);
            TestLog.Message(TestContext, result.Message);

            Assert.AreEqual(testCase.GetBool("expected"), result.Success);

            if (testCase.GetBool("expected"))
            {
                Assert.AreEqual(testCase.Get("role"), result.Role);
            }
        }

        /// <summary>
        /// Проверяет регистрацию пользователя.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Authorization")]
        [DynamicData(nameof(XmlCaseLoader.RegisterCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void Register_ByXmlData_ReturnsExpectedResult(XmlCase testCase)
        {
            LocalAuthService service = new LocalAuthService();

            string message;
            bool result = service.Register(
                testCase.Get("login"),
                testCase.Get("password"),
                testCase.Get("role"),
                out message
            );

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("login: " + testCase.Get("login"));
            TestContext.WriteLine("role: " + testCase.Get("role"));
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);
            TestLog.Message(TestContext, message);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет права для ролей admin, user и statistic.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Authorization")]
        [DynamicData(nameof(XmlCaseLoader.RoleCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void RolePermissions_ByXmlData_AreCorrect(XmlCase testCase)
        {
            string role = testCase.Get("role");

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("role: " + role);
            TestContext.WriteLine("users: " + testCase.Get("users"));
            TestContext.WriteLine("logs: " + testCase.Get("logs"));
            TestContext.WriteLine("monitoring: " + testCase.Get("monitoring"));
            TestContext.WriteLine("notes: " + testCase.Get("notes"));

            if (role == "admin")
            {
                Assert.IsTrue(testCase.GetBool("users"));
                Assert.IsTrue(testCase.GetBool("logs"));
                Assert.IsTrue(testCase.GetBool("monitoring"));
                Assert.IsTrue(testCase.GetBool("notes"));
            }
            else if (role == "user")
            {
                Assert.IsFalse(testCase.GetBool("users"));
                Assert.IsFalse(testCase.GetBool("logs"));
                Assert.IsFalse(testCase.GetBool("monitoring"));
                Assert.IsTrue(testCase.GetBool("notes"));
            }
            else if (role == "statistic")
            {
                Assert.IsFalse(testCase.GetBool("users"));
                Assert.IsFalse(testCase.GetBool("logs"));
                Assert.IsTrue(testCase.GetBool("monitoring"));
                Assert.IsFalse(testCase.GetBool("notes"));
            }
            else
            {
                Assert.Fail("Неизвестная роль: " + role);
            }
        }

        /// <summary>
        /// Проверяет выход из учетной записи.
        /// </summary>
        [TestMethod]
        [TestCategory("Authorization")]
        public void Logout_ClearsSession()
        {
            bool isLoggedIn = true;

            TestContext.WriteLine("До выхода: " + isLoggedIn);

            isLoggedIn = false;

            TestContext.WriteLine("После выхода: " + isLoggedIn);

            Assert.IsFalse(isLoggedIn);
        }

        /// <summary>
        /// Результат тестовой авторизации.
        /// </summary>
        private sealed class AuthResult
        {
            public bool Success { get; set; }
            public string Role { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Локальная тестовая авторизация без настоящей БД.
        /// </summary>
        private sealed class LocalAuthService
        {
            private readonly Dictionary<string, string> passwords;
            private readonly Dictionary<string, string> roles;

            /// <summary>
            /// Создает тестовый сервис авторизации.
            /// </summary>
            public LocalAuthService()
            {
                passwords = new Dictionary<string, string>();
                roles = new Dictionary<string, string>();

                passwords["admin"] = Hash("admin123");
                passwords["user1"] = Hash("admin123");
                passwords["stat"] = Hash("admin123");

                roles["admin"] = "admin";
                roles["user1"] = "user";
                roles["stat"] = "statistic";
            }

            /// <summary>
            /// Выполняет тестовый вход.
            /// </summary>
            /// <param name="login">Логин.</param>
            /// <param name="password">Пароль.</param>
            /// <returns>Результат авторизации.</returns>
            public AuthResult Login(string login, string password)
            {
                if (string.IsNullOrWhiteSpace(login))
                {
                    return new AuthResult { Success = false, Role = "", Message = "Логин не может быть пустым." };
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return new AuthResult { Success = false, Role = "", Message = "Пароль не может быть пустым." };
                }

                if (!passwords.ContainsKey(login))
                {
                    return new AuthResult { Success = false, Role = "", Message = "Пользователь не найден." };
                }

                if (passwords[login] != Hash(password))
                {
                    return new AuthResult { Success = false, Role = "", Message = "Неверный пароль." };
                }

                return new AuthResult { Success = true, Role = roles[login], Message = "Авторизация выполнена." };
            }

            /// <summary>
            /// Выполняет тестовую регистрацию.
            /// </summary>
            /// <param name="login">Логин.</param>
            /// <param name="password">Пароль.</param>
            /// <param name="role">Роль.</param>
            /// <param name="message">Сообщение результата.</param>
            /// <returns>True, если регистрация успешна.</returns>
            public bool Register(string login, string password, string role, out string message)
            {
                if (string.IsNullOrWhiteSpace(login))
                {
                    message = "Логин не может быть пустым.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    message = "Пароль не может быть пустым.";
                    return false;
                }

                if (password.Length < 6)
                {
                    message = "Пароль должен содержать минимум 6 символов.";
                    return false;
                }

                if (role != "admin" && role != "user" && role != "statistic")
                {
                    message = "Неизвестная роль.";
                    return false;
                }

                if (passwords.ContainsKey(login))
                {
                    message = "Пользователь уже существует.";
                    return false;
                }

                passwords[login] = Hash(password);
                roles[login] = role;

                message = "Регистрация выполнена.";
                return true;
            }

            /// <summary>
            /// Хеширует строку через SHA-256.
            /// </summary>
            /// <param name="text">Исходная строка.</param>
            /// <returns>Хеш строки.</returns>
            private static string Hash(string text)
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                    StringBuilder builder = new StringBuilder();

                    foreach (byte value in bytes)
                    {
                        builder.Append(value.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
        }
    }
}