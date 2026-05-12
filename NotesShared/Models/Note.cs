using System;

namespace NotesShared.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public Note()
        {
            Text = "";
        }
    }
}
