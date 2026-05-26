using System;
using NotesShared.Exceptions;
using Npgsql;

namespace NotesShared.Utils
{
    public static class DbErrorTranslator
    {
        public static void Execute(Action action)
        {
            try
            {
                action();
            }
            catch (PostgresException ex) when (ex.SqlState == "42501")
            {
                throw new AccessDeniedException("Недостаточно прав для выполнения этой команды.");
            }
        }

        public static T Execute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (PostgresException ex) when (ex.SqlState == "42501")
            {
                throw new AccessDeniedException("Недостаточно прав для выполнения этой команды.");
            }
        }
    }
}