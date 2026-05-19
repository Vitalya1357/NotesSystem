using NotesShared.Database;
using NotesShared.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace NotesShared.Services
{
    public class NoteService
    {
        public int AddNote(int userId, string noteText)
        {
            int noteId;

            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql = @"
                    INSERT INTO notes (note_text, user_id, is_deleted, created_at)
                    VALUES (@noteText, @userId, FALSE, NOW())
                    RETURNING id;
                ";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@noteText", noteText);
                    command.Parameters.AddWithValue("@userId", userId);

                    object result = command.ExecuteScalar();
                    noteId = Convert.ToInt32(result);
                }
            }

            SecurityLogService logService = new SecurityLogService();
            logService.AddLog(userId, "NOTE_CREATED", "Создана заметка #" + noteId);

            return noteId;
        }

        public List<Note> GetNotes(int userId, string role)
        {
            List<Note> notes = new List<Note>();

            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql;

                if (role == "admin")
                {
                    sql = @"
                        SELECT id, note_text, user_id, created_at, is_deleted
                        FROM notes
                        ORDER BY id DESC;
                    ";
                }
                else
                {
                    sql = @"
                        SELECT id, note_text, user_id, created_at, is_deleted
                        FROM notes
                        WHERE user_id = @userId
                        ORDER BY id DESC;
                    ";
                }

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Note note = new Note();

                            note.Id = reader.GetInt32(reader.GetOrdinal("id"));
                            note.Text = reader.GetString(reader.GetOrdinal("note_text"));
                            note.UserId = reader.GetInt32(reader.GetOrdinal("user_id"));
                            note.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
                            note.IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted"));

                            notes.Add(note);
                        }
                    }
                }
            }

            return notes;
        }

        public void EditNote(int noteId, int userId, string role, string newText)
        {
            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql;

                if (role == "admin")
                {
                    sql = @"
                        UPDATE notes
                        SET note_text = @newText
                        WHERE id = @noteId;
                    ";
                }
                else
                {
                    sql = @"
                        UPDATE notes
                        SET note_text = @newText
                        WHERE id = @noteId
                          AND user_id = @userId;
                    ";
                }

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@newText", newText);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows == 0)
                        throw new Exception("Заметка не найдена или нет прав на изменение.");
                }
            }

            SecurityLogService logService = new SecurityLogService();
            logService.AddLog(userId, "NOTE_UPDATED", "Изменена заметка #" + noteId);
        }

        public void DeleteNote(int noteId, int userId, string role)
        {
            using (var connection = DatabaseConnection.CreateConnection())
            {
                connection.Open();

                string sql;

                if (role == "admin")
                {
                    sql = @"
                        DELETE FROM notes
                        WHERE id = @noteId;
                    ";
                }
                else
                {
                    sql = @"
                        DELETE FROM notes
                        WHERE id = @noteId
                          AND user_id = @userId;
                    ";
                }

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@userId", userId);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows == 0)
                        throw new Exception("Заметка не найдена или нет прав на удаление.");
                }
            }

            SecurityLogService logService = new SecurityLogService();
            logService.AddLog(userId, "NOTE_DELETED", "Удалена заметка #" + noteId);
        }

        public void RestoreNote(int noteId, int userId, string role)
        {
            throw new Exception("Восстановление невозможно: заметки удаляются физически из базы данных.");
        }
    }
}