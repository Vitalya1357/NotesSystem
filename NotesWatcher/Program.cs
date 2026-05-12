using System;
using System.Threading;
using NotesShared.Models;
using NotesShared.Services;

namespace NotesWatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SystemMetricService metricService = new SystemMetricService();

            Console.WriteLine("=== NotesWatcher Agent ===");
            Console.WriteLine("Агент мониторинга запущен.");
            Console.WriteLine("Каждые 30 секунд статистика будет отправляться в PostgreSQL.");
            Console.WriteLine("Для остановки нажмите Ctrl + C.");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    SystemMetric metric = metricService.GetLocalStats();
                    metricService.SaveMetric(metric);

                    Console.WriteLine(
                        DateTime.Now +
                        " | " + metric.DeviceName +
                        " | CPU: " + metric.CpuUsage + "%" +
                        " | RAM: " + metric.RamUsage + "%" +
                        " | HDD: " + metric.HddUsage + "%"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка агента:");
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(30000);
            }
        }
    }
}