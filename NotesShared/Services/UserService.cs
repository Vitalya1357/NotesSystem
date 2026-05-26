using System;
using System.Collections.Generic;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;
using NotesShared.Utils;

namespace NotesShared.Services
{
    public class UserService
    {
        public bool RegisterUser(string username, string password)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                string passwordHash = PasswordHasher.Hash(password);

                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT register_user(@username, @password_hash);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("username", username);
                        command.Parameters.AddWithValue("password_hash", passwordHash);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }

        public List<UserInfo> GetUsers()
        {
            return DbErrorTranslator.Execute<List<UserInfo>>(() =>
            {
                List<UserInfo> users = new List<UserInfo>();

                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT * FROM get_users();";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserInfo user = new UserInfo();

                                user.Id = reader.GetInt32(reader.GetOrdinal("user_id"));
                                user.Username = reader.GetString(reader.GetOrdinal("username"));
                                user.Role = reader.GetString(reader.GetOrdinal("role_name"));
                                user.IsBlocked = reader.GetBoolean(reader.GetOrdinal("is_blocked"));
                                user.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                                users.Add(user);
                            }
                        }
                    }
                }

                return users;
            });
        }

        public bool AddUser(string username, string password, string role)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                string passwordHash = PasswordHasher.Hash(password);

                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT add_user(@username, @password_hash, @role_name);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("username", username);
                        command.Parameters.AddWithValue("password_hash", passwordHash);
                        command.Parameters.AddWithValue("role_name", role);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }

        public bool DeleteUser(string username)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT delete_user(@username);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }
    }
}