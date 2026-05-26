using System;

namespace NotesShared.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
