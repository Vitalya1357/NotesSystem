using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NotesShared.Config;

namespace NotesShared.Database
{
    public static class DatabaseConnection
    {
        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(AppConfig.GetConnectionString());
        }
    }
}