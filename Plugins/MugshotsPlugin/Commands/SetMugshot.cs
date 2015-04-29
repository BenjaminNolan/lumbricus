using NLog;
using System;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Shared;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using TwoWholeWorms.Lumbricus.Shared.Model;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Commands
{

    public class SetMugshot : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public SetMugshot(IrcConnection conn) : base(conn)
        {
            // …
        }

        public override string Name {
            get {
                return "Set Mugshot Command";
            }
        }

        public override void HandleCommand(IrcLine line, Nick nick, Channel channel)
        {
            try {
                Setting largeImageBasePath = Setting.Fetch("Mugshots", "LargeImageBasePath");
                Setting largeImageBaseUri  = Setting.Fetch("Mugshots", "LargeImageBaseUri");
                Setting thumbnailBasePath  = Setting.Fetch("Mugshots", "ThumbnailBasePath");
                Setting thumbnailBaseUri   = Setting.Fetch("Mugshots", "ThumbnailBaseUri");

                if (largeImageBasePath == null) {
                    throw new Exception("Mugshots/LargeImageBasePath is missing from Settings table");
                }
                if (largeImageBaseUri == null) {
                    throw new Exception("Mugshots/LargeImageBaseUri is missing from Settings table");
                }
                if (thumbnailBasePath == null) {
                    throw new Exception("Mugshots/ThumbnailBasePath is missing from Settings table");
                }
                if (thumbnailBaseUri == null) {
                    throw new Exception("Mugshots/ThumbnailBaseUri is missing from Settings table");
                }

                if (!Directory.Exists(largeImageBasePath.Value)) {
                    throw new Exception($"Directory `{largeImageBasePath.Value}` does not exist");
                }
                if (!Directory.Exists(thumbnailBasePath.Value)) {
                    throw new Exception($"Directory `{thumbnailBasePath.Value}` does not exist");
                }

                Seen seen = Seen.Fetch(nick);
                DateTime checkTime = DateTime.Now;
                checkTime.AddDays(-7);
                if (seen.FirstSeenAt == DateTime.MinValue || seen.FirstSeenAt > checkTime) {
                    conn.SendPrivmsg(nick.Name, String.Format("Sorry, {0}, but you aren't allowed to use the mugshots functions yet. :(", nick.DisplayName));
                    return;
                }

                Regex r = new Regex(@"^!setmugshot ?");
                string imageUri = r.Replace(line.Args, "").Trim();
                if (imageUri.Length <= 0) { // Whaaaat??
                    conn.SendPrivmsg(nick.Name, "Usage: !setmugshot <image_url_here>");
                    return;
                }

                r = new Regex(@"^https?://.*\.(png|gif|jpe?g)");
                Match m = r.Match(imageUri);
                if (!m.Success) {
                    conn.SendPrivmsg(nick.Name, "Usage: !setmugshot <image_url_here> - the image must be a PNG, GIF, or JPEG file.");
                    return;
                }

                r = new Regex(@"^https?://(www.)?dropbox.com/.*\?dl=0");
                m = r.Match(imageUri);
                if (m.Success) {
                    imageUri = imageUri.Replace("?dl=0", "?dl=1");
                }

                Image image = GetImageFromUrl(imageUri);
                if (image == null) {
                    throw new Exception(String.Format("Unable to get image from URI `{0}`", imageUri));
                }

                r = new Regex(@"\.([a-z0-9]{3})$");
                string newFileName = r.Replace(Path.GetRandomFileName(), ".png");
                Image thumb = image.GetThumbnailFixedSize(170, 170, true);

                ImageCodecInfo info = GetEncoderInfo("image/png");
                Encoder encoder = Encoder.Quality;
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(encoder, 100);
                thumb.Save(Path.Combine(thumbnailBasePath.Value, newFileName), info, encoderParams);
                image.Save(Path.Combine(largeImageBasePath.Value, newFileName), info, encoderParams);

                nick.Account.MostRecentNick = nick;

                Mugshot mugshot = Mugshot.FetchOrCreate(nick.Account);
                mugshot.OriginalImageUri = imageUri;
                mugshot.ThumbnailUri = thumbnailBaseUri.Value + "/" + newFileName;
                mugshot.LargeUri = largeImageBaseUri.Value + "/" + newFileName;
                mugshot.IsActive = true;
                mugshot.IsDeleted = false;
                mugshot.Save();
            } catch (Exception e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Oof… I shouldn't have eaten that pie, I can't do that right now. :(");
            }
        }

        public static Image ByteArrayToImage(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes)) {
                return Image.FromStream(stream);
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            using (var webClient = new WebClient()) {
                return ByteArrayToImage(webClient.DownloadData(url));
            }
        }

        static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs) {
                if (codec.MimeType == mimeType) {
                    return codec;
                }
            }
            return null;
        }

    }

}
    