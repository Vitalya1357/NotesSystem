using System.Collections.Generic;
using System.Xml.Linq;

namespace NotesUnitTests
{
    /// <summary>
    /// Преобразует XML-элемент Case в словарь параметров.
    /// </summary>
    public static class XmlValueReader
    {
        /// <summary>
        /// Читает все атрибуты XML-элемента.
        /// </summary>
        /// <param name="element">XML-элемент Case.</param>
        /// <returns>Словарь параметров теста.</returns>
        public static Dictionary<string, string> ReadAttributes(XElement element)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (XAttribute attribute in element.Attributes())
            {
                result[attribute.Name.LocalName] = attribute.Value;
            }

            return result;
        }
    }
}