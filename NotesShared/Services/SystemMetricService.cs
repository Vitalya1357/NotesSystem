using NotesShared.Database;
using NotesShared.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace NotesShared.Services
{
    public class SystemMetricService
    {
        public SystemMetric GetLocalStats()
        {
            string deviceName = Environment.MachineName;
            string ipAddress = GetLocalIpAddress();

            double cpuUsage = GetCpuUsage();
            double ramUsage = GetRamUsage();
            double hddUsage = GetHddUsage();

            return new SystemMetric
            {
                DeviceName = deviceName,
                IpAddress = ipAddress,
                CpuUsage = cpuUsage,
                RamUsage = ramUsage,
                HddUsage = hddUsage,
                CreatedAt = DateTime.Now
            };
        }

        public void SaveLocalStats()
        {
            SystemMetric metric = GetLocalStats();

            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                int deviceId = GetOrCreateDeviceId(
                    connection,
                    metric.DeviceName,
                    metric.IpAddress
                );

                string sql = @"
                    INSERT INTO system_metrics
                        (device_id, cpu_usage, ram_usage, hdd_usage, created_at)
                    VALUES
                        (@deviceId, @cpuUsage, @ramUsage, @hddUsage, @createdAt);
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@deviceId", deviceId);
                    command.Parameters.AddWithValue("@cpuUsage", metric.CpuUsage);
                    command.Parameters.AddWithValue("@ramUsage", metric.RamUsage);
                    command.Parameters.AddWithValue("@hddUsage", metric.HddUsage);
                    command.Parameters.AddWithValue("@createdAt", metric.CreatedAt);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<SystemMetric> GetHistory()
        {
            List<SystemMetric> metrics = new List<SystemMetric>();

            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT
                        d.device_name AS device_name,
                        d.ip_address,
                        sm.cpu_usage,
                        sm.ram_usage,
                        sm.hdd_usage,
                        sm.created_at
                    FROM system_metrics sm
                    JOIN devices d ON sm.device_id = d.id
                    ORDER BY sm.created_at DESC
                    LIMIT 50;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SystemMetric metric = new SystemMetric
                            {
                                DeviceName = reader.GetString(reader.GetOrdinal("device_name")),
                                IpAddress = reader.GetString(reader.GetOrdinal("ip_address")),
                                CpuUsage = Convert.ToDouble(reader["cpu_usage"]),
                                RamUsage = Convert.ToDouble(reader["ram_usage"]),
                                HddUsage = Convert.ToDouble(reader["hdd_usage"]),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                            };

                            metrics.Add(metric);
                        }
                    }
                }
            }

            return metrics;
        }

        private int GetOrCreateDeviceId(NpgsqlConnection connection, string deviceName, string ipAddress)
        {
            string selectSql = @"
                SELECT id
                FROM devices
                WHERE name = @name
                LIMIT 1;
            ";

            using (var command = new NpgsqlCommand(selectSql, connection))
            {
                command.Parameters.AddWithValue("@name", deviceName);

                object result = command.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);
            }

            string insertSql = @"
                INSERT INTO devices (name, ip_address)
                VALUES (@name, @ipAddress)
                RETURNING id;
            ";

            using (var command = new NpgsqlCommand(insertSql, connection))
            {
                command.Parameters.AddWithValue("@name", deviceName);
                command.Parameters.AddWithValue("@ipAddress", ipAddress);

                object result = command.ExecuteScalar();

                return Convert.ToInt32(result);
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

                IPAddress address = addresses.FirstOrDefault(
                    ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                );

                if (address != null)
                    return address.ToString();

                return "unknown";
            }
            catch
            {
                return "unknown";
            }
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
                using (PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    float availableMb = ramCounter.NextValue();
                    double totalMb = GetTotalMemoryInMegabytes();

                    if (totalMb <= 0)
                        return 0;

                    double usedPercent = 100 - ((availableMb / totalMb) * 100);

                    return Math.Round(usedPercent, 2);
                }
            }
            catch
            {
                return 0;
            }
        }

        private double GetTotalMemoryInMegabytes()
        {
            try
            {
                return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024.0 / 1024.0;
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
                    .FirstOrDefault(d => d.IsReady && d.Name.StartsWith("C"));

                if (drive == null)
                    return 0;

                double total = drive.TotalSize;
                double free = drive.TotalFreeSpace;

                double usedPercent = 100 - ((free / total) * 100);

                return Math.Round(usedPercent, 2);
            }
            catch
            {
                return 0;
            }
        }
    }
}