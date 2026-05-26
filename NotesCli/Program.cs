using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NotesShared.Config;
using NotesShared.Exceptions;
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
        private static UserService userService = new UserService();

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

                        case "--register":
                            Register(parts);
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

                        case "--users":
                            UsersCommand(parts);
                            break;

                        case "--securityLogs":
                            SecurityLogs(parts);
                            break;

                        case "--systemStats":
                            SystemStats(parts);
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

                        case "exit":
                            return;

                        default:
                            Console.WriteLine("Неизвестная команда. Введите --help.");
                            break;
                    }
                }
                catch (AccessDeniedException)
                {
                    Console.WriteLine(GetAccessDeniedMessage(command));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }
            }
        }

        private static string GetAccessDeniedMessage(string command)
        {
            if (command == "--users")
                return "Нет прав для управления пользователями.";

            if (command == "--securityLogs")
                return "Нет прав для просмотра журнала безопасности.";

            if (command == "--systemStats")
                return "Нет прав для просмотра мониторинга.";

            if (command == "--addNewNote" ||
                command == "--listNotes" ||
                command == "--editNote" ||
                command == "--deleteNote" ||
                command == "--restoreNote")
                return "Нет прав для работы с заметками.";

            return "Недостаточно прав для выполнения этой команды.";
        }

        private static void Register(string[] parts)
        {
            if (parts.Length < 3)
            {
                Console.WriteLine("Использование: --register <username> <password>");
                return;
            }

            string username = parts[1];
            string password = parts[2];

            bool result = userService.RegisterUser(username, password);

            if (result)
                Console.WriteLine("Регистрация выполнена. Теперь можно войти через --login.");
            else
                Console.WriteLine("Не удалось зарегистрироваться. Возможно, пользователь уже существует.");
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
                authService.CurrentUser.Id
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
                    "user_id: " + note.UserId + " | " +
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

            int noteId;

            if (!int.TryParse(parts[1], out noteId))
            {
                Console.WriteLine("ID заметки должен быть числом.");
                return;
            }

            string newText = parts[2];

            bool result = noteService.EditNote(
                noteId,
                authService.CurrentUser.Id,
                newText
            );

            if (result)
                Console.WriteLine("Заметка изменена.");
            else
                Console.WriteLine("Заметка не найдена или нет прав.");
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

            int noteId;

            if (!int.TryParse(parts[1], out noteId))
            {
                Console.WriteLine("ID заметки должен быть числом.");
                return;
            }

            bool result = noteService.DeleteNote(
                noteId,
                authService.CurrentUser.Id
            );

            if (result)
                Console.WriteLine("Заметка удалена.");
            else
                Console.WriteLine("Заметка не найдена или нет прав.");
        }

        private static void RestoreNote(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2)
            {
                Console.WriteLine("Использование: --restoreNote <id>");
                return;
            }

            int noteId;

            if (!int.TryParse(parts[1], out noteId))
            {
                Console.WriteLine("ID заметки должен быть числом.");
                return;
            }

            bool result = noteService.RestoreNote(
                noteId,
                authService.CurrentUser.Id
            );

            if (result)
                Console.WriteLine("Заметка восстановлена.");
            else
                Console.WriteLine("Заметка не найдена, уже восстановлена или нет прав.");
        }

        private static void UsersCommand(string[] parts)
        {
            if (!authService.IsLoggedIn)
            {
                Console.WriteLine("Сначала выполните вход.");
                return;
            }

            if (parts.Length < 2)
            {
                Console.WriteLine("Использование:");
                Console.WriteLine("--users list");
                Console.WriteLine("--users add <username> <password> <role>");
                Console.WriteLine("--users delete <username>");
                return;
            }

            string action = parts[1];

            if (action == "list")
            {
                List<UserInfo> users = userService.GetUsers();

                if (users.Count == 0)
                {
                    Console.WriteLine("Пользователей нет.");
                    return;
                }

                foreach (UserInfo user in users)
                {
                    Console.WriteLine(
                    user.Id + " | " +
                    user.Username + " | " +
                    user.Role + " | " +
                    user.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss")
                    );
                }

                return;
            }

            if (action == "add")
            {
                if (parts.Length < 5)
                {
                    Console.WriteLine("Использование: --users add <username> <password> <role>");
                    Console.WriteLine("Роли: admin, user, statistic");
                    return;
                }

                string username = parts[2];
                string password = parts[3];
                string role = parts[4];

                bool result = userService.AddUser(username, password, role);

                if (result)
                    Console.WriteLine("Пользователь добавлен.");
                else
                    Console.WriteLine("Не удалось добавить пользователя.");

                return;
            }

            if (action == "delete")
            {
                if (parts.Length < 3)
                {
                    Console.WriteLine("Использование: --users delete <username>");
                    return;
                }

                string username = parts[2];

                bool result = userService.DeleteUser(username);

                if (result)
                    Console.WriteLine("Пользователь удален/заблокирован.");
                else
                    Console.WriteLine("Пользователь не найден.");

                return;
            }

            Console.WriteLine("Неизвестное действие для --users.");
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
                systemMetricService.EnsureMonitoringAccess();

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

Авторизация:
--register <username> <password>           Регистрация пользователя
--login <username> <password>              Вход в систему
--logout                                   Выход из системы
--myrole                                   Показать текущую роль

Заметки:
--addNewNote ""текст""                     Создать заметку
--listNotes                                Показать заметки
--editNote <id> ""текст""                  Изменить заметку
--deleteNote <id>                          Удалить заметку
--restoreNote <id>                         Восстановить заметку

Пользователи:
--users list                               Показать пользователей
--users add <username> <password> <role>   Добавить пользователя
--users delete <username>                  Удалить/заблокировать пользователя

Мониторинг:
--systemStats local                        Показать CPU/RAM/HDD текущего устройства
--systemStats history                      Показать последние записи статистики из БД

Безопасность:
--securityLogs list                        Показать последние 50 логов безопасности

Обновления:
--version                                  Показать текущую версию
--checkUpdate                              Проверить обновления через GitHub
--update                                   Скачать обновление

Прочее:
--help                                     Показать справку
exit                                       Закрыть приложение

Примеры:
--register newuser password123
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