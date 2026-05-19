using NotesShared.Database;
using NotesShared.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace NotesShared.Services
{
    public class SecurityLogService
    {
        public List<SecurityLog> GetSecurityLogs()
        {
            List<SecurityLog> logs = new List<SecurityLog>();

            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT
                        sl.id,
                        sl.user_id,
                        u.username,
                        sl.event_type,
                        sl.description,
                        sl.created_at
                    FROM security_logs sl
                    LEFT JOIN users u ON sl.user_id = u.id
                    ORDER BY sl.created_at DESC
                    LIMIT 50;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SecurityLog log = new SecurityLog();

                            log.Id = reader.GetInt32(reader.GetOrdinal("id"));
                            log.UserId = reader.GetInt32(reader.GetOrdinal("user_id"));

                            if (reader.IsDBNull(reader.GetOrdinal("username")))
                                log.Username = "unknown";
                            else
                                log.Username = reader.GetString(reader.GetOrdinal("username"));

                            log.EventType = reader.GetString(reader.GetOrdinal("event_type"));
                            log.Description = reader.GetString(reader.GetOrdinal("description"));
                            log.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                            logs.Add(log);
                        }
                    }
                }
            }

            return logs;
        }

        public void AddLog(int userId, string eventType, string description)
        {
            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    INSERT INTO security_logs (user_id, event_type, description, created_at)
                    VALUES (@userId, @eventType, @description, NOW());
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@eventType", eventType);
                    command.Parameters.AddWithValue("@description", description);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}