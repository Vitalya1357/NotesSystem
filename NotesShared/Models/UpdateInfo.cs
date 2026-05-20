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