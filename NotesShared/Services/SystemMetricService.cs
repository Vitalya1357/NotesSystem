using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualBasic.Devices;
using NotesShared.Models;
using Npgsql;
using NotesShared.Database;

namespace NotesShared.Services
{
    public class SystemMetricService
    {
        public SystemMetric GetLocalStats()
        {
            SystemMetric metric = new SystemMetric();

            metric.DeviceName = Environment.MachineName;
            metric.IpAddress = GetLocalIpAddress();
            metric.CpuUsage = GetCpuUsage();
            metric.RamUsage = GetRamUsage();
            metric.HddUsage = GetHddUsage();
            metric.CreatedAt = DateTime.Now;

            return metric;
        }

        private double GetCpuUsage()
        {
            try
            {
                using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(1000);
                    return Math.Round(cpuCounter.NextValue(), 2);
                }
            }
            catch
            {
                return 0;
            }
        }

        private double GetRamUsage()
        {
            try
            {
                ComputerInfo computerInfo = new ComputerInfo();

                double total = computerInfo.TotalPhysicalMemory;
                double available = computerInfo.AvailablePhysicalMemory;

                double used = total - available;
                double percent = used / total * 100;

                return Math.Round(percent, 2);
            }
            catch
            {
                return 0;
            }
        }

        private double GetHddUsage()
        {
            try
            {
                DriveInfo drive = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.Name == Path.GetPathRoot(Environment.SystemDirectory))
                    .FirstOrDefault();

                if (drive == null)
                    return 0;

                double total = drive.TotalSize;
                double free = drive.AvailableFreeSpace;
                double used = total - free;

                double percent = used / total * 100;

                return Math.Round(percent, 2);
            }
            catch
            {
                return 0;
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();

                IPAddress ip = Dns.GetHostAddresses(hostName)
                    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ip == null)
                    return "unknown";

                return ip.ToString();
            }
            catch
            {
                return "unknown";
            }
        }
        public void SaveMetric(SystemMetric metric)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                int deviceId = GetOrCreateDeviceId(connection, metric.DeviceName, metric.IpAddress);

                string sql = @"
            INSERT INTO system_metrics(device_id, cpu_usage, ram_usage, hdd_usage, created_at)
            VALUES (@device_id, @cpu_usage, @ram_usage, @hdd_usage, @created_at);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("device_id", deviceId);
                    command.Parameters.AddWithValue("cpu_usage", metric.CpuUsage);
                    command.Parameters.AddWithValue("ram_usage", metric.RamUsage);
                    command.Parameters.AddWithValue("hdd_usage", metric.HddUsage);
                    command.Parameters.AddWithValue("created_at", metric.CreatedAt);

                    command.ExecuteNonQuery();
                }
            }
        }

        private int GetOrCreateDeviceId(NpgsqlConnection connection, string deviceName, string ipAddress)
        {
            string selectSql = @"
        SELECT id
        FROM devices
        WHERE device_name = @device_name
        LIMIT 1;";

            using (NpgsqlCommand selectCommand = new NpgsqlCommand(selectSql, connection))
            {
                selectCommand.Parameters.AddWithValue("device_name", deviceName);

                object result = selectCommand.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);
            }

            string insertSql = @"
        INSERT INTO devices(device_name, ip_address)
        VALUES (@device_name, @ip_address)
        RETURNING id;";

            using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertSql, connection))
            {
                insertCommand.Parameters.AddWithValue("device_name", deviceName);
                insertCommand.Parameters.AddWithValue("ip_address", ipAddress);

                object result = insertCommand.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
        public System.Collections.Generic.List<SystemMetric> GetLastMetrics(int limit)
        {
            System.Collections.Generic.List<SystemMetric> metrics = new System.Collections.Generic.List<SystemMetric>();

            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
            SELECT 
                sm.id,
                d.device_name,
                d.ip_address,
                sm.cpu_usage,
                sm.ram_usage,
                sm.hdd_usage,
                sm.created_at
            FROM system_metrics sm
            JOIN devices d ON d.id = sm.device_id
            ORDER BY sm.id DESC
            LIMIT @limit;";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SystemMetric metric = new SystemMetric();

                            metric.DeviceName = reader.GetString(reader.GetOrdinal("device_name"));
                            metric.IpAddress = reader.GetString(reader.GetOrdinal("ip_address"));
                            metric.CpuUsage = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("cpu_usage")));
                            metric.RamUsage = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("ram_usage")));
                            metric.HddUsage = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("hdd_usage")));
                            metric.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                            metrics.Add(metric);
                        }
                    }
                }
            }

            return metrics;
        }
    }
}