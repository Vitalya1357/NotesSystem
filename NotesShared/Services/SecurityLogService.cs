using System.Collections.Generic;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;

namespace NotesShared.Services
{
    public class SecurityLogService
    {
        public List<SecurityLog> GetLastLogs(int limit)
        {
            List<SecurityLog> logs = new List<SecurityLog>();

            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT id, user_id, event_type, description, created_at
                    FROM security_logs
                    ORDER BY id DESC
                    LIMIT @limit;";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("limit", limit);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SecurityLog log = new SecurityLog();

                            log.Id = reader.GetInt32(reader.GetOrdinal("id"));

                            int userIdIndex = reader.GetOrdinal("user_id");
                            if (!reader.IsDBNull(userIdIndex))
                                log.UserId = reader.GetInt32(userIdIndex);

                            log.EventType = reader.GetString(reader.GetOrdinal("event_type"));

                            int descriptionIndex = reader.GetOrdinal("description");
                            if (!reader.IsDBNull(descriptionIndex))
                                log.Description = reader.GetString(descriptionIndex);

                            log.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                            logs.Add(log);
                        }
                    }
                }
            }

            return logs;
        }

        public void AddLog(int? userId, string eventType, string description)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    INSERT INTO security_logs(user_id, event_type, description)
                    VALUES (@user_id, @event_type, @description);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    if (userId.HasValue)
                        command.Parameters.AddWithValue("user_id", userId.Value);
                    else
                        command.Parameters.AddWithValue("user_id", System.DBNull.Value);

                    command.Parameters.AddWithValue("event_type", eventType);
                    command.Parameters.AddWithValue("description", description);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}