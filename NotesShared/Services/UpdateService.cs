using System;
using System.Net;
using System.Web.Script.Serialization;
using NotesShared.Config;
using NotesShared.Models;

namespace NotesShared.Services
{
    public class UpdateService
    {
        public string GetCurrentVersion()
        {
            return AppConfig.AppVersion;
        }

        public UpdateInfo GetLatestVersionInfo()
        {
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(AppConfig.UpdateInfoUrl);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                UpdateInfo info = serializer.Deserialize<UpdateInfo>(json);

                return info;
            }
        }

        public bool IsUpdateAvailable()
        {
            UpdateInfo latestInfo = GetLatestVersionInfo();

            Version current = new Version(AppConfig.AppVersion);
            Version latest = new Version(latestInfo.Version);

            return latest > current;
        }

        public void DownloadUpdate(string downloadUrl, string outputFile)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(downloadUrl, outputFile);
            }
        }
    }
}