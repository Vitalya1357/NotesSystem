using System.Collections.Generic;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;

namespace NotesShared.Services
{
    public class NoteService
    {
        public int AddNote(int userId, string text)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = "SELECT add_note(@user_id, @note_text);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("note_text", text);

                    object result = command.ExecuteScalar();
                    return int.Parse(result.ToString());
                }
            }
        }

        public List<Note> GetNotes(int userId)
        {
            List<Note> notes = new List<Note>();

            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT id, user_id, note_text, created_at, updated_at, is_deleted
                    FROM notes
                    WHERE user_id = @user_id
                    ORDER BY id DESC;";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Note note = new Note();

                            note.Id = reader.GetInt32(reader.GetOrdinal("id"));
                            note.UserId = reader.GetInt32(reader.GetOrdinal("user_id"));
                            note.Text = reader.GetString(reader.GetOrdinal("note_text"));
                            note.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                            int updatedAtIndex = reader.GetOrdinal("updated_at");
                            if (!reader.IsDBNull(updatedAtIndex))
                                note.UpdatedAt = reader.GetDateTime(updatedAtIndex);

                            note.IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"));

                            notes.Add(note);
                        }
                    }
                }
            }

            return notes;
        }

        public bool EditNote(int userId, int noteId, string text)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = "SELECT edit_note(@user_id, @note_id, @note_text);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("note_id", noteId);
                    command.Parameters.AddWithValue("note_text", text);

                    object result = command.ExecuteScalar();
                    return bool.Parse(result.ToString());
                }
            }
        }

        public bool DeleteNote(int userId, int noteId)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = "SELECT delete_note(@user_id, @note_id);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("note_id", noteId);

                    object result = command.ExecuteScalar();
                    return bool.Parse(result.ToString());
                }
            }
        }

        public bool RestoreNote(int userId, int noteId)
        {
            using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = "SELECT restore_note(@user_id, @note_id);";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("note_id", noteId);

                    object result = command.ExecuteScalar();
                    return bool.Parse(result.ToString());
                }
            }
        }
    }
}
