using System.Linq;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;
using NotesShared.Utils;
using NotesShared.Config;

namespace NotesShared.Services
{
    public class AuthService
    {
        public User CurrentUser { get; private set; }

        public bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public bool Login(string username, string password)
        {
            string passwordHash = PasswordHasher.Hash(password);

            AppConfig.UseAuthConnection();

            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = "SELECT * FROM login_user(@username, @password_hash);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("username", username);
                    command.Parameters.AddWithValue("password_hash", passwordHash);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CurrentUser = new User();

                            CurrentUser.Id = reader.GetInt32(reader.GetOrdinal("user_id"));
                            CurrentUser.Username = reader.GetString(reader.GetOrdinal("username"));
                            CurrentUser.Role = reader.GetString(reader.GetOrdinal("role_name"));

                            string connectionString = reader.GetString(reader.GetOrdinal("connection_string"));

                            if (string.IsNullOrWhiteSpace(connectionString))
                                return false;

                            AppConfig.UseConnectionString(connectionString);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
            AppConfig.ClearRuntimeConnectionString();
            AppConfig.UseAuthConnection();
        }

        public bool HasRole(params string[] roles)
        {
            if (CurrentUser == null)
                return false;

            return roles.Contains(CurrentUser.Role);
        }
    }
}