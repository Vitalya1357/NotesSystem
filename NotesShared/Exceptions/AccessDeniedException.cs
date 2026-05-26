using System;

namespace NotesShared.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message)
            : base(message)
        {
        }
    }
}