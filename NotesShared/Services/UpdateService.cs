using NotesShared.Config;
using NotesShared.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

namespace NotesShared.Services
{
    public class UpdateService
    {
        public void CheckUpdate()
        {
            try
            {
                UpdateInfo info = GetUpdateInfo();

                Console.WriteLine("Текущая версия: " + AppConfig.AppVersion);
                Console.WriteLine("Последняя версия на GitHub: " + info.Version);

                if (IsNewVersionAvailable(AppConfig.AppVersion, info.Version))
                {
                    Console.WriteLine("Доступно обновление.");
                    Console.WriteLine("Ссылка: " + info.DownloadUrl);
                }
                else
                {
                    Console.WriteLine("Установлена последняя версия.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка проверки обновления: " + ex.Message);
            }
        }

        public void Update()
        {
            try
            {
                UpdateInfo info = GetUpdateInfo();

                Console.WriteLine("Текущая версия: " + AppConfig.AppVersion);
                Console.WriteLine("Последняя версия на GitHub: " + info.Version);

                if (!IsNewVersionAvailable(AppConfig.AppVersion, info.Version))
                {
                    Console.WriteLine("Обновление не требуется.");
                    return;
                }

                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string zipPath = Path.Combine(appDirectory, "NotesSystem_update.zip");
                string tempDirectory = Path.Combine(appDirectory, "update_temp");
                string batPath = Path.Combine(appDirectory, "update.bat");

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);

                Directory.CreateDirectory(tempDirectory);

                Console.WriteLine("Скачивание обновления...");

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(info.DownloadUrl, zipPath);
                }

                Console.WriteLine("Файл скачан: " + zipPath);
                Console.WriteLine("Распаковка архива...");

                ZipFile.ExtractToDirectory(zipPath, tempDirectory);

                string sourceDirectory = FindFolderWithNewFiles(tempDirectory);

                if (string.IsNullOrWhiteSpace(sourceDirectory))
                {
                    Console.WriteLine("Ошибка: в архиве не найден NotesCli.exe.");
                    return;
                }

                CreateUpdateBat(
                    batPath,
                    sourceDirectory,
                    appDirectory,
                    zipPath,
                    tempDirectory
                );

                Console.WriteLine("Обновление подготовлено.");
                Console.WriteLine("Приложение сейчас закроется, файлы заменятся автоматически.");

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = batPath;
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = appDirectory;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;

                Process.Start(startInfo);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления: " + ex.Message);
            }
        }

        public UpdateInfo GetUpdateInfo()
        {
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(AppConfig.UpdateInfoUrl);

                return ParseUpdateInfo(json);
            }
        }

        public UpdateInfo ParseUpdateInfo(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("version.json пустой.");

            string version = ExtractJsonValue(json, "version");
            string url = ExtractJsonValue(json, "url");

            if (string.IsNullOrWhiteSpace(version))
                throw new Exception("В version.json не найдена версия.");

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("В version.json не найдена ссылка на архив.");

            UpdateInfo info = new UpdateInfo();
            info.Version = version;
            info.DownloadUrl = url;

            return info;
        }

        public string ExtractJsonValue(string json, string key)
        {
            string pattern = "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"";

            Match match = Regex.Match(json, pattern);

            if (!match.Success)
                return "";

            return match.Groups[1].Value;
        }

        public bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            try
            {
                Version current = new Version(currentVersion);
                Version latest = new Version(latestVersion);

                return latest > current;
            }
            catch
            {
                return currentVersion != latestVersion;
            }
        }

        private string FindFolderWithNewFiles(string tempDirectory)
        {
            string[] files = Directory.GetFiles(
                tempDirectory,
                "NotesCli.exe",
                SearchOption.AllDirectories
            );

            if (files.Length == 0)
                return "";

            return Path.GetDirectoryName(files[0]);
        }

        private void CreateUpdateBat(
            string batPath,
            string sourceDirectory,
            string appDirectory,
            string zipPath,
            string tempDirectory)
        {
            string notesCliPath = Path.Combine(appDirectory, "NotesCli.exe");

            string bat = "";

            bat += "@echo off" + Environment.NewLine;
            bat += "chcp 65001 > nul" + Environment.NewLine;
            bat += "echo Обновление NotesSystem..." + Environment.NewLine;
            bat += "echo Ожидание закрытия приложения..." + Environment.NewLine;
            bat += "timeout /t 3 /nobreak > nul" + Environment.NewLine;

            bat += "echo Завершение процессов..." + Environment.NewLine;
            bat += "taskkill /IM NotesCli.exe /F > nul 2> nul" + Environment.NewLine;
            bat += "taskkill /IM NotesWatcher.exe /F > nul 2> nul" + Environment.NewLine;
            bat += "timeout /t 2 /nobreak > nul" + Environment.NewLine;

            bat += "echo Замена файлов..." + Environment.NewLine;
            bat += "xcopy \"" + sourceDirectory + "\\*\" \"" + appDirectory + "\" /E /Y /I" + Environment.NewLine;

            bat += "echo Очистка временных файлов..." + Environment.NewLine;
            bat += "del \"" + zipPath + "\" > nul 2> nul" + Environment.NewLine;
            bat += "rmdir /s /q \"" + tempDirectory + "\" > nul 2> nul" + Environment.NewLine;

            bat += "echo Запуск новой версии..." + Environment.NewLine;
            bat += "start \"\" \"" + notesCliPath + "\"" + Environment.NewLine;

            bat += "echo Обновление завершено." + Environment.NewLine;
            bat += "timeout /t 2 /nobreak > nul" + Environment.NewLine;
            bat += "del \"%~f0\"" + Environment.NewLine;

            File.WriteAllText(batPath, bat, System.Text.Encoding.Default);
        }
    }
}