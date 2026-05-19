using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NotesShared.Config;
using NotesShared.Models;
using NotesShared.Services;

namespace NotesCli
{
    internal class Program
    {
        private static AuthService authService = new AuthService();
        private static NoteService noteService = new NoteService();
        private static SecurityLogService securityLogService = new SecurityLogService();
        private static SystemMetricService systemMetricService = new SystemMetricService();
        private static UpdateService updateService = new UpdateService();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AppConfig.UseAuthConnection();

            StartWatcherConsole();

            Console.WriteLine("=== NotesSystem CLI ===");
            Console.WriteLine("Введите --help для просмотра команд.");

            while (true)
            {
                Console.Write("> ");

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = SplitCommand(input);
                string command = parts[0];

                try
                {
                    switch (command)
                    {
                        case "--help":
                            ShowHelp();
                            break;

                        case "--login":
                            Login(parts);
                            break;

                        case "--logout":
                            Logout();
                            break;

                        case "--myrole":
                            ShowMyRole();
                            break;

                        case "--addNewNote":
                            AddNewNote(parts);
                            break;

                        case "--listNotes":
                            ListNotes();
                            break;

                        case "--editNote":
                            EditNote(parts);
                            break;

                        case "--deleteNote":
                            DeleteNote(parts);
                            break;

                        case "--restoreNote":
                            RestoreNote(parts);
                            break;

                        case "--checkUpdate":
                            updateService.CheckUpdate();
                            break;

                        case "--update":
                            updateService.Update();
                            break;

                        case "--version":
                            Console.WriteLine("Текущая версия: " + AppConfig.AppVersion);
                            break;

                        case "--securityLogs":
                            SecurityLogs(parts);
                            break;

                        case "--systemStats":
                            SystemStats(parts);
                            break;

                        case "exit":
                            return;

                        default:
                            Console.WriteLine("Неизвестная команда. Введите --help.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message.ToLower();

                    if (message.Contains("permission denied") ||
                        message.Contains("отказано в доступе") ||
                        message.Contains("нет доступа") ||
                        message.Contains("42501"))
                    {
                        Console.WriteLine("Нет прав на выполнение операции в PostgreSQL.");
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: " + ex.Message);
                    }
                }
            }
        }

        private static void Login(string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Использование: --login <username> <password>");
                return;
            }

            bool loginResult = authService.Login(parts[1], parts[2]);

            if (loginResult)
            {
                Console.WriteLine("Добро пожаловать, " + authService.CurrentUser.Username + "!");
                Console.WriteLine("Ваша роль: " + authService.CurrentUser.Role);
            }
            else
            {
                Console.WriteLine("Ошибка входа. Неверный логин или пароль.");
            }
        }

        private static void Logout()
        {
            authService.Logout();
            AppConfig.UseAuthConnection();

            Console.WriteLine("Вы вышли из системы.");
        }

        private static void ShowMyRole()
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            Console.WriteLine("Ваша роль: " + authService.CurrentUser.Role);
        }

        private static void AddNewNote(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: --addNewNote \"Текст заметки\"");
                return;
            }

            int newNoteId = noteService.AddNote(
                authService.CurrentUser.Id,
                parts[1]
            );

            Console.WriteLine("Заметка создана. ID: " + newNoteId);
        }

        private static void ListNotes()
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            List<Note> notes = noteService.GetNotes(
                authService.CurrentUser.Id,
                authService.CurrentUser.Role
            );

            if (notes.Count == 0)
            {
                Console.WriteLine("Заметок нет.");
                return;
            }

            foreach (Note note in notes)
            {
                Console.WriteLine(
                    note.Id + " | " +
                    note.Text + " | " +
                    note.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss") + " | " +
                    "deleted: " + note.IsDeleted
                );
            }
        }

        private static void EditNote(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 3)
            {
                Console.WriteLine("Использование: --editNote <id> \"Новый текст\"");
                return;
            }

            int noteId = int.Parse(parts[1]);
            string newText = parts[2];

            noteService.EditNote(
                noteId,
                authService.CurrentUser.Id,
                authService.CurrentUser.Role,
                newText
            );

            Console.WriteLine("Заметка изменена.");
        }

        private static void DeleteNote(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: --deleteNote <id>");
                return;
            }

            int noteId = int.Parse(parts[1]);

            noteService.DeleteNote(
                noteId,
                authService.CurrentUser.Id,
                authService.CurrentUser.Role
            );

            Console.WriteLine("Заметка удалена.");
        }

        private static void RestoreNote(string[] parts)
        {
            Console.WriteLine("Восстановление невозможно: заметки удаляются физически из базы данных.");
        }

        private static void ShowVersion()
        {
            Console.WriteLine("Текущая версия: " + AppConfig.AppVersion);
        }

        private static void CheckUpdate()
        {
            updateService.CheckUpdate();
        }

        private static void Update()
        {
            updateService.Update();
        }

        private static void SecurityLogs(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2 || parts[1] != "list")
            {
                Console.WriteLine("Использование: --securityLogs list");
                return;
            }

            List<SecurityLog> logs = securityLogService.GetSecurityLogs();

            if (logs.Count == 0)
            {
                Console.WriteLine("Логов безопасности нет.");
                return;
            }

            foreach (SecurityLog log in logs)
            {
                Console.WriteLine(
                    log.Id + " | " +
                    log.Username + " | " +
                    log.EventType + " | " +
                    log.Description + " | " +
                    log.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss")
                );
            }
        }

        private static void SystemStats(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: --systemStats local или --systemStats history");
                return;
            }

            if (parts[1] == "local")
            {
                SystemMetric metric = systemMetricService.GetLocalStats();

                Console.WriteLine(
                    metric.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss") + " | " +
                    metric.DeviceName + " | " +
                    "IP: " + metric.IpAddress + " | " +
                    "CPU: " + metric.CpuUsage + "% | " +
                    "RAM: " + metric.RamUsage + "% | " +
                    "HDD: " + metric.HddUsage + "%"
                );

                return;
            }

            if (parts[1] == "history")
            {
                List<SystemMetric> metrics = systemMetricService.GetHistory();

                if (metrics.Count == 0)
                {
                    Console.WriteLine("История статистики пуста.");
                    return;
                }

                foreach (SystemMetric metric in metrics)
                {
                    Console.WriteLine(
                        metric.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss") + " | " +
                        metric.DeviceName + " | " +
                        "IP: " + metric.IpAddress + " | " +
                        "CPU: " + metric.CpuUsage + "% | " +
                        "RAM: " + metric.RamUsage + "% | " +
                        "HDD: " + metric.HddUsage + "%"
                    );
                }

                return;
            }

            Console.WriteLine("Неизвестный параметр. Использование: --systemStats local или --systemStats history");
        }

        private static object CallFirstExistingMethod(object service, string[] methodNames)
        {
            Type type = service.GetType();

            foreach (string methodName in methodNames)
            {
                MethodInfo method = type.GetMethod(methodName, Type.EmptyTypes);

                if (method != null)
                {
                    return method.Invoke(service, null);
                }
            }

            return null;
        }

        private static void PrintCollection(object result)
        {
            IEnumerable collection = result as IEnumerable;

            if (collection == null || result is string)
            {
                PrintObject(result);
                return;
            }

            bool hasItems = false;

            foreach (object item in collection)
            {
                hasItems = true;
                PrintObject(item);
            }

            if (!hasItems)
            {
                Console.WriteLine("Данных нет.");
            }
        }

        private static void PrintObject(object obj)
        {
            if (obj == null)
            {
                Console.WriteLine("Данных нет.");
                return;
            }

            PropertyInfo[] properties = obj.GetType().GetProperties();

            string line = "";

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj, null);

                if (line.Length > 0)
                    line += " | ";

                line += property.Name + ": " + value;
            }

            Console.WriteLine(line);
        }

        private static void StartWatcherConsole()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string watcherPath = Path.Combine(baseDirectory, "NotesWatcher.exe");

                if (!File.Exists(watcherPath))
                    return;

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = watcherPath;
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = baseDirectory;

                Process.Start(startInfo);

                Console.WriteLine("Открыта вторая консоль NotesWatcher.");
            }
            catch
            {
                Console.WriteLine("Не удалось открыть вторую консоль NotesWatcher.");
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine(@"
Доступные команды:
--login <username> <password>           Вход в систему
--logout                                Выход из системы
--myrole                                Показать текущую роль
--addNewNote ""текст""                   Создать заметку
--listNotes                             Показать свои заметки
--editNote <id> ""текст""                Изменить заметку
--deleteNote <id>                       Удалить заметку
--restoreNote <id>                      Восстановить заметку
--help                                  Показать справку
--version                               Показать текущую версию
--checkUpdate                           Проверить обновления через GitHub
--update                                Скачать обновление
--securityLogs list                     Показать последние 50 логов безопасности
--systemStats local                     Показать CPU/RAM/HDD текущего устройства
--systemStats history                   Показать последние записи статистики из БД
exit                                    Закрыть приложение

Примеры:
--login admin admin123
--login user1 admin123
--login stat admin123
");
        }

        private static string[] SplitCommand(string input)
        {
            List<string> result = new List<string>();
            bool insideQuotes = false;
            string current = "";

            foreach (char c in input)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                    continue;
                }

                if (c == ' ' && !insideQuotes)
                {
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        result.Add(current);
                        current = "";
                    }
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
                result.Add(current);

            return result.ToArray();
        }
    }
}