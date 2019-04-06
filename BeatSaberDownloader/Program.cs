using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace BeatSaberDownloader
{
    class Program
    {
        const string UriScheme = "modsaber";
        const string FriendlyName = "Custom ModSaber loader";

        const string BeatSaverLocation = @"G:\SteamLibrary\steamapps\common\Beat Saber";

        public static void RegisterUriScheme()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                string applicationLocation = typeof(Program).Assembly.Location[..^4] + ".exe";

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
                RegisterUriScheme();

            else if (args.Length == 1)
            {
                if (Uri.TryCreate(args[0], UriKind.Absolute, out var uri) &&
                    string.Equals(uri.Scheme, UriScheme, StringComparison.OrdinalIgnoreCase))
                {
                    var path = uri.LocalPath.Substring(1);
                    Uri downloadUri;
                    switch (uri.Authority.ToLowerInvariant())
                    {
                        case "song":
                            //https://beatsaver.com/storage/songs/134/134-78.zip
                            var subfolder = path.Split('-')[0];
                            downloadUri = new Uri($"https://beatsaver.com/storage/songs/{subfolder}/{path}.zip");


                            try
                            {
                                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(downloadUri);
                                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                                using (ZipArchive zip = new ZipArchive(resp.GetResponseStream()))
                                {
                                    zip.ExtractToDirectory(Path.Join(BeatSaverLocation, "CustomSongs"));
                                }
                            }
                            catch
                            {

                            }

                            break;
                        case "avatar":
                            downloadUri = new Uri(path);
                            if (!downloadUri.IsFile)
                            {
                                var output = Path.Join(BeatSaverLocation, "CustomAvatars", Path.GetFileName(downloadUri.LocalPath));
                                if (File.Exists(output)) File.Move(output, $"{output}.old");
                                using (var wc = new WebClient()) { wc.DownloadFile(downloadUri, output); }
                            }
                            break;

                        case "saber":
                            downloadUri = new Uri(path);
                            if (!downloadUri.IsFile)
                            {
                                var output = Path.Join(BeatSaverLocation, "CustomSabers", Path.GetFileName(downloadUri.LocalPath));
                                if (File.Exists(output)) File.Move(output, $"{output}.old");
                                using (var wc = new WebClient()) { wc.DownloadFile(downloadUri, output); }
                            }
                            break;

                        case "platform":
                            downloadUri = new Uri(path);
                            if (!downloadUri.IsFile)
                            {
                                var output = Path.Join(BeatSaverLocation, "CustomPlatforms", Path.GetFileName(downloadUri.LocalPath));
                                if (File.Exists(output)) File.Move(output, $"{output}.old");
                                using (var wc = new WebClient()) { wc.DownloadFile(downloadUri, output); }
                            }
                            break;
                    }
                }
            }
        }
    }
}
