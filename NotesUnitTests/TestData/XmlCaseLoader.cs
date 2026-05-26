using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace NotesUnitTests
{
    /// <summary>
    /// Загружает тестовые наборы из XML-файла.
    /// </summary>
    public static class XmlCaseLoader
    {
        /// <summary>
        /// Загружает XML-файл с тестовыми данными.
        /// </summary>
        /// <returns>XML-документ.</returns>
        private static XDocument LoadDocument()
        {
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestData",
                "test-input-data.xml"
            );

            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "TestData",
                    "test-input-data.xml"
                );
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Не найден файл test-input-data.xml.", filePath);
            }

            return XDocument.Load(filePath);
        }

        /// <summary>
        /// Читает группу тестовых данных.
        /// </summary>
        /// <param name="groupName">Название группы в XML.</param>
        /// <returns>Коллекция данных для DynamicData.</returns>
        private static IEnumerable<object[]> LoadGroup(string groupName)
        {
            XDocument document = LoadDocument();

            XElement group = document.Root.Element(groupName);

            if (group == null)
            {
                throw new Exception("В XML не найдена группа: " + groupName);
            }

            foreach (XElement item in group.Elements("Case"))
            {
                Dictionary<string, string> values = XmlValueReader.ReadAttributes(item);
                XmlCase testCase = new XmlCase(groupName, values);

                yield return new object[] { testCase };
            }
        }

        /// <summary>
        /// Возвращает данные для тестов авторизации.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> AuthCases()
        {
            return LoadGroup("AuthCases");
        }

        /// <summary>
        /// Возвращает данные для тестов регистрации.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> RegisterCases()
        {
            return LoadGroup("RegisterCases");
        }

        /// <summary>
        /// Возвращает данные для тестов ролей.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> RoleCases()
        {
            return LoadGroup("RoleCases");
        }

        /// <summary>
        /// Возвращает данные для тестов строк подключения.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> ConnectionCases()
        {
            return LoadGroup("ConnectionCases");
        }

        /// <summary>
        /// Возвращает данные для проверки запрета postgres.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> PostgresBanCases()
        {
            return LoadGroup("PostgresBanCases");
        }

        /// <summary>
        /// Возвращает данные для ошибок подключения.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> ConnectionErrorCases()
        {
            return LoadGroup("ConnectionErrorCases");
        }

        /// <summary>
        /// Возвращает данные для тестов заметок.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> NoteCases()
        {
            return LoadGroup("NoteCases");
        }

        /// <summary>
        /// Возвращает данные для тестов обновлений.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> UpdateCases()
        {
            return LoadGroup("UpdateCases");
        }

        /// <summary>
        /// Возвращает данные для тестов параметров установщика.
        /// </summary>
        /// <returns>Наборы тестовых данных.</returns>
        public static IEnumerable<object[]> InstallerCases()
        {
            return LoadGroup("InstallerCases");
        }
    }
}