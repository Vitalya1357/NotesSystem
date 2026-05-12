using System;

namespace NotesShared.Models
{
    public class SecurityLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public SecurityLog()
        {
            EventType = "";
            Description = "";
        }
    }
}