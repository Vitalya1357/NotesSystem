using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesShared.Config
{
    public static class AppConfig
    {
        public static string ConnectionString =
            "Host=localhost;Port=5432;Database=NotesSystemDB;Username=vitalya;Password=12345";

        public static string AppVersion = "1.0.0";
    }
}
