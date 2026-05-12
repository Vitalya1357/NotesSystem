using System;
using System.Collections.Generic;
using NotesShared.Services;

namespace NotesCli
{
    internal class Program
    {
        private static AuthService authService = new AuthService();
        private static NoteService noteService = new NoteService();
        private static SecurityLogService securityLogService = new SecurityLogService();
        private static SystemMetricService systemMetricService = new SystemMetricService();
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== NotesSystem CLI ===");
            Console.WriteLine("Введите --help для просмотра команд.");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input == "exit")
                    break;

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
                            if (parts.Length < 3)
                            {
                                Console.WriteLine("Использование: --login <username> <password>");
                                break;
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

                            break;

                        case "--logout":
                            authService.Logout();
                            Console.WriteLine("Вы вышли из системы.");
                            break;

                        case "--myrole":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            Console.WriteLine("Ваша роль: " + authService.CurrentUser.Role);
                            break;
                        case "--addNewNote":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "user"))
                            {
                                Console.WriteLine("Нет прав для создания заметок.");
                                break;
                            }

                            if (parts.Length < 2)
                            {
                                Console.WriteLine("Использование: --addNewNote \"Текст заметки\"");
                                break;
                            }

                            int newNoteId = noteService.AddNote(authService.CurrentUser.Id, parts[1]);
                            Console.WriteLine("Заметка создана. ID: " + newNoteId);
                            break;


                        case "--listNotes":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "user"))
                            {
                                Console.WriteLine("Нет прав для просмотра заметок.");
                                break;
                            }

                            List<NotesShared.Models.Note> notes = noteService.GetNotes(authService.CurrentUser.Id);

                            if (notes.Count == 0)
                            {
                                Console.WriteLine("Заметок нет.");
                                break;
                            }

                            foreach (NotesShared.Models.Note note in notes)
                            {
                                string status = note.IsDeleted ? "удалена" : "активна";
                                Console.WriteLine("[" + note.Id + "] " + note.CreatedAt + " | " + status);
                                Console.WriteLine(note.Text);
                                Console.WriteLine();
                            }

                            break;


                        case "--editNote":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "user"))
                            {
                                Console.WriteLine("Нет прав для редактирования заметок.");
                                break;
                            }

                            if (parts.Length < 3)
                            {
                                Console.WriteLine("Использование: --editNote <id> \"Новый текст\"");
                                break;
                            }

                            int editId;
                            if (!int.TryParse(parts[1], out editId))
                            {
                                Console.WriteLine("Некорректный ID заметки.");
                                break;
                            }

                            bool editResult = noteService.EditNote(authService.CurrentUser.Id, editId, parts[2]);

                            if (editResult)
                                Console.WriteLine("Заметка обновлена.");
                            else
                                Console.WriteLine("Заметка не найдена или уже удалена.");

                            break;


                        case "--deleteNote":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "user"))
                            {
                                Console.WriteLine("Нет прав для удаления заметок.");
                                break;
                            }

                            if (parts.Length < 2)
                            {
                                Console.WriteLine("Использование: --deleteNote <id>");
                                break;
                            }

                            int deleteId;
                            if (!int.TryParse(parts[1], out deleteId))
                            {
                                Console.WriteLine("Некорректный ID заметки.");
                                break;
                            }

                            bool deleteResult = noteService.DeleteNote(authService.CurrentUser.Id, deleteId);

                            if (deleteResult)
                                Console.WriteLine("Заметка удалена.");
                            else
                                Console.WriteLine("Заметка не найдена или уже удалена.");

                            break;


                        case "--restoreNote":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "user"))
                            {
                                Console.WriteLine("Нет прав для восстановления заметок.");
                                break;
                            }

                            if (parts.Length < 2)
                            {
                                Console.WriteLine("Использование: --restoreNote <id>");
                                break;
                            }

                            int restoreId;
                            if (!int.TryParse(parts[1], out restoreId))
                            {
                                Console.WriteLine("Некорректный ID заметки.");
                                break;
                            }

                            bool restoreResult = noteService.RestoreNote(authService.CurrentUser.Id, restoreId);

                            if (restoreResult)
                                Console.WriteLine("Заметка восстановлена.");
                            else
                                Console.WriteLine("Заметка не найдена или не была удалена.");

                            break;
                        case "--securityLogs":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin"))
                            {
                                Console.WriteLine("Нет прав для просмотра логов безопасности.");
                                break;
                            }

                            if (parts.Length < 2)
                            {
                                Console.WriteLine("Использование: --securityLogs list");
                                break;
                            }

                            if (parts[1] == "list")
                            {
                                List<NotesShared.Models.SecurityLog> logs = securityLogService.GetLastLogs(50);

                                if (logs.Count == 0)
                                {
                                    Console.WriteLine("Логов безопасности нет.");
                                    break;
                                }

                                Console.WriteLine("Логи безопасности:");

                                foreach (NotesShared.Models.SecurityLog log in logs)
                                {
                                    string userIdText = log.UserId.HasValue ? log.UserId.Value.ToString() : "-";

                                    Console.WriteLine(
                                        "[" + log.Id + "] " +
                                        log.CreatedAt + " | UserId: " + userIdText +
                                        " | " + log.EventType +
                                        " | " + log.Description
                                    );
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неизвестное действие. Использование: --securityLogs list");
                            }

                            break;
                        case "--systemStats":
                            if (!authService.IsLoggedIn)
                            {
                                Console.WriteLine("Сначала выполните вход.");
                                break;
                            }

                            if (!authService.HasRole("admin", "statistic"))
                            {
                                Console.WriteLine("Нет прав для просмотра статистики.");
                                break;
                            }

                            if (parts.Length < 2)
                            {
                                Console.WriteLine("Использование: --systemStats local");
                                break;
                            }

                            if (parts[1] == "local")
                            {
                                NotesShared.Models.SystemMetric metric = systemMetricService.GetLocalStats();

                                Console.WriteLine("Статистика устройства:");
                                Console.WriteLine("Устройство: " + metric.DeviceName);
                                Console.WriteLine("IP: " + metric.IpAddress);
                                Console.WriteLine("CPU: " + metric.CpuUsage + "%");
                                Console.WriteLine("RAM: " + metric.RamUsage + "%");
                                Console.WriteLine("HDD: " + metric.HddUsage + "%");
                                Console.WriteLine("Дата: " + metric.CreatedAt);
                            }
                            else if (parts[1] == "history")
                            {
                                List<NotesShared.Models.SystemMetric> metrics = systemMetricService.GetLastMetrics(20);

                                if (metrics.Count == 0)
                                {
                                    Console.WriteLine("История статистики пуста.");
                                    break;
                                }

                                Console.WriteLine("Последние записи статистики:");

                                foreach (NotesShared.Models.SystemMetric metric in metrics)
                                {
                                    Console.WriteLine(
                                        metric.CreatedAt +
                                        " | " + metric.DeviceName +
                                        " | IP: " + metric.IpAddress +
                                        " | CPU: " + metric.CpuUsage + "%" +
                                        " | RAM: " + metric.RamUsage + "%" +
                                        " | HDD: " + metric.HddUsage + "%"
                                    );
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неизвестное действие. Использование: --systemStats local или --systemStats history");
                            }

                            break;

                        default:
                            Console.WriteLine("Неизвестная команда. Введите --help.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка:");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("--login <username> <password>       Вход в систему");
            Console.WriteLine("--logout                            Выход из системы");
            Console.WriteLine("--myrole                            Показать текущую роль");
            Console.WriteLine("--addNewNote \"текст\"               Создать заметку");
            Console.WriteLine("--listNotes                         Показать свои заметки");
            Console.WriteLine("--editNote <id> \"текст\"            Изменить заметку");
            Console.WriteLine("--deleteNote <id>                   Удалить заметку");
            Console.WriteLine("--restoreNote <id>                  Восстановить заметку");
            Console.WriteLine("--help                              Показать справку");
            Console.WriteLine("--securityLogs list                 Показать последние 50 логов безопасности");
            Console.WriteLine("--systemStats local                 Показать CPU/RAM/HDD текущего устройства");
            Console.WriteLine("--systemStats history               Показать последние записи статистики из БД");
            Console.WriteLine("exit                                Закрыть приложение");
            Console.WriteLine();
            Console.WriteLine("Примеры:");
            Console.WriteLine("--login admin admin123");
            Console.WriteLine("--login user1 admin123");
            Console.WriteLine("--login stat admin123");
        }

        static string[] SplitCommand(string input)
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