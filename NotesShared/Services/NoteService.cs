using System;
using System.Collections.Generic;
using Npgsql;
using NotesShared.Database;
using NotesShared.Models;
using NotesShared.Utils;

namespace NotesShared.Services
{
    public class NoteService
    {
        public int AddNote(int userId, string noteText)
        {
            return DbErrorTranslator.Execute<int>(() =>
            {
                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT add_note(@user_id, @note_text);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("user_id", userId);
                        command.Parameters.AddWithValue("note_text", noteText);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            });
        }

        public List<Note> GetNotes(int userId)
        {
            return DbErrorTranslator.Execute<List<Note>>(() =>
            {
                List<Note> notes = new List<Note>();

                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT * FROM get_notes(@user_id);";

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
                                note.IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"));
                                note.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

                                int updatedAtIndex = reader.GetOrdinal("updated_at");
                                if (!reader.IsDBNull(updatedAtIndex))
                                    note.UpdatedAt = reader.GetDateTime(updatedAtIndex);

                                notes.Add(note);
                            }
                        }
                    }
                }

                return notes;
            });
        }

        public bool EditNote(int noteId, int userId, string newText)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT edit_note(@note_id, @user_id, @note_text);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("note_id", noteId);
                        command.Parameters.AddWithValue("user_id", userId);
                        command.Parameters.AddWithValue("note_text", newText);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }

        public bool DeleteNote(int noteId, int userId)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT delete_note(@note_id, @user_id);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("note_id", noteId);
                        command.Parameters.AddWithValue("user_id", userId);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }

        public bool RestoreNote(int noteId, int userId)
        {
            return DbErrorTranslator.Execute<bool>(() =>
            {
                using (NpgsqlConnection connection = DatabaseConnection.CreateConnection())
                {
                    connection.Open();

                    string sql = "SELECT restore_note(@note_id, @user_id);";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("note_id", noteId);
                        command.Parameters.AddWithValue("user_id", userId);

                        return Convert.ToBoolean(command.ExecuteScalar());
                    }
                }
            });
        }
    }
}