using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesShared.Config
{
    public static class AppConfig
    {
        public static string Host = "localhost";
        public static string Port = "5432";
        public static string Database = "NotesSystemDB";

        public static string Username = "";
        public static string Password = "";

        public static string RuntimeConnectionString = "";

        public static string AppVersion = "1.0.1";

        public static string UpdateInfoUrl =
            "https://raw.githubusercontent.com/Vitalya1357/NotesSystem/develop/releases/version.json";

        public static string GetConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(RuntimeConnectionString))
                return RuntimeConnectionString;

            return
                "Host=" + Host + ";" +
                "Port=" + Port + ";" +
                "Database=" + Database + ";" +
                "Username=" + Username + ";" +
                "Password=" + Password;
        }

        public static void UseAuthConnection()
        {
            RuntimeConnectionString = "";

            Username = "notes_auth";
            Password = "auth123";
        }

        public static void UseConnectionString(string connectionString)
        {
            RuntimeConnectionString = connectionString;
        }

        public static void ClearRuntimeConnectionString()
        {
            RuntimeConnectionString = "";
        }
        public static void UseWatcherConnectionFromEnvironment()
        {
            string host = Environment.GetEnvironmentVariable("NOTES_DB_HOST");
            string port = Environment.GetEnvironmentVariable("NOTES_DB_PORT");
            string database = Environment.GetEnvironmentVariable("NOTES_DB_NAME");
            string username = Environment.GetEnvironmentVariable("NOTES_WATCHER_USER");
            string password = Environment.GetEnvironmentVariable("NOTES_WATCHER_PASSWORD");

            if (string.IsNullOrWhiteSpace(host))
                host = "localhost";

            if (string.IsNullOrWhiteSpace(port))
                port = "5432";

            if (string.IsNullOrWhiteSpace(database))
                database = "NotesSystemDB";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Не заданы переменные среды для подключения NotesWatcher.");

            RuntimeConnectionString =
                "Host=" + host + ";" +
                "Port=" + port + ";" +
                "Database=" + database + ";" +
                "Username=" + username + ";" +
                "Password=" + password;
        }
    }
}