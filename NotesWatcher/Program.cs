using System;
using System.Reflection;
using System.Threading;
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
            Console.WriteLine("Каждые 30 секунд будет показываться статистика текущего устройства.");
            Console.WriteLine("Для остановки нажмите Ctrl + C.");
            Console.WriteLine();

            while (true)
            {
                try
                {
                    object metric = metricService.GetLocalStats();

                    Console.WriteLine("Время: " + GetValue(metric, "CreatedAt", "created_at"));
                    Console.WriteLine("Устройство: " + GetValue(metric, "DeviceName", "device_name", "ComputerName"));
                    Console.WriteLine("IP: " + GetValue(metric, "IpAddress", "IPAddress", "ip_address"));
                    Console.WriteLine("CPU: " + GetValue(metric, "CpuUsage", "CPU", "cpu_usage") + "%");
                    Console.WriteLine("RAM: " + GetValue(metric, "RamUsage", "RAM", "ram_usage") + "%");
                    Console.WriteLine("HDD: " + GetValue(metric, "HddUsage", "HDD", "hdd_usage") + "%");
                    Console.WriteLine("--------------------------------");

                    Thread.Sleep(30000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка Watcher: " + ex.Message);
                    Console.WriteLine("--------------------------------");

                    Thread.Sleep(30000);
                }
            }
        }

        static string GetValue(object obj, params string[] names)
        {
            if (obj == null)
                return "";

            Type type = obj.GetType();

            foreach (string name in names)
            {
                PropertyInfo property = type.GetProperty(name);

                if (property != null)
                {
                    object value = property.GetValue(obj, null);

                    if (value == null)
                        return "";

                    return value.ToString();
                }
            }

            return "";
        }
    }
}