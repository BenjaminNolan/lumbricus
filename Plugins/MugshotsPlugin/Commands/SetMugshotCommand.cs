using NLog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Exceptions;
using TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Model;
using TwoWholeWorms.Lumbricus.Shared;
using TwoWholeWorms.Lumbricus.Shared.Model;

namespace TwoWholeWorms.Lumbricus.Plugins.MugshotsPlugin.Commands
{

    public class SetMugshotCommand : AbstractCommand
    {

        readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public SetMugshotCommand(IrcConnection conn) : base(conn)
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
                Setting largeImageBasePath    = Setting.Fetch("Mugshots", "LargeImageBasePath");
                Setting originalBasePath      = Setting.Fetch("Mugshots", "OriginalBasePath");
                Setting thumbnailBasePath     = Setting.Fetch("Mugshots", "ThumbnailBasePath");

                Setting maxLargeHeightSetting = Setting.Fetch("Mugshots", "MaxLargeHeight");
                Setting maxLargeWidthSetting  = Setting.Fetch("Mugshots", "MaxLargeWidth");
                Setting thumbWidthSetting     = Setting.Fetch("Mugshots", "ThumbWidth");
                Setting thumbHeightSetting    = Setting.Fetch("Mugshots", "ThumbHeight");

                if (largeImageBasePath == null) {
                    throw new Exception("Mugshots/LargeImageBasePath is missing from Settings table");
                }
                if (originalBasePath == null) {
                    throw new Exception("Mugshots/OriginalBasePath is missing from Settings table");
                }
                if (thumbnailBasePath == null) {
                    throw new Exception("Mugshots/ThumbnailBasePath is missing from Settings table");
                }

                if (maxLargeHeightSetting == null) {
                    throw new Exception("Mugshots/MaxLargeHeight is missing from Settings table");
                }
                if (maxLargeWidthSetting == null) {
                    throw new Exception("Mugshots/MaxLargeWidth is missing from Settings table");
                }
                if (thumbWidthSetting == null) {
                    throw new Exception("Mugshots/ThumbWidth is missing from Settings table");
                }
                if (thumbHeightSetting == null) {
                    throw new Exception("Mugshots/ThumbHeight is missing from Settings table");
                }

                int maxLargeHeight = Int32.Parse(maxLargeHeightSetting.Value);
                int maxLargeWidth  = Int32.Parse(maxLargeWidthSetting.Value);
                int thumbHeight    = Int32.Parse(thumbHeightSetting.Value);
                int thumbWidth     = Int32.Parse(thumbWidthSetting.Value);

                if (maxLargeHeight < 1) {
                    throw new Exception("Mugshots/MaxLargeHeight in Settings table must be > 0");
                }
                if (maxLargeWidth < 1) {
                    throw new Exception("Mugshots/MaxLargeWidth in Settings table must be > 0");
                }
                if (thumbWidth < 1) {
                    throw new Exception("Mugshots/ThumbWidth in Settings table must be > 0");
                }
                if (thumbHeight < 1) {
                    throw new Exception("Mugshots/ThumbHeight in Settings table must be > 0");
                }

                if (!Directory.Exists(originalBasePath.Value)) {
                    throw new Exception($"Directory `{originalBasePath.Value}` does not exist");
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

                Image original = GetImageFromUrl(imageUri);
                if (original == null) {
                    throw new Exception(String.Format("Unable to get image from URI `{0}`", imageUri));
                }

                float fileRatio = ((float)original.Width) / ((float)original.Height);
                logger.Trace("Got image! {0}x{1}, {2}", original.Width, original.Height, fileRatio);

                float maxRatio = thumbWidth / thumbHeight;
                int width = 0;
                int height = 0;
                if (fileRatio < maxRatio) {
                    height = maxLargeHeight;
                    width = (int)Math.Ceiling(height * fileRatio);
                } else {
                    width = maxLargeWidth;
                    height = (int)Math.Ceiling(width / fileRatio);
                }
                logger.Trace("Calculate resize dimensions: {0}x{1}", width, height);

                var rect = new Rectangle(0, 0, width, height);
                var image = new Bitmap(width, height);
                try {
                    image.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                } catch (Exception e) {
                    logger.Debug("image.SetResolution failed.");
                    logger.Trace(e);
                }
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(original, rect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                r = new Regex(@"\.([a-z0-9]{3})$");
                string newFileName = r.Replace(Path.GetRandomFileName(), ".png");
                Image thumb = original.GetThumbnailFixedSize(thumbWidth, thumbHeight, true);

                ImageCodecInfo info = GetEncoderInfo("image/png");
                Encoder encoder = Encoder.Quality;

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(encoder, 100);

                thumb.Save(Path.Combine(thumbnailBasePath.Value, newFileName), info, encoderParams);
                image.Save(Path.Combine(largeImageBasePath.Value, newFileName), info, encoderParams);
                original.Save(Path.Combine(originalBasePath.Value, newFileName), info, encoderParams);

                nick.Account.MostRecentNick = nick;

                Mugshot mugshot = Mugshot.FetchOrCreate(nick.Account);
                mugshot.FileName = newFileName;
                mugshot.OriginalImageUri = imageUri;
                mugshot.IsActive = true;
                mugshot.IsDeleted = false;
                mugshot.LastModifiedAt = DateTime.Now;
                mugshot.Save();

                conn.SendPrivmsg(nick.Name, "Your mugshot has been set! :D");
            } catch (SetMugshotException e) {
                logger.Error(e);
                conn.SendPrivmsg(nick.Name, "Sorry, but I was unable to download your mugshot photo from the URL you provided. Please try another URL, or poke TwoWholeWorms if this error continues.");
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
            try {
                using (var webClient = new WebClient()) {
                    return ByteArrayToImage(webClient.DownloadData(url));
                }
            } catch (Exception e) {
                throw new SetMugshotException(string.Format("Unable to download image from URL `{0}`", url), e);
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
    