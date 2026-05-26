using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;

namespace NotesUnitTests
{
    /// <summary>
    /// Проверяет логику обновлений без обращения к настоящему GitHub.
    /// </summary>
    [TestClass]
    public sealed class UpdateTests
    {
        /// <summary>
        /// Контекст выполнения теста.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Проверяет обновления по данным из XML через fake HTTP handler.
        /// </summary>
        /// <param name="testCase">Данные теста из XML.</param>
        [DataTestMethod]
        [TestCategory("Updates")]
        [DynamicData(nameof(XmlCaseLoader.UpdateCases), typeof(XmlCaseLoader), DynamicDataSourceType.Method)]
        public void UpdateCheck_ByXmlData_UsesFakeHttp(XmlCase testCase)
        {
            bool result = CheckUpdateWithFakeHttp(
                testCase.Get("current"),
                testCase.Get("json"),
                testCase.GetInt("status")
            );

            TestLog.CaseInfo(TestContext, testCase);
            TestContext.WriteLine("current: " + testCase.Get("current"));
            TestContext.WriteLine("status: " + testCase.Get("status"));
            TestContext.WriteLine("json: " + testCase.Get("json"));
            TestContext.WriteLine("Настоящий GitHub не используется.");
            TestLog.Result(TestContext, testCase.GetBool("expected"), result);

            Assert.AreEqual(testCase.GetBool("expected"), result);
        }

        /// <summary>
        /// Проверяет сравнение версий приложения.
        /// </summary>
        /// <param name="currentVersion">Текущая версия.</param>
        /// <param name="latestVersion">Новая версия.</param>
        /// <param name="expected">Ожидаемый результат.</param>
        [DataTestMethod]
        [TestCategory("Updates")]
        [DataRow("0.9.0", "1.0.0", true)]
        [DataRow("1.0.0", "1.0.0", false)]
        [DataRow("1.0.1", "1.0.0", false)]
        [DataRow("1.0.0", "1.0.1", true)]
        [DataRow("v1.0.0", "v1.0.1", true)]
        public void VersionCompare_WithDifferentVersions_ReturnsExpectedResult(
            string currentVersion,
            string latestVersion,
            bool expected)
        {
            bool result = IsNewVersionAvailable(currentVersion, latestVersion);

            TestContext.WriteLine("currentVersion: " + currentVersion);
            TestContext.WriteLine("latestVersion: " + latestVersion);
            TestLog.Result(TestContext, expected, result);

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Проверяет контролируемую ошибку, если API не вернул zip-архив.
        /// </summary>
        [TestMethod]
        [TestCategory("Updates")]
        public void ApiResponseWithoutZip_ReturnsControlledMessage()
        {
            string json = "{ \"version\": \"1.0.1\", \"url\": \"https://example.com/NotesSystem.txt\" }";

            string version;
            string url;
            string message;

            bool result = TryParseUpdateJson(json, out version, out url, out message);

            TestContext.WriteLine("json: " + json);
            TestContext.WriteLine("result: " + result);
            TestContext.WriteLine("message: " + message);

            Assert.IsFalse(result);
            Assert.AreEqual("Ответ API не содержит zip-архив.", message);
        }

        /// <summary>
        /// Проверяет контролируемую ошибку при пустом ответе API.
        /// </summary>
        [TestMethod]
        [TestCategory("Updates")]
        public void EmptyApiResponse_ReturnsControlledMessage()
        {
            string json = "";

            string version;
            string url;
            string message;

            bool result = TryParseUpdateJson(json, out version, out url, out message);

            TestContext.WriteLine("json: " + json);
            TestContext.WriteLine("result: " + result);
            TestContext.WriteLine("message: " + message);

            Assert.IsFalse(result);
            Assert.AreEqual("Пустой ответ API.", message);
        }

        /// <summary>
        /// Проверяет обновление через поддельный HTTP-ответ.
        /// </summary>
        /// <param name="currentVersion">Текущая версия.</param>
        /// <param name="json">JSON-ответ API.</param>
        /// <param name="statusCode">HTTP-статус.</param>
        /// <returns>True, если обновление доступно.</returns>
        private static bool CheckUpdateWithFakeHttp(
            string currentVersion,
            string json,
            int statusCode)
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler(
                json,
                (HttpStatusCode)statusCode
            );

            using (HttpClient client = new HttpClient(handler))
            {
                HttpResponseMessage response = client
                    .GetAsync("https://fake.local/version.json")
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                string body = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                string version;
                string url;
                string message;

                bool parsed = TryParseUpdateJson(body, out version, out url, out message);

                if (!parsed)
                {
                    return false;
                }

                return IsNewVersionAvailable(currentVersion, version);
            }
        }

        /// <summary>
        /// Проверяет, является ли версия обновления новее текущей.
        /// </summary>
        /// <param name="currentVersion">Текущая версия.</param>
        /// <param name="latestVersion">Новая версия.</param>
        /// <returns>True, если новая версия больше текущей.</returns>
        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            Version current = NormalizeVersion(currentVersion);
            Version latest = NormalizeVersion(latestVersion);

            return latest > current;
        }

        /// <summary>
        /// Нормализует строку версии.
        /// </summary>
        /// <param name="version">Строка версии.</param>
        /// <returns>Объект Version.</returns>
        private static Version NormalizeVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return new Version(0, 0, 0);
            }

            version = version.Trim();

            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                version = version.Substring(1);
            }

            return new Version(version);
        }

        /// <summary>
        /// Разбирает JSON-ответ API обновлений.
        /// </summary>
        /// <param name="json">JSON-ответ.</param>
        /// <param name="version">Версия из ответа.</param>
        /// <param name="url">Ссылка на архив.</param>
        /// <param name="message">Сообщение результата.</param>
        /// <returns>True, если ответ корректный.</returns>
        private static bool TryParseUpdateJson(
            string json,
            out string version,
            out string url,
            out string message)
        {
            version = "";
            url = "";
            message = "";

            if (string.IsNullOrWhiteSpace(json))
            {
                message = "Пустой ответ API.";
                return false;
            }

            try
            {
                string normalized = json.Replace(" ", "");

                version = ExtractJsonValue(normalized, "version");
                url = ExtractJsonValue(normalized, "url");

                if (string.IsNullOrWhiteSpace(version))
                {
                    message = "В ответе API не найдена версия.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    message = "В ответе API не найдена ссылка на архив.";
                    return false;
                }

                if (!url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    message = "Ответ API не содержит zip-архив.";
                    return false;
                }

                message = "Ответ API обработан.";
                return true;
            }
            catch
            {
                message = "Некорректный ответ API.";
                return false;
            }
        }

        /// <summary>
        /// Извлекает строковое значение из простого JSON.
        /// </summary>
        /// <param name="json">JSON-строка.</param>
        /// <param name="propertyName">Название свойства.</param>
        /// <returns>Значение свойства или пустая строка.</returns>
        private static string ExtractJsonValue(string json, string propertyName)
        {
            string pattern = "\"" + propertyName + "\":\"";
            int start = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);

            if (start < 0)
            {
                return "";
            }

            start += pattern.Length;

            int end = json.IndexOf("\"", start, StringComparison.OrdinalIgnoreCase);

            if (end < 0)
            {
                return "";
            }

            return json.Substring(start, end - start);
        }
    }
}