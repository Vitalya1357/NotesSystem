using System.Linq;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;
using NotesShared.Utils;

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
                            CurrentUser = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("user_id")),
                                Username = reader.GetString(reader.GetOrdinal("username")),
                                Role = reader.GetString(reader.GetOrdinal("role_name"))
                            };

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
        }

        public bool HasRole(params string[] roles)
        {
            if (CurrentUser == null)
                return false;

            return roles.Contains(CurrentUser.Role);
        }
    }
}
