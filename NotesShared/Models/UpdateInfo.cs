using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesShared.Models
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }

        public UpdateInfo()
        {
            Version = "";
            DownloadUrl = "";
        }
    }
}
