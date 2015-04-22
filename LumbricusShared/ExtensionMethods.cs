using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System;
using System.Collections.Generic;

namespace TwoWholeWorms.Lumbricus.Shared
{

    public static class ExtensionMethods
    {

        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }

        public static bool IsConnected(this TcpClient tcpClient)
        {
            return (tcpClient.GetState() == TcpState.Established);
        }

        public static Image GetThumbnailFixedSize(this Image image, int Width, int Height, bool needToFill)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int sourceX = 0;
            int sourceY = 0;
            double destX = 0;
            double destY = 0;

            double nScaleW = ((double)Width / (double)sourceWidth);
            double nScaleH = ((double)Height / (double)sourceHeight);

            double nScale;
            if (!needToFill)
            {
                nScale = Math.Min(nScaleH, nScaleW);
            }
            else
            {
                nScale = Math.Max(nScaleH, nScaleW);
                destY = (Height - sourceHeight * nScale) / 2;
                destX = (Width - sourceWidth * nScale) / 2;
            }

            if (nScale > 1)
                nScale = 1;

            int destWidth = (int)Math.Round(sourceWidth * nScale);
            int destHeight = (int)Math.Round(sourceHeight * nScale);

            Bitmap bmPhoto;
            try
            {
                bmPhoto = new Bitmap(destWidth + (int)Math.Round(2 * destX), destHeight + (int)Math.Round(2 * destY));
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("destWidth:{0}, destX:{1}, destHeight:{2}, desxtY:{3}, Width:{4}, Height:{5}",
                    destWidth, destX, destHeight, destY, Width, Height), ex);
            }
            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.CompositingQuality = CompositingQuality.HighQuality;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;

                Rectangle to = new Rectangle((int)Math.Round(destX), (int)Math.Round(destY), destWidth, destHeight);
                Rectangle from = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);
                grPhoto.DrawImage(image, to, from, GraphicsUnit.Pixel);

                return bmPhoto;
            }
        }

        public static string GetStringSafe(this IDataReader reader, int colIndex)
        {
            return GetStringSafe(reader, colIndex, string.Empty);
        }

        public static string GetStringSafe(this IDataReader reader, int colIndex, string defaultValue)
        {
            return !reader.IsDBNull(colIndex) ? reader.GetString(colIndex) : defaultValue;
        }

        public static string GetStringSafe(this IDataReader reader, string indexName)
        {
            return GetStringSafe(reader, reader.GetOrdinal(indexName));
        }

        public static string GetStringSafe(this IDataReader reader, string indexName, string defaultValue)
        {
            return GetStringSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }

        public static long GetInt64Safe(this IDataReader reader, int colIndex)
        {
            return GetInt64Safe(reader, colIndex, 0);
        }

        public static long GetInt64Safe(this IDataReader reader, int colIndex, long defaultValue)
        {
            return (!reader.IsDBNull(colIndex) ? reader.GetInt64(colIndex) : defaultValue);
        }

        public static long GetInt64Safe(this IDataReader reader, string indexName)
        {
            return GetInt64Safe(reader, reader.GetOrdinal(indexName));
        }

        public static long GetInt64Safe(this IDataReader reader, string indexName, long defaultValue)
        {
            return GetInt64Safe(reader, reader.GetOrdinal(indexName), defaultValue);
        }

        public static DateTime? GetDateTimeSafe(this IDataReader reader, int colIndex)
        {
            return GetDateTimeSafe(reader, colIndex, null);
        }

        public static DateTime? GetDateTimeSafe(this IDataReader reader, int colIndex, DateTime? defaultValue)
        {
            try {
                if (!reader.IsDBNull(colIndex)) {
                    return reader.GetString(colIndex) == "0000-00-00 00:00:00" ? defaultValue : reader.GetDateTime(colIndex);
                } else {
                    return defaultValue;
                }
#pragma warning disable 168
            } catch (Exception e) {
#pragma warning restore 168
//                Logger.Log(e);
                return defaultValue;
            }
        }

        public static DateTime? GetDateTimeSafe(this IDataReader reader, string indexName)
        {
            return GetDateTimeSafe(reader, reader.GetOrdinal(indexName));
        }

        public static DateTime? GetDateTimeSafe(this IDataReader reader, string indexName, DateTime? defaultValue)
        {
            return GetDateTimeSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }

        public static string Capitalise(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static T ToEnum<T>(this string enumValue) where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), enumValue);
        }

        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }
        
    }

}
