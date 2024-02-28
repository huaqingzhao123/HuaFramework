

using System;

namespace XMLib.DataHandlers
{
    /// <summary>
    /// DataHandler
    /// </summary>
    public class DataHandler
    {
        public static string GetFileName<T>(string childName = null)
        {
            return GetFileName(typeof(T), childName);
        }

        public static string GetFileName(Type type, string childName = null)
        {
            string fileName = string.IsNullOrEmpty(childName)
                ? type.Name
                : $"{childName}.{type.Name}";

            return fileName;
        }
    }
}