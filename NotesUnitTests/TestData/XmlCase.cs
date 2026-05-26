using System.Collections.Generic;

namespace NotesUnitTests
{
    /// <summary>
    /// Представляет один тестовый набор, загруженный из XML.
    /// </summary>
    public sealed class XmlCase
    {
        private readonly Dictionary<string, string> values;

        /// <summary>
        /// Создает объект тестового набора.
        /// </summary>
        /// <param name="group">Название группы XML.</param>
        /// <param name="values">Словарь параметров теста.</param>
        public XmlCase(string group, Dictionary<string, string> values)
        {
            Group = group;
            this.values = values;
        }

        /// <summary>
        /// Название группы тестов.
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// Идентификатор теста.
        /// </summary>
        public string Id
        {
            get { return Get("id"); }
        }

        /// <summary>
        /// Название теста.
        /// </summary>
        public string Title
        {
            get { return Get("title"); }
        }

        /// <summary>
        /// Получает строковое значение параметра.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        /// <returns>Значение параметра или пустая строка.</returns>
        public string Get(string name)
        {
            if (values == null)
            {
                return string.Empty;
            }

            if (!values.ContainsKey(name))
            {
                return string.Empty;
            }

            return values[name];
        }

        /// <summary>
        /// Получает параметр как целое число.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        /// <returns>Целое число или 0.</returns>
        public int GetInt(string name)
        {
            int result;
            int.TryParse(Get(name), out result);
            return result;
        }

        /// <summary>
        /// Получает параметр как логическое значение.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        /// <returns>Логическое значение или false.</returns>
        public bool GetBool(string name)
        {
            bool result;
            bool.TryParse(Get(name), out result);
            return result;
        }

        /// <summary>
        /// Возвращает удобное имя теста для отображения в Test Explorer.
        /// </summary>
        /// <returns>Идентификатор и название теста.</returns>
        public override string ToString()
        {
            return Id + " - " + Title;
        }
    }
}