using System;
using System.IO;
using Npgsql;
using NotesShared.Config;
using NotesShared.Database;

namespace NotesInstaller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== NotesSystem Installer ===");
            Console.WriteLine("Начало установки...");
            Console.WriteLine();

            string installPath = @"C:\NotesSystem";
            string logsPath = Path.Combine(installPath, "logs");
            string configPath = Path.Combine(installPath, "config");
            string configFile = Path.Combine(configPath, "config.json");

            try
            {
                CreateDirectory(installPath);
                CreateDirectory(logsPath);
                CreateDirectory(configPath);

                CreateConfigFile(configFile);

                CheckDatabaseConnection();

                Console.WriteLine();
                Console.WriteLine("Установка завершена успешно.");
                Console.WriteLine("Папка установки: " + installPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Ошибка установки:");
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
        }

        static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Console.WriteLine("Создана папка: " + path);
            }
            else
            {
                Console.WriteLine("Папка уже существует: " + path);
            }
        }

        static void CreateConfigFile(string configFile)
        {
            string json =
@"{
  ""appName"": ""NotesSystem"",
  ""version"": """ + AppConfig.AppVersion + @""",
  ""database"": ""NotesSystemDB"",
  ""description"": ""Configuration file for NotesSystem""
}";

            File.WriteAllText(configFile, json);
            Console.WriteLine("Создан конфигурационный файл: " + configFile);
        }

        static void CheckDatabaseConnection()
        {
            Console.WriteLine("Проверка подключения к PostgreSQL...");

            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();
                Console.WriteLine("Подключение к базе данных успешно.");
            }
        }
    }
}